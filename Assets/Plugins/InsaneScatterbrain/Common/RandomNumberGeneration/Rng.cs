using System;

namespace InsaneScatterbrain.RandomNumberGeneration
{
    public class Rng
    {
        private RngState state;

        public Rng(RngState state)
        {
            ValidateState(state);
            this.state = state;
        }
        public void SetState(RngState newState)
        {
            ValidateState(newState);
            state = newState;
        }

        public RngState GetState() => state;

        private static void ValidateState(RngState validateState)
        {
            if (!validateState.IsValid)
            {
                throw new ArgumentOutOfRangeException(nameof(validateState), validateState,
                    "All components of the state are 0, which is an invalid state.");
            }
        }

        private uint Random()
        {
            var t  = state.d;
    
            var s = state.a;
            (state.d, state.c, state.b) = (state.c, state.b, s);

            t ^= t << 11;
            t ^= t >> 8;
            return state.a = t ^ s ^ (s >> 19);
        }

        public uint UInt() => Random();
        public double Double() => Random() * 1d / (uint.MaxValue + 1d);
        public float Float() => (float)Double();
        public int Int() => (int)(Double() * int.MaxValue);
        public bool Bool() => Random() > uint.MaxValue / 2;
        
        public void Bytes(byte[] bytes)
        {
            var i = 0;
            uint s;
            while (i < bytes.Length - 3)
            {
                s = Random();
                bytes[i++] = (byte)s;
                bytes[i++] = (byte)(s >> 8);
                bytes[i++] = (byte)(s >> 16);
                bytes[i++] = (byte)(s >> 24);
            }

            if (i == bytes.Length) return;

            s = Random();

            bytes[i++] = (byte)s;
            if (i < bytes.Length) bytes[i++] = (byte)(s >> 8);
            if (i < bytes.Length) bytes[i++] = (byte)(s >> 16);
            if (i < bytes.Length) bytes[i] = (byte)(s >> 24);
        }

        public uint UInt(uint max) => (uint)(Double() * max);
        public uint UInt(uint min, uint max)
        {
            if (min > max)
            {
                throw new ArgumentOutOfRangeException(nameof(min), min, "Min. cannot be larger than max.");
            }
            return min + (uint)(Double() * (max - min));
        }
        
        public int Int(int max) => (int)(Double() * max);
        public int Int(int min, int max)
        {
            if (min > max)
            {
                throw new ArgumentOutOfRangeException(nameof(min), min, "Min. cannot be larger than max.");
            }
            return min + (int)(Double() * (max - min));
        }

        public double Double(double max) => Double() * max;
        public double Double(double min, double max)
        {
            if (min > max)
            {
                throw new ArgumentOutOfRangeException(nameof(min), min, "Min. cannot be larger than max.");
            }
            return min + Double() * (max - min);
        }
        
        public float Float(float max) => Float() * max;
        public float Float(float min, float max)
        {
            if (min > max)
            {
                throw new ArgumentOutOfRangeException(nameof(min), min, "Min. cannot be larger than max.");
            }
            return min + Float() * (max - min);
        }
    }
}
