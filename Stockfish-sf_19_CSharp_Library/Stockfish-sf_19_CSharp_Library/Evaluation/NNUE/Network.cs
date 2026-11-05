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
using System.IO;
using System.Text;
using System.Security.Cryptography;
using Stockfish_sf_19_CSharp_Library.Infrastructure.Memory;
using System.Collections.Generic;
using File = System.IO.File;
using Stockfish_sf_19_CSharp_Library.Board;
using Stockfish_sf_19_CSharp_Library.Types;

namespace Stockfish_sf_19_CSharp_Library.Evaluation.NNUE
{
    /// <summary>Shared read-only during evaluation. Load, copy and save require exclusive ownership.</summary>
    public sealed class Network : IDisposable
    {
        public FeatureTransformer FeatureTransformer { get; }

        private readonly NetworkArchitecture[] layers = new NetworkArchitecture[8];
        public IReadOnlyList<NetworkArchitecture> Layers { get; }
        public bool Initialized { get; private set; }
        /// <summary>Optional host equivalent of DEFAULT_NNUE_DIRECTORY, searched after the root directory.</summary>
        public string? DefaultDirectory { get; set; }
        /// <summary>Optional host-provided embedded stream; by default opens the packaged resource, or an empty stream if absent.</summary>
        public Func<Stream>? EmbeddedNetwork { get; set; } = () => EmbeddedNnue.OpenDefault() ?? Stream.Null;
        public static uint Hash { get; } = FeatureTransformer.GetHashValue() ^ new NetworkArchitecture().GetHashValue();

        private readonly WeightStorage storage;
        public int WeightBytes => storage.Length;
        public bool IsReadOnly => storage.IsReadOnly;
        public SystemWideSharedConstantAllocationStatus AllocationStatus { get; private set; } = SystemWideSharedConstantAllocationStatus.LocalMemory;
        public string? SharedIdentity { get; private set; }
        public string? SharedError { get; private set; }
        public string? SharedBackingFile { get; private set; }
        private string? contentIdentity;
        private const string LayoutIdentity = "sf19-scalar-canonical-v1";
        public Network() : this(new WeightStorage()) { }
        private Network(WeightStorage storage)
        {
            this.storage = storage;
            FeatureTransformer = new FeatureTransformer(storage);
            for (int i = 0; i < 8; ++i)
                layers[i] = new NetworkArchitecture(storage);
            Layers = Array.AsReadOnly(layers);
        }

        public Network(Network other) : this() => CopyFrom(other);
        public void Dispose() { storage.Dispose(); AllocationStatus = SystemWideSharedConstantAllocationStatus.NoAllocation; }
        /// <summary>Independent lifetime lease, sharing immutable weights without copying them.</summary>
        public Network AcquireLease()
        {
            var lease = new Network(new WeightStorage(storage.Retain(), true));
            lease.Initialized = Initialized; lease.contentIdentity = contentIdentity;
            lease.DefaultDirectory = DefaultDirectory; lease.EmbeddedNetwork = EmbeddedNetwork;
            lease.AllocationStatus = AllocationStatus; lease.SharedIdentity = SharedIdentity;
            lease.SharedError = SharedError; lease.SharedBackingFile = SharedBackingFile;
            return lease;
        }
        public Network CreateShared(string discriminator, string? unixDirectory = null, bool enableSharing = true)
        {
            // Always hash mutable local images anew. A stored identity belongs only to immutable views.
            string content = IsReadOnly && contentIdentity != null ? contentIdentity : HashImage(storage.Bytes);
            using var constant = new SystemWideSharedConstant<Network>(content, LayoutIdentity + ":" + BitConverter.IsLittleEndian,
                discriminator, WeightBytes, bytes => storage.Bytes.CopyTo(bytes),
                owner => new Network(new WeightStorage(owner, true)), unixDirectory, enableSharing);
            Network result = constant.Value;
            result.Initialized = Initialized; result.contentIdentity = content;
            result.AllocationStatus = constant.Status; result.SharedIdentity = constant.Identity;
            result.SharedError = constant.Error; result.SharedBackingFile = constant.BackingFile;
            result.DefaultDirectory = DefaultDirectory; result.EmbeddedNetwork = EmbeddedNetwork;
            return result.AcquireLease();
        }
        private static string HashImage(ReadOnlySpan<byte> image)
        {
            using var sha = SHA256.Create();
            // Incremental hashing bounds scratch memory, even for the full feature transformer.
            byte[] chunk = new byte[65536];
            while (!image.IsEmpty) { int n = Math.Min(image.Length, chunk.Length); image.Slice(0,n).CopyTo(chunk); sha.TransformBlock(chunk,0,n,null,0); image = image.Slice(n); }
            sha.TransformFinalBlock(Array.Empty<byte>(),0,0);
            return BitConverter.ToString(sha.Hash!).Replace("-", "").ToLowerInvariant();
        }
        /// <summary>Hashes the compressed source by streaming; an existing image skips decoding/private weights.</summary>
        public static Network LoadSharedFile(string path, string discriminator, out string description, string? unixDirectory = null, bool enableSharing = true)
        {
            using var source = File.OpenRead(path);
            string content;
            using (var sha = SHA256.Create()) content = "file:" + BitConverter.ToString(sha.ComputeHash(source)).Replace("-", "").ToLowerInvariant();
            source.Position = 0;
            if (!ReadHeader(source, out uint hash, out description) || hash != Hash) throw new InvalidDataException("Invalid NNUE header.");
            string text = description;
            using var layout = new Network(); // Layout only: no weights allocated.
            using var constant = new SystemWideSharedConstant<Network>(content, LayoutIdentity + ":" + BitConverter.IsLittleEndian,
                discriminator, layout.WeightBytes, bytes =>
                {
                    // The temporary writable view borrows the initializer's mapping; it never owns it.
                    source.Position = 0;
                    InitializeMapped(source, bytes, text);
                    source.Position = 0;
                    using var verify = SHA256.Create();
                    string after = "file:" + BitConverter.ToString(verify.ComputeHash(source)).Replace("-", "").ToLowerInvariant();
                    if (after != content) throw new InvalidDataException("NNUE source changed during initialization.");
                }, owner => new Network(new WeightStorage(owner, true)), unixDirectory, enableSharing);
            var result = constant.Value; result.Initialized = true; result.contentIdentity = content;
            result.AllocationStatus = constant.Status; result.SharedIdentity = constant.Identity;
            result.SharedError = constant.Error; result.SharedBackingFile = constant.BackingFile;
            return result.AcquireLease();
        }
        private static unsafe void InitializeMapped(Stream source, Span<byte> bytes, string description)
        {
            fixed (byte* p = bytes)
            {
                using var borrowed = new BufferOwner(new IntPtr(p), bytes.Length, () => { });
                using var writable = new Network(new WeightStorage(borrowed.Retain(), false));
                if (writable.Load(source) != description) throw new InvalidDataException("Invalid or truncated NNUE parameters.");
            }
        }
        public void CopyFrom(Network other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));
            FeatureTransformer.CopyFrom(other.FeatureTransformer);
            for (int i = 0; i < 8; ++i)
                Layers[i].CopyFrom(other.Layers[i]);
            Initialized = other.Initialized;
            EmbeddedNetwork = other.EmbeddedNetwork;
            DefaultDirectory = other.DefaultDirectory;
        }

        public void Initialize() => Initialized = true;
        public string? Load(Stream stream)
        {
            Initialize();
            return ReadParameters(stream, out string description) ? description : null;
        }

        public bool Save(Stream stream, string description) => WriteParameters(stream, description);
        public void Load(string rootDirectory, string evalfilePath, EvalFile evalFile)
        {
            if (string.IsNullOrEmpty(evalfilePath))
                evalfilePath = EvalFile.DefaultName;
            if (evalFile.Current != evalfilePath && evalfilePath == EvalFile.DefaultName)
                LoadInternal(evalFile);
            foreach (string? dir in new[]
            {
                "",
                rootDirectory,
                DefaultDirectory
            }

            )
                if (dir != null && evalFile.Current != evalfilePath)
                    LoadExternal(dir, evalfilePath, evalFile);
        }

        public void LoadExternal(string directory, string path, EvalFile evalFile)
        {
            Initialize();
            try
            {
                using var stream = File.OpenRead(Path.Combine(directory, path));
                string? description = Load(stream);
                if (description != null)
                {
                    evalFile.Current = path;
                    evalFile.NetDescription = description;
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        public void LoadInternal(EvalFile evalFile)
        {
            try
            {
                using Stream stream = EmbeddedNetwork?.Invoke() ?? Stream.Null;
                string? description = Load(stream);
                if (description != null)
                {
                    evalFile.Current = EvalFile.DefaultName;
                    evalFile.NetDescription = description;
                }
            }
            catch (IOException) { } // Missing/unreadable embedding falls through to external search.
            catch (UnauthorizedAccessException) { }
        }

        public bool Save(EvalFile evalFile, string? filename, Action<string>? report = null)
        {
            if (evalFile.Current == null)
            {
                report?.Invoke("Failed to export a net. No network file is currently loaded. Please load a network file first.");
                return false;
            }

            if (filename == null && evalFile.Current != EvalFile.DefaultName)
            {
                report?.Invoke("Failed to export a net. A non-embedded net can only be saved if the filename is specified");
                return false;
            }

            string path = filename ?? EvalFile.DefaultName;
            bool saved;
            try
            {
                using var stream = File.Create(path);
                saved = Save(stream, evalFile.NetDescription);
            }
            catch (IOException)
            {
                saved = false;
            }
            catch (UnauthorizedAccessException)
            {
                saved = false;
            }

            report?.Invoke(saved ? "Network saved successfully to " + path : "Failed to export a net");
            return saved;
        }

        public void Verify(Action<string>? report, EvalFile evalFile, string evalfilePath)
        {
            if (string.IsNullOrEmpty(evalfilePath))
                evalfilePath = EvalFile.DefaultName;
            if (evalFile.Current != evalfilePath)
            {
                string msg = "ERROR: Network evaluation parameters compatible with the engine must be available.\nERROR: The network file " + evalfilePath + " was not loaded successfully.\nERROR: The UCI option EvalFile might need to specify the full path, including the directory name, to the network file.\nERROR: The default net can be downloaded from: https://tests.stockfishchess.org/api/nn/" + EvalFile.DefaultName + "\nERROR: The engine will be terminated now.\n";
                report?.Invoke(msg);
                throw new NetworkVerificationException(msg);
            }

            report?.Invoke("NNUE evaluation using " + evalfilePath + " (109MiB, (86896, 1024, 32, 32, 1))");
        }

        public UIntPtr GetContentHash()
        {
            if (!Initialized)
                return UIntPtr.Zero;
            ulong h = NnueHash.Combine(0, FeatureTransformer.GetContentHash().ToUInt64());
            foreach (var layer in Layers)
                h = NnueHash.Combine(h, layer.GetContentHash().ToUInt64());
            return new UIntPtr(h);
        }

        public override int GetHashCode() => unchecked((int)GetContentHash().ToUInt64());
        public NetworkOutput Evaluate(Position pos, AccumulatorStack stack, AccumulatorCaches cache)
        {
            Span<byte> transformed = stackalloc byte[1024];
            int bucket = (pos.Count(PieceType.ALL_PIECES) - 1) / 4;
            int psqt, positional;
            if (NnueExecution.UseBlocks)
            {
                int chunkSize = NnueExecution.UseVariable ? 1 : 4;
                Span<ushort> indices = stackalloc ushort[1024 / chunkSize];
                Span<byte> bits = stackalloc byte[128 / chunkSize];
                Span<int> state = stackalloc int[3];
                var info = new NNZInfo(1024, indices, bits, state, chunkSize);
                psqt = FeatureTransformer.Transform(pos, stack, cache, transformed, bucket, info);
                positional = Layers[bucket].Propagate(transformed, info);
            }
            else
            {
                psqt = FeatureTransformer.Transform(pos, stack, cache, transformed, bucket);
                positional = Layers[bucket].Propagate(transformed);
            }
            return new NetworkOutput(psqt / NnueConstants.OutputScale, positional / NnueConstants.OutputScale);
        }

        public NnueEvalTrace TraceEvaluate(Position pos, AccumulatorStack stack, AccumulatorCaches cache)
        {
            var trace = new NnueEvalTrace
            {
                CorrectBucket = (pos.Count(PieceType.ALL_PIECES) - 1) / 4
            };
            Span<byte> transformed = stackalloc byte[1024];
            for (int b = 0; b < 8; ++b)
            {
                trace.Psqt[b] = FeatureTransformer.Transform(pos, stack, cache, transformed, b) / NnueConstants.OutputScale;
                trace.Positional[b] = Layers[b].Propagate(transformed) / NnueConstants.OutputScale;
            }

            return trace;
        }

        public static bool ReadHeader(Stream stream, out uint hashValue, out string description)
        {
            hashValue = 0;
            description = "";
            try
            {
                uint version = NnueBinary.ReadLittleEndian<uint>(stream);
                hashValue = NnueBinary.ReadLittleEndian<uint>(stream);
                uint size = NnueBinary.ReadLittleEndian<uint>(stream);
                if (version != NnueConstants.Version)
                    return false;
                // Native std::string is a byte string; map its bytes to U+0000..U+00FF without loss.
                byte[] chunk = new byte[4096];
                var text = new StringBuilder();
                while (size > 0)
                {
                    int want = (int)Math.Min(size, 4096U);
                    int got = 0;
                    while (got < want)
                    {
                        int count = stream.Read(chunk, got, want - got);
                        if (count == 0)
                            break;
                        got += count;
                    }

                    for (int i = 0; i < got; ++i)
                        text.Append((char)chunk[i]);
                    if (got != want)
                    {
                        description = text.ToString();
                        return false;
                    }

                    size -= (uint)got;
                }

                description = text.ToString();
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        public static bool WriteHeader(Stream stream, uint hashValue, string description)
        {
            if (description == null)
                throw new ArgumentNullException(nameof(description));
            foreach (char c in description)
                if (c > 255)
                    throw new ArgumentException("A network description must be a byte string.", nameof(description));
            try
            {
                NnueBinary.WriteLittleEndian(stream, NnueConstants.Version);
                NnueBinary.WriteLittleEndian(stream, hashValue);
                NnueBinary.WriteLittleEndian(stream, (uint)description.Length);
                byte[] chunk = new byte[4096];
                for (int i = 0; i < description.Length;)
                {
                    int count = Math.Min(4096, description.Length - i);
                    for (int j = 0; j < count; ++j)
                        chunk[j] = (byte)description[i + j];
                    stream.Write(chunk, 0, count);
                    i += count;
                }

                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        public bool ReadParameters(Stream stream, out string description)
        {
            if (!ReadHeader(stream, out uint hash, out description) || hash != Hash)
                return false;
            try
            {
                if (NnueBinary.ReadLittleEndian<uint>(stream) != FeatureTransformer.GetHashValue() || !FeatureTransformer.ReadParameters(stream))
                    return false;
                foreach (var layer in Layers)
                    if (NnueBinary.ReadLittleEndian<uint>(stream) != layer.GetHashValue() || !layer.ReadParameters(stream))
                        return false;
                return stream.ReadByte() == -1;
            }
            catch (IOException)
            {
                return false;
            }
        }

        public bool WriteParameters(Stream stream, string description)
        {
            if (!WriteHeader(stream, Hash, description))
                return false;
            try
            {
                NnueBinary.WriteLittleEndian(stream, FeatureTransformer.GetHashValue());
                if (!FeatureTransformer.WriteParameters(stream))
                    return false;
                foreach (var layer in Layers)
                {
                    NnueBinary.WriteLittleEndian(stream, layer.GetHashValue());
                    if (!layer.WriteParameters(stream))
                        return false;
                }

                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }
    }
}
