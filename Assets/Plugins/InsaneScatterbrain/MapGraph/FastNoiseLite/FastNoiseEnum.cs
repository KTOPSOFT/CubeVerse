namespace InsaneScatterbrain.MapGraph
{
    public static class FastNoiseEnum
    {
        public static FastNoiseLite.FractalType Convert(FractalType fractalType)
        {
            switch (fractalType)
            {
                case FractalType.None:
                    return FastNoiseLite.FractalType.None;
                case FractalType.FBm:
                    return FastNoiseLite.FractalType.FBm;
                case FractalType.Ridged:
                    return FastNoiseLite.FractalType.Ridged;
                case FractalType.PingPong:
                    return FastNoiseLite.FractalType.PingPong;
                default:
                    return FastNoiseLite.FractalType.None;
            }
        }
        
        public static FastNoiseLite.FractalType Convert(DomainWarpFractalType domainWarpFractalType)
        {
            switch (domainWarpFractalType)
            {
                case DomainWarpFractalType.None:
                    return FastNoiseLite.FractalType.None;
                case DomainWarpFractalType.DomainWarpProgressive:
                    return FastNoiseLite.FractalType.DomainWarpProgressive;
                case DomainWarpFractalType.DomainWarpIndependent:
                    return FastNoiseLite.FractalType.DomainWarpIndependent;
                default:
                    return FastNoiseLite.FractalType.None;
            }
        }
        
        public static FastNoiseLite.DomainWarpType Convert(DomainWarpType domainWarpType)
        {
            switch (domainWarpType)
            {
                case DomainWarpType.None:
                    throw new System.ArgumentException("Can't convert None to DomainWarpType");
                case DomainWarpType.OpenSimplex2:
                    return FastNoiseLite.DomainWarpType.OpenSimplex2;
                case DomainWarpType.OpenSimplex2Reduced:
                    return FastNoiseLite.DomainWarpType.OpenSimplex2Reduced;
                case DomainWarpType.BasicGrid:
                    return FastNoiseLite.DomainWarpType.BasicGrid;
                default:
                    throw new System.ArgumentException("Invalid DomainWarpType");
            }
        }
    }
}