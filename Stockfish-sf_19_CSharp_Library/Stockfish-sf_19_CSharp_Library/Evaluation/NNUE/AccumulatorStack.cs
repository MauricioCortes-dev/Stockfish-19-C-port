/*
  Stockfish, a UCI chess playing engine derived from Glaurung 2.1
  Copyright (C) 2004-2026 The Stockfish developers (see AUTHORS file)

  Stockfish is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  Stockfish is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Numerics;
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.Types;
using Half = Stockfish_sf_19_CSharp_Library.Evaluation.NNUE.Features.HalfKAv2_hm;
using Threats = Stockfish_sf_19_CSharp_Library.Evaluation.NNUE.Features.FullThreats;
using Pairs = Stockfish_sf_19_CSharp_Library.Evaluation.NNUE.Features.PP_3Wide;

namespace Stockfish_sf_19_CSharp_Library.Evaluation.NNUE
{
    /// <summary>Per-worker lazy accumulator stack. Push before DoMove and pop after UndoMove.</summary>
    public sealed class AccumulatorStack
    {
        public const int MaxSize = ChessConstants.MAX_PLY + 1;
        private readonly AccumulatorState[] accumulators = new AccumulatorState[MaxSize];
        private readonly Piece[] hybridPrevious = new Piece[64];
        public int Size { get; private set; } = 1;
        public AccumulatorState Latest => accumulators[Size - 1];

        public AccumulatorStack()
        {
            for (int i = 0; i < MaxSize; ++i)
                accumulators[i] = new AccumulatorState();
        }

        public AccumulatorStack(AccumulatorStack other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            Size = other.Size;
            for (int i = 0; i < MaxSize; ++i)
                accumulators[i] = new AccumulatorState(other.accumulators[i]);
        }

        public AccumulatorState this[int index]
        {
            get
            {
                if ((uint)index >= Size)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return accumulators[index];
            }
        }

        public void Reset()
        {
            accumulators[0].Dirties = default;
            Array.Clear(accumulators[0].Computed, 0, 2);
            Size = 1;
        }

        public ref Dirties Push()
        {
            if (Size >= MaxSize)
                throw new InvalidOperationException("Accumulator stack is full.");
            var state = accumulators[Size++];
            Array.Clear(state.Computed, 0, 2);
            state.Dirties.dirtyThreats = default;
            state.Dirties.dirtyPawnPairs = default;
            return ref state.Dirties;
        }

        public void Pop()
        {
            if (Size <= 1)
                throw new InvalidOperationException("Cannot pop the root accumulator.");
            --Size;
        }

        public void Evaluate(Position pos, FeatureTransformer ft, AccumulatorCaches cache)
        {
            if (pos == null || ft == null || cache == null)
                throw new ArgumentNullException();
            int white = FindLastUsableAccumulator(Color.WHITE), black = FindLastUsableAccumulator(Color.BLACK);
            if (accumulators[white].Computed[0] && accumulators[black].Computed[1])
                ForwardUpdateIncrementalBoth(pos, ft, white, black);
            else
            {
                EvaluateSide(Color.WHITE, pos, ft, cache, white);
                EvaluateSide(Color.BLACK, pos, ft, cache, black);
            }
        }

        private int FindLastUsableAccumulator(Color perspective)
        {
            for (int i = Size - 1; i > 0; --i)
            {
                if (accumulators[i].Computed[perspective] || Half.RequiresRefresh(accumulators[i].Dirties.dirtyPiece, perspective))
                    return i;
            }

            return 0;
        }

        private void EvaluateSide(Color perspective, Position pos, FeatureTransformer ft, AccumulatorCaches cache, int last)
        {
            if (accumulators[last].Computed[perspective])
                ForwardUpdateIncremental(perspective, pos, ft, last);
            else
            {
                var dirty = Latest.Dirties.dirtyPiece;
                if (Size > 1 && dirty.pc == Piece.MakePiece(perspective, PieceType.KING) && accumulators[Size - 2].Computed[perspective] && pos.Count(PieceType.ALL_PIECES) >= 15 && ((int)dirty.from & 4) == ((int)dirty.to & 4) && dirty.add_sq == Square.SQ_NONE)
                {
                    UpdateAccumulatorHybrid(perspective, pos, ft, Latest, accumulators[Size - 2], cache);
                    return;
                }

                UpdateAccumulatorRefreshCache(perspective, ft, pos, Latest, cache);
                BackwardUpdateIncremental(perspective, pos, ft, last);
            }
        }

        private void ForwardUpdateIncremental(Color c, Position pos, FeatureTransformer ft, int begin)
        {
            Square k = pos.SquareOf(PieceType.KING, c);
            for (int i = begin + 1; i < Size; ++i)
                UpdateAccumulatorIncremental(true, c, ft, k, accumulators[i], accumulators[i - 1]);
        }

        private void BackwardUpdateIncremental(Color c, Position pos, FeatureTransformer ft, int end)
        {
            Square k = pos.SquareOf(PieceType.KING, c);
            for (int i = Size - 2; i >= end; --i)
                UpdateAccumulatorIncremental(false, c, ft, k, accumulators[i], accumulators[i + 1]);
        }

        private void ForwardUpdateIncrementalBoth(Position pos, FeatureTransformer ft, int whiteBegin, int blackBegin)
        {
            Square wk = pos.SquareOf(PieceType.KING, Color.WHITE), bk = pos.SquareOf(PieceType.KING, Color.BLACK);
            int shared = Math.Max(whiteBegin, blackBegin);
            for (int i = whiteBegin + 1; i <= shared; ++i)
                UpdateAccumulatorIncremental(true, Color.WHITE, ft, wk, accumulators[i], accumulators[i - 1]);
            for (int i = blackBegin + 1; i <= shared; ++i)
                UpdateAccumulatorIncremental(true, Color.BLACK, ft, bk, accumulators[i], accumulators[i - 1]);
            for (int i = shared + 1; i < Size; ++i)
                UpdateAccumulatorIncrementalBoth(ft, wk, bk, accumulators[i], accumulators[i - 1]);
        }

        private static void AppendPairs(ref Threats.IndexList to, in Pairs.IndexList pairs)
        {
            for (int i = 0; i < pairs.Size(); ++i)
                to.PushBack(pairs[i]);
        }

        private static void Changed(Color c, Square k, in Dirties dirty, out Half.IndexList pr, out Half.IndexList pa, out Threats.IndexList tr, out Threats.IndexList ta)
        {
            pr = default;
            pa = default;
            tr = default;
            ta = default;
            Pairs.IndexList ppRemoved = default, ppAdded = default;
            Threats.AppendChangedIndices(c, k, dirty.dirtyThreats, ref tr, ref ta);
            if (NnueExecution.UseBlocks) Pairs.AppendChangedIndicesBlocks(c, k, dirty.dirtyPawnPairs, ref ppRemoved, ref ppAdded);
            else Pairs.AppendChangedIndices(c, k, dirty.dirtyPawnPairs, ref ppRemoved, ref ppAdded);
            AppendPairs(ref tr, ppRemoved);
            AppendPairs(ref ta, ppAdded);
            Half.AppendChangedIndices(c, k, dirty.dirtyPiece, ref pr, ref pa);
        }

        private static void UpdateAccumulatorIncremental(bool forward, Color c, FeatureTransformer ft, Square k, AccumulatorState target, AccumulatorState computed)
        {
            Changed(c, k, forward ? target.Dirties : computed.Dirties, out var pr, out var pa, out var tr, out var ta);
            if (!forward)
            {
                var p = pr;
                pr = pa;
                pa = p;
                var t = tr;
                tr = ta;
                ta = t;
            }

            ApplyCombined(c, ft, computed, target, pa, pr, ta, tr);
            target.Computed[c] = true;
        }

        private static void UpdateAccumulatorIncrementalBoth(FeatureTransformer ft, Square wk, Square bk, AccumulatorState target, AccumulatorState computed)
        {
            Threats.IndexList wr = default, wa = default, br = default, ba = default;
            Pairs.IndexList pwr = default, pwa = default, pbr = default, pba = default;
            Threats.AppendChangedIndicesBoth(wk, bk, target.Dirties.dirtyThreats, ref wr, ref wa, ref br, ref ba);
            if (NnueExecution.UseBlocks) Pairs.AppendChangedIndicesBothBlocks(wk, bk, target.Dirties.dirtyPawnPairs, ref pwr, ref pwa, ref pbr, ref pba);
            else Pairs.AppendChangedIndicesBoth(wk, bk, target.Dirties.dirtyPawnPairs, ref pwr, ref pwa, ref pbr, ref pba);
            AppendPairs(ref wr, pwr);
            AppendPairs(ref wa, pwa);
            AppendPairs(ref br, pbr);
            AppendPairs(ref ba, pba);
            Half.IndexList pr = default, pa = default;
            Half.AppendChangedIndices(Color.WHITE, wk, target.Dirties.dirtyPiece, ref pr, ref pa);
            ApplyCombined(Color.WHITE, ft, computed, target, pa, pr, wa, wr);
            pr = default;
            pa = default;
            Half.AppendChangedIndices(Color.BLACK, bk, target.Dirties.dirtyPiece, ref pr, ref pa);
            ApplyCombined(Color.BLACK, ft, computed, target, pa, pr, ba, br);
            target.Computed[0] = target.Computed[1] = true;
        }

        private static void PsqBlocks(Span<short> target, in Half.IndexList list, FeatureTransformer ft, int sign)
        {
            var weights = MemoryMarshal.Cast<WeightType, short>(ft.Weights.AsReadOnlySpan());
            for (int i = 0; i < list.Size(); ++i) NnueKernels.Add16(target, weights.Slice(list[i] * 1024, 1024), sign);
        }
        private static void ThreatBlocks(Span<short> target, in Threats.IndexList list, FeatureTransformer ft, int sign)
        {
            var weights = MemoryMarshal.Cast<ThreatWeightType, sbyte>(ft.ThreatAndPpWeights.AsReadOnlySpan());
            if (NnueExecution.UseVariable) { VariableKernels.AddThreats(target, weights, 1024, list, sign, NnueExecution.VariableTiling); return; }
            for (int i = 0; i < list.Size(); ++i) NnueKernels.Add8(target, weights.Slice(list[i] * 1024, 1024), sign);
        }
        private static void PsqtBlocks(Span<int> target, in Half.IndexList list, FeatureTransformer ft, int sign)
        {
            var weights = MemoryMarshal.Cast<PSQTWeightType, int>(ft.PsqtWeights.AsReadOnlySpan());
            for (int i = 0; i < list.Size(); ++i) NnueKernels.AddPsqt(target, weights.Slice(list[i] * 8, 8), sign);
        }
        private static void ThreatPsqtBlocks(Span<int> target, in Threats.IndexList list, FeatureTransformer ft, int sign)
        {
            var weights = MemoryMarshal.Cast<PSQTWeightType, int>(ft.ThreatAndPpPsqtWeights.AsReadOnlySpan());
            for (int i = 0; i < list.Size(); ++i) NnueKernels.AddPsqt(target, weights.Slice(list[i] * 8, 8), sign);
        }

        private static short ApplyPsq(int j, short acc, in Half.IndexList list, FeatureTransformer ft, int sign)
        {
            ReadOnlySpan<WeightType> weights = ft.Weights.AsReadOnlySpan();
            for (int i = 0; i < list.Size(); ++i)
                acc = unchecked((short)(acc + sign * (int)weights[list[i] * 1024 + j]));
            return acc;
        }

        private static short ApplyThreats(int j, short acc, in Threats.IndexList list, FeatureTransformer ft, int sign)
        {
            ReadOnlySpan<ThreatWeightType> weights = ft.ThreatAndPpWeights.AsReadOnlySpan();
            for (int i = 0; i < list.Size(); ++i)
                acc = unchecked((short)(acc + sign * (int)weights[list[i] * 1024 + j]));
            return acc;
        }

        private static int ApplyPsqt(int j, int acc, in Half.IndexList list, FeatureTransformer ft, int sign)
        {
            ReadOnlySpan<PSQTWeightType> weights = ft.PsqtWeights.AsReadOnlySpan();
            for (int i = 0; i < list.Size(); ++i)
                acc = checked(acc + sign * (int)weights[list[i] * 8 + j]);
            return acc;
        }

        private static int ApplyPsqtThreats(int j, int acc, in Threats.IndexList list, FeatureTransformer ft, int sign)
        {
            ReadOnlySpan<PSQTWeightType> weights = ft.ThreatAndPpPsqtWeights.AsReadOnlySpan();
            for (int i = 0; i < list.Size(); ++i)
                acc = checked(acc + sign * (int)weights[list[i] * 8 + j]);
            return acc;
        }

        private static void ApplyCombined(Color c, FeatureTransformer ft, AccumulatorState from, AccumulatorState to, in Half.IndexList pa, in Half.IndexList pr, in Threats.IndexList ta, in Threats.IndexList tr)
        {
            if (NnueExecution.UseBlocks)
            {
                Span<short> target = to.Accumulation[c];
                from.Accumulation[c].AsSpan().CopyTo(target);
                PsqBlocks(target, pr, ft, -1); PsqBlocks(target, pa, ft, 1);
                ThreatBlocks(target, tr, ft, -1); ThreatBlocks(target, ta, ft, 1);
                Span<int> psqt = to.PsqtAccumulation[c];
                from.PsqtAccumulation[c].AsSpan().CopyTo(psqt);
                PsqtBlocks(psqt, pr, ft, -1); PsqtBlocks(psqt, pa, ft, 1);
                ThreatPsqtBlocks(psqt, tr, ft, -1); ThreatPsqtBlocks(psqt, ta, ft, 1);
                return;
            }
            for (int j = 0; j < 1024; ++j)
            {
                short a = from.Accumulation[c][j];
                a = ApplyPsq(j, a, pr, ft, -1);
                a = ApplyPsq(j, a, pa, ft, 1);
                a = ApplyThreats(j, a, tr, ft, -1);
                a = ApplyThreats(j, a, ta, ft, 1);
                to.Accumulation[c][j] = a;
            }

            for (int j = 0; j < 8; ++j)
            {
                int a = from.PsqtAccumulation[c][j];
                a = ApplyPsqt(j, a, pr, ft, -1);
                a = ApplyPsqt(j, a, pa, ft, 1);
                a = ApplyPsqtThreats(j, a, tr, ft, -1);
                a = ApplyPsqtThreats(j, a, ta, ft, 1);
                to.PsqtAccumulation[c][j] = a;
            }
        }

        public static Bitboard GetChangedPieces(IReadOnlyList<Piece> oldPieces, IReadOnlyList<Piece> newPieces)
        {
            if (oldPieces.Count != 64 || newPieces.Count != 64)
                throw new ArgumentException("A piece array must contain 64 squares.");
            if (NnueExecution.UseFixed) return FixedKernels.GetChangedPieces(oldPieces, newPieces, NnueExecution.FixedTiling);
            if (NnueExecution.UseVariable) return VariableKernels.GetChangedPieces(oldPieces, newPieces, NnueExecution.VariableTiling);
            ulong changed = 0;
            if (NnueExecution.HardwareVectors && oldPieces is Piece[] oldArray && newPieces is Piece[] newArray)
            {
                var before = MemoryMarshal.Cast<Piece, byte>(oldArray.AsSpan());
                var after = MemoryMarshal.Cast<Piece, byte>(newArray.AsSpan());
                for (int start = 0; start < 64; start += Vector<byte>.Count)
                {
                    var equal = Vector.Equals(NnueKernels.Load<byte>(before.Slice(start)), NnueKernels.Load<byte>(after.Slice(start)));
                    for (int lane = 0; lane < Vector<byte>.Count; ++lane)
                        if (equal[lane] == 0) changed |= 1UL << (start + lane);
                }
                return changed;
            }
            for (int s = 0; s < 64; ++s)
                if (oldPieces[s] != newPieces[s])
                    changed |= 1UL << s;
            return changed;
        }

        private static void CacheChanges(Color c, Square k, AccumulatorCacheEntry entry, IReadOnlyList<Piece> pieces, Bitboard pieceBB, out Half.IndexList removed, out Half.IndexList added)
        {
            removed = default;
            added = default;
            Bitboard changed = GetChangedPieces(entry.Pieces, pieces), r = changed & entry.PieceBB, a = changed & pieceBB;
            if (NnueExecution.UseBlocks)
            {
                Half.WriteIndices(entry.Pieces, pieces, r, a, c, k, ref removed, ref added);
                return;
            }
            while (r != 0)
            {
                Square s = Bitboards.PopLsb(ref r);
                removed.PushBack(Half.MakeIndex(c, s, entry.Pieces[s], k));
            }

            while (a != 0)
            {
                Square s = Bitboards.PopLsb(ref a);
                added.PushBack(Half.MakeIndex(c, s, pieces[s], k));
            }
        }

        private static void StoreBoard(AccumulatorCacheEntry entry, IReadOnlyList<Piece> pieces, Bitboard pieceBB)
        {
            for (int s = 0; s < 64; ++s)
                entry.Pieces[s] = pieces[s];
            entry.PieceBB = pieceBB;
        }

        private static void UpdateAccumulatorRefreshCache(Color c, FeatureTransformer ft, Position pos, AccumulatorState state, AccumulatorCaches cache)
        {
            Square k = pos.SquareOf(PieceType.KING, c);
            var entry = cache[k, c];
            CacheChanges(c, k, entry, pos.PieceArray(), pos.Pieces(), out var removed, out var added);
            StoreBoard(entry, pos.PieceArray(), pos.Pieces());
            Threats.IndexList active = default;
            Pairs.IndexList pairs = default;
            Threats.AppendActiveIndices(c, pos, ref active);
            Pairs.AppendActiveIndices(c, pos, ref pairs);
            AppendPairs(ref active, pairs);
            if (NnueExecution.UseBlocks)
            {
                var cached = MemoryMarshal.Cast<BiasType, short>(entry.Accumulation.AsSpan());
                PsqBlocks(cached, removed, ft, -1); PsqBlocks(cached, added, ft, 1);
                cached.CopyTo(state.Accumulation[c]);
                ThreatBlocks(state.Accumulation[c], active, ft, 1);
                var psqt = MemoryMarshal.Cast<PSQTWeightType, int>(entry.PsqtAccumulation.AsSpan());
                PsqtBlocks(psqt, removed, ft, -1); PsqtBlocks(psqt, added, ft, 1);
                psqt.CopyTo(state.PsqtAccumulation[c]);
                ThreatPsqtBlocks(state.PsqtAccumulation[c], active, ft, 1);
                state.Computed[c] = true;
                return;
            }
            for (int j = 0; j < 1024; ++j)
            {
                short a = (short)entry.Accumulation[j];
                a = ApplyPsq(j, a, removed, ft, -1);
                a = ApplyPsq(j, a, added, ft, 1);
                entry.Accumulation[j] = a;
                state.Accumulation[c][j] = ApplyThreats(j, a, active, ft, 1);
            }

            for (int j = 0; j < 8; ++j)
            {
                int a = entry.PsqtAccumulation[j];
                a = ApplyPsqt(j, a, removed, ft, -1);
                a = ApplyPsqt(j, a, added, ft, 1);
                entry.PsqtAccumulation[j] = a;
                state.PsqtAccumulation[c][j] = ApplyPsqtThreats(j, a, active, ft, 1);
            }

            state.Computed[c] = true;
        }

        private void UpdateAccumulatorHybrid(Color c, Position pos, FeatureTransformer ft, AccumulatorState target, AccumulatorState from, AccumulatorCaches cache)
        {
            var dirty = target.Dirties.dirtyPiece;
            Square oldK = dirty.from, newK = dirty.to;
            Piece[] previous = hybridPrevious;
            for (int s = 0; s < 64; ++s)
                previous[s] = pos.PieceArray()[s];
            Bitboard previousBB = pos.Pieces();
            if (dirty.remove_sq != Square.SQ_NONE)
                previous[newK] = dirty.remove_pc;
            else
            {
                previous[newK] = Piece.NO_PIECE;
                previousBB &= ~Bitboards.SquareBB(newK);
            }

            previous[oldK] = Piece.MakePiece(c, PieceType.KING);
            previousBB |= Bitboards.SquareBB(oldK);
            var oldEntry = cache[oldK, c];
            var newEntry = cache[newK, c];
            CacheChanges(c, oldK, oldEntry, previous, previousBB, out var oldRemove, out var oldAdd);
            CacheChanges(c, newK, newEntry, pos.PieceArray(), pos.Pieces(), out var newRemove, out var newAdd);
            Threats.IndexList tr = default, ta = default;
            Pairs.IndexList pr = default, pa = default;
            Threats.AppendChangedIndices(c, newK, target.Dirties.dirtyThreats, ref tr, ref ta);
            if (NnueExecution.UseBlocks) Pairs.AppendChangedIndicesBlocks(c, newK, target.Dirties.dirtyPawnPairs, ref pr, ref pa);
            else Pairs.AppendChangedIndices(c, newK, target.Dirties.dirtyPawnPairs, ref pr, ref pa);
            AppendPairs(ref tr, pr);
            AppendPairs(ref ta, pa);
            if (NnueExecution.UseBlocks)
            {
                var cached = MemoryMarshal.Cast<BiasType, short>(newEntry.Accumulation.AsSpan());
                PsqBlocks(cached, newRemove, ft, -1); PsqBlocks(cached, newAdd, ft, 1);
                cached.CopyTo(target.Accumulation[c]);
                NnueKernels.Add16(target.Accumulation[c], from.Accumulation[c], 1);
                NnueKernels.Add16(target.Accumulation[c], MemoryMarshal.Cast<BiasType, short>(oldEntry.Accumulation.AsSpan()), -1);
                PsqBlocks(target.Accumulation[c], oldRemove, ft, 1); PsqBlocks(target.Accumulation[c], oldAdd, ft, -1);
                ThreatBlocks(target.Accumulation[c], tr, ft, -1); ThreatBlocks(target.Accumulation[c], ta, ft, 1);
                var cachedPsqt = MemoryMarshal.Cast<PSQTWeightType, int>(newEntry.PsqtAccumulation.AsSpan());
                PsqtBlocks(cachedPsqt, newRemove, ft, -1); PsqtBlocks(cachedPsqt, newAdd, ft, 1);
                cachedPsqt.CopyTo(target.PsqtAccumulation[c]);
                // Preserve checked signed32 diagnostic and original combined-expression order.
                for (int j = 0; j < 8; ++j)
                    target.PsqtAccumulation[c][j] = checked(target.PsqtAccumulation[c][j] + from.PsqtAccumulation[c][j] - (int)oldEntry.PsqtAccumulation[j]);
                PsqtBlocks(target.PsqtAccumulation[c], oldRemove, ft, 1); PsqtBlocks(target.PsqtAccumulation[c], oldAdd, ft, -1);
                ThreatPsqtBlocks(target.PsqtAccumulation[c], tr, ft, -1); ThreatPsqtBlocks(target.PsqtAccumulation[c], ta, ft, 1);
                StoreBoard(newEntry, pos.PieceArray(), pos.Pieces()); target.Computed[c] = true;
                return;
            }
            for (int j = 0; j < 1024; ++j)
            {
                short a = (short)newEntry.Accumulation[j];
                a = ApplyPsq(j, a, newRemove, ft, -1);
                a = ApplyPsq(j, a, newAdd, ft, 1);
                newEntry.Accumulation[j] = a;
                a = unchecked((short)(a + from.Accumulation[c][j]));
                a = unchecked((short)(a - (int)oldEntry.Accumulation[j]));
                a = ApplyPsq(j, a, oldRemove, ft, 1);
                a = ApplyPsq(j, a, oldAdd, ft, -1);
                a = ApplyThreats(j, a, tr, ft, -1);
                target.Accumulation[c][j] = ApplyThreats(j, a, ta, ft, 1);
            }

            for (int j = 0; j < 8; ++j)
            {
                int a = newEntry.PsqtAccumulation[j];
                a = ApplyPsqt(j, a, newRemove, ft, -1);
                a = ApplyPsqt(j, a, newAdd, ft, 1);
                newEntry.PsqtAccumulation[j] = a;
                a = checked(a + from.PsqtAccumulation[c][j] - (int)oldEntry.PsqtAccumulation[j]);
                a = ApplyPsqt(j, a, oldRemove, ft, 1);
                a = ApplyPsqt(j, a, oldAdd, ft, -1);
                a = ApplyPsqtThreats(j, a, tr, ft, -1);
                target.PsqtAccumulation[c][j] = ApplyPsqtThreats(j, a, ta, ft, 1);
            }

            StoreBoard(newEntry, pos.PieceArray(), pos.Pieces());
            target.Computed[c] = true;
        }
    }
}
