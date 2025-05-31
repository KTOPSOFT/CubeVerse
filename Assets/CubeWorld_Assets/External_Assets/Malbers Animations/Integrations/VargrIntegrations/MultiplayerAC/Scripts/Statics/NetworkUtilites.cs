using System;
using UnityEngine;

namespace MalbersAnimations.VargrMultiplayer
{
    public static class SeedRNG
    {
        public static uint Next (UInt32 a)
        {
            return (UInt32)(((UInt64)a * 279470273uL) % 4294967291uL);
        }

        public static Vector3 onUnitSphere(ref UInt32 seed)
		{
			Vector3 data = Vector3.zero;
            seed = SeedRNG.Next(seed);
            data.x = ((float)(seed % 20)-10)*0.1f;
            seed = SeedRNG.Next(seed);
            data.y = ((float)(seed % 20)-10)*0.1f;
            seed = SeedRNG.Next(seed);
            data.z = ((float)(seed % 20)-10)*0.1f;
			return data;
		}

        public static Quaternion rotationUniform(ref UInt32 seed)
		{
			Vector3 data = Vector3.zero;
            seed = SeedRNG.Next(seed);
            data.x = ((float)(seed % 20)-10)*0.1f;
            seed = SeedRNG.Next(seed);
            data.y = ((float)(seed % 20)-10)*0.1f;
            seed = SeedRNG.Next(seed);
            data.z = ((float)(seed % 20)-10)*0.1f;
            seed = SeedRNG.Next(seed);
			return new Quaternion(data.x, data.y , data.z, ((float)(seed % 20)-10)*0.1f);
		}
    }
}