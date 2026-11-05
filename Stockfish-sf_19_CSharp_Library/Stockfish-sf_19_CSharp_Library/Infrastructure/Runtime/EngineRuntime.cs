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
using System.Reflection;
using System.Runtime.InteropServices;
using Stockfish_sf_19_CSharp_Library.Evaluation.NNUE;

namespace Stockfish_sf_19_CSharp_Library.Infrastructure.Runtime
{
    [Flags]
    public enum X86Feature : ulong
    {
        Sse41=1, Popcnt=2, Avx2=4, Bmi2=8, Avx512F=16, Avx512Vl=32, Avx512Bw=64,
        Avx512Vnni=128, Avx512Ifma=256, Avx512Vbmi=512, Avx512Vbmi2=1024,
        Avx512Vpopcntdq=2048, Avx512Bitalg=4096, Vpclmulqdq=8192, Gfni=16384, Vaes=32768, AvxVnni=65536
    }
    public readonly struct X86CpuFeatures
    {
        public X86Feature Features { get; }
        public bool SlowBmi2 { get; }
        public X86CpuFeatures(X86Feature features, bool slowBmi2=false) { Features=features; SlowBmi2=slowBmi2; }
        public bool Has(X86Feature features) => (Features & features)==features;
        public static bool HasSlowBmi2(string vendor, int family, int model) => vendor=="AuthenticAMD" &&
            (family==0x17 || (family==0x15 && model>=0x60 && model<=0x7f));
    }
    public readonly struct Arm64CpuFeatures
    {
        public bool Dotprod { get; }
        public Arm64CpuFeatures(bool dotprod) => Dotprod=dotprod;
    }
    public readonly struct Riscv64CpuFeatures
    {
        public bool Gcv { get; }
        public bool Zbitmanip { get; }
        public bool Zicond { get; }
        public int VectorBits { get; }
        public Riscv64CpuFeatures(bool gcv, bool zbitmanip, bool zicond, int vectorBits)
        { Gcv=gcv; Zbitmanip=zbitmanip; Zicond=zicond; VectorBits=gcv?vectorBits:0; }
        public static Riscv64CpuFeatures FromProbe(ulong hwcap, long key, ulong extensions, bool success, int vectorBits) =>
            new Riscv64CpuFeatures((hwcap & 0x20112d)==0x20112d, success && key==4 && (extensions & 0x38)==0x38,
                success && key==4 && (extensions & (1UL<<35))!=0, vectorBits);
    }
    /// <summary>Original universal dispatch decisions. Names describe original builds, not CLR code emission.</summary>
    public static class UniversalDispatch
    {
        public static string Select(X86CpuFeatures f)
        {
            if (!f.Has(X86Feature.Sse41|X86Feature.Popcnt)) return "x86_64";
            if (!f.Has(X86Feature.Avx2)) return "x86_64_sse41_popcnt";
            if (!f.Has(X86Feature.Bmi2) || f.SlowBmi2) return "x86_64_avx2";
            if (!f.Has(X86Feature.Avx512F|X86Feature.Avx512Vl|X86Feature.Avx512Bw))
                return f.Has(X86Feature.AvxVnni)?"x86_64_avxvnni":"x86_64_bmi2";
            if (!f.Has(X86Feature.Avx512Vnni)) return "x86_64_avx512";
            const X86Feature icl=X86Feature.Avx512Ifma|X86Feature.Avx512Vbmi|X86Feature.Avx512Vbmi2|
                X86Feature.Avx512Vpopcntdq|X86Feature.Avx512Bitalg|X86Feature.Vpclmulqdq|X86Feature.Gfni|X86Feature.Vaes;
            return f.Has(icl)?"x86_64_avx512icl":"x86_64_vnni512";
        }
        public static string Select(Arm64CpuFeatures f) => f.Dotprod?"armv8_dotprod":"armv8";
        public static string Select(Riscv64CpuFeatures f) => f.Gcv && f.Zbitmanip && f.Zicond && f.VectorBits>=128?"riscv64_rva23":"riscv64";
    }
    /// <summary>Safe runtime discovery. Unknown features stay false; no handwritten executable probes.</summary>
    public static class RuntimeCapabilities
    {
        public static string ProcessArchitecture => RuntimeInformation.ProcessArchitecture.ToString();
        public static bool Supports(string className)
        {
#if ENABLE_IL2CPP || STOCKFISH_AOT_SAFE
            // Optional CoreCLR intrinsics are neither part of netstandard2.1 nor
            // a promise made by IL2CPP. Portable kernels retain full functionality.
            return false;
#else
            try
            {
                var type=Type.GetType("System.Runtime.Intrinsics."+className+", System.Private.CoreLib", false)
                    ?? Type.GetType("System.Runtime.Intrinsics."+className+", System.Runtime.Intrinsics", false);
                return type?.GetProperty("IsSupported", BindingFlags.Static|BindingFlags.Public)?.GetValue(null) is bool supported && supported;
            }
            catch (Exception e) when (e is TypeLoadException || e is TargetInvocationException || e is NotSupportedException) { return false; }
#endif
        }
        private static (uint A,uint B,uint C,uint D) CpuId(int leaf, int subleaf=0)
        {
#if ENABLE_IL2CPP || STOCKFISH_AOT_SAFE
            return default;
#else
            var type=Type.GetType("System.Runtime.Intrinsics.X86.X86Base, System.Private.CoreLib",false)
                ?? Type.GetType("System.Runtime.Intrinsics.X86.X86Base, System.Runtime.Intrinsics",false);
            var method=type?.GetMethod("CpuId",new[]{typeof(int),typeof(int)});
            object? value=method?.Invoke(null,new object[]{leaf,subleaf});
            if (value==null) return default;
            var resultType=value.GetType();
            uint Field(string name)=>unchecked((uint)(int)resultType.GetField(name)!.GetValue(value)!);
            return (Field("Item1"),Field("Item2"),Field("Item3"),Field("Item4"));
#endif
        }
        public static X86CpuFeatures QueryX86()
        {
            if (!Supports("X86.X86Base")) return default;
            try
            {
                var id=CpuId(0); var one=CpuId(1); var seven=id.A>=7?CpuId(7):default;
                X86Feature flags=0;
                void Add(bool condition,X86Feature flag) { if(condition) flags|=flag; }
                bool Bit(uint value,int bit)=>(value & (1U<<bit))!=0;
                Add(Supports("X86.Sse41"),X86Feature.Sse41); Add(Supports("X86.Popcnt"),X86Feature.Popcnt);
                Add(Supports("X86.Avx2"),X86Feature.Avx2); Add(Supports("X86.Bmi2"),X86Feature.Bmi2);
                bool wide=Supports("X86.Avx512F");
                Add(wide,X86Feature.Avx512F); Add(wide && Bit(seven.B,31),X86Feature.Avx512Vl); Add(wide && Bit(seven.B,30),X86Feature.Avx512Bw);
                Add(wide && Bit(seven.C,11),X86Feature.Avx512Vnni); Add(wide && Bit(seven.B,21),X86Feature.Avx512Ifma);
                Add(wide && Bit(seven.C,1),X86Feature.Avx512Vbmi); Add(wide && Bit(seven.C,6),X86Feature.Avx512Vbmi2);
                Add(wide && Bit(seven.C,14),X86Feature.Avx512Vpopcntdq); Add(wide && Bit(seven.C,12),X86Feature.Avx512Bitalg);
                Add(wide && Bit(seven.C,10),X86Feature.Vpclmulqdq); Add(wide && Bit(seven.C,8),X86Feature.Gfni); Add(wide && Bit(seven.C,9),X86Feature.Vaes);
                Add(Supports("X86.AvxVnni"),X86Feature.AvxVnni);
                var vendorBytes=new byte[12]; Buffer.BlockCopy(new[]{id.B,id.D,id.C},0,vendorBytes,0,12);
                string vendor=System.Text.Encoding.ASCII.GetString(vendorBytes);
                int family=(int)(one.A>>8)&15,model=(int)(one.A>>4)&15;
                if(family==15) family+=(int)(one.A>>20)&255;
                if(((one.A>>8)&15)==6 || ((one.A>>8)&15)==15) model|=(int)(one.A>>12)&0xf0;
                return new X86CpuFeatures(flags,X86CpuFeatures.HasSlowBmi2(vendor,family,model));
            }
            catch(Exception e) when(e is TargetInvocationException || e is NotSupportedException || e is MemberAccessException) { return default; }
        }
        public static Arm64CpuFeatures QueryArm64()
        {
            if(ProcessArchitecture!="Arm64") return default;
            if(Supports("Arm.Dp")) return new Arm64CpuFeatures(true);
            try
            {
                if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return new Arm64CpuFeatures(IsProcessorFeaturePresent(43));
                if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return new Arm64CpuFeatures((GetAuxval(new UIntPtr(16)).ToUInt64() & (1UL<<20))!=0);
            }
            catch(Exception e) when(e is DllNotFoundException || e is EntryPointNotFoundException) { }
            return default;
        }
        /// <summary>Optional host-target native service supplies vlenb safely after HWCAP.
        /// Missing/unsupported services select the fully functional baseline.</summary>
        public static Riscv64CpuFeatures QueryRiscv64(Func<Riscv64CpuFeatures>? hostProbe=null)
        {
            if (!ProcessArchitecture.Equals("RiscV64",StringComparison.OrdinalIgnoreCase)) return default;
            if (hostProbe!=null) return hostProbe();
            try
            {
                bool success=QueryRiscvNative(out ulong hwcap,out long key,out ulong extensions,out int bits)!=0;
                return Riscv64CpuFeatures.FromProbe(hwcap,key,extensions,success,bits);
            }
            catch(Exception e) when(e is DllNotFoundException || e is EntryPointNotFoundException || e is BadImageFormatException) { return default; }
        }
        [DllImport("stockfish_runtime",EntryPoint="sf19_query_riscv",CallingConvention=CallingConvention.Cdecl)]
        private static extern int QueryRiscvNative(out ulong hwcap,out long key,out ulong extensions,out int vectorBits);
        public static string DefaultNnueProfile => ProcessArchitecture=="Arm64"
            ? (QueryArm64().Dotprod?"neon-dotprod":"neon")
            : System.Numerics.Vector.IsHardwareAccelerated?"blocks":"scalar";
        public static string OriginalBuild => ProcessArchitecture=="X64"?UniversalDispatch.Select(QueryX86())
            : ProcessArchitecture=="Arm64"?UniversalDispatch.Select(QueryArm64())
            : ProcessArchitecture.Equals("RiscV64",StringComparison.OrdinalIgnoreCase)?UniversalDispatch.Select(QueryRiscv64()):"baseline";
        [DllImport("kernel32.dll")][return:MarshalAs(UnmanagedType.Bool)] private static extern bool IsProcessorFeaturePresent(uint feature);
        [DllImport("libc",EntryPoint="getauxval")] private static extern UIntPtr GetAuxval(UIntPtr type);
    }
    /// <summary>CLR type initialization replaces linker initializer arrays. Calling this from any
    /// host or Program establishes and validates immutable execution policies before worker startup.</summary>
    public static class EngineRuntime
    {
        public static string Initialize() => "NNUE="+NnueExecution.SelectedProfile+"; moves="+
            (MoveGeneration.MoveProcessing.UseBlocks?"blocks":"scalar")+"; sliding="+Board.SlidingAttacks.Backend+
            "; original-capability="+RuntimeCapabilities.OriginalBuild+"; code=managed IL";
    }
}
