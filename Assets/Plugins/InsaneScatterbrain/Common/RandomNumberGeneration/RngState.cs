using System;

namespace InsaneScatterbrain.RandomNumberGeneration
{
    public struct RngState
    {
        public uint a;
        public uint b;
        public uint c;
        public uint d;

        public static RngState FromBytes(byte[] bytes) => new RngState
        {
            a = BitConverter.ToUInt32(bytes, 0),
            b = BitConverter.ToUInt32(bytes, 4),
            c = BitConverter.ToUInt32(bytes, 8),
            d = BitConverter.ToUInt32(bytes, 12)
        };

        
        private static ulong SplitMix64(ref ulong state) 
        {
            var result = state += 0x9E3779B97f4A7C15;
            result = (result ^ (result >> 30)) * 0xBF58476D1CE4E5B9;
            result = (result ^ (result >> 27)) * 0x94D049BB133111EB;
            return result ^ (result >> 31);
        }

        public static RngState FromUInt64(ulong seed)
        {
            var result = new RngState();
            
            var tmp = SplitMix64(ref seed);
            result.a = (uint)tmp;
            result.b = (uint)(tmp >> 32);

            tmp = SplitMix64(ref seed);
            result.c = (uint)tmp;
            result.d = (uint)(tmp >> 32);

            return result;
        }

        public static RngState FromInt(int seed) => FromUInt64((ulong)seed);

        public bool IsValid => !(a == 0 && b == 0 && c == 0 && d == 0);

        public static RngState New() => FromBytes(Guid.NewGuid().ToByteArray());

        public override string ToString() => $"{a} {b} {c} {d}";
    }
}