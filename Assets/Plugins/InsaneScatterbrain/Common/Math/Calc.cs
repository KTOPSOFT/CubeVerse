using System;

namespace InsaneScatterbrain
{
    public static class Calc
    {
        public static int Mod(int a, int b)
        {
            return (Math.Abs(a * b) + a) % b;
        }
    }
}