using System;
using InsaneScatterbrain.RandomNumberGeneration;
using InsaneScatterbrain.ScriptGraph;
using UnityEngine;
using Rng = InsaneScatterbrain.Services.Rng;

namespace InsaneScatterbrain.MapGraph
{
    [ScriptNode("Noise", "Noise"), Serializable]
    public class NoiseNode : ProcessorNode
    {
        [InPort("Size", typeof(Vector2Int), true), SerializeReference] 
        private InPort sizeIn = null;
        
        [InPort("Noise Type", typeof(FastNoiseLite.NoiseType)), SerializeReference]
        private InPort noiseType = null;
        
        [InPort("Frequency", typeof(float)), SerializeReference] 
        private InPort frequencyIn = null;
        
        [InPort("Invert", typeof(bool)), SerializeReference]
        private InPort invertIn = null;
        
        [InPort("Fractal Type", typeof(FractalType)), SerializeReference]
        private InPort fractalTypeIn = null;
        
        [InPort("Fractal Octaves", typeof(int)), SerializeReference]
        private InPort fractalOctavesIn = null;
        
        [InPort("Fractal Lacunarity", typeof(float)), SerializeReference]
        private InPort fractalLacunarityIn = null;
        
        [InPort("Fractal Gain", typeof(float)), SerializeReference]
        private InPort fractalGainIn = null;
        
        [InPort("Fractal Weighted Strength", typeof(float)), SerializeReference]
        private InPort fractalWeightedStrengthIn = null;
        
        [InPort("Fractal Ping Pong Strength", typeof(float)), SerializeReference]
        private InPort fractalPingPongStrengthIn = null;

        [InPort("Cellular Distance Function", typeof(FastNoiseLite.CellularDistanceFunction)), SerializeReference]
        private InPort cellularDistanceFunctionIn = null;
        
        [InPort("Cellular Return Type", typeof(FastNoiseLite.CellularReturnType)), SerializeReference]
        private InPort cellularReturnTypeIn = null;
        
        [InPort("Cellular Jitter", typeof(float)), SerializeReference]
        private InPort cellularJitterIn = null;
        
        [InPort("Domain Warp Type", typeof(DomainWarpType)), SerializeReference]
        private InPort domainWarpTypeIn = null;
        
        [InPort("Domain Warp Amplitude", typeof(float)), SerializeReference]
        private InPort domainWarpAmpIn = null;

        [InPort("Domain Warp Frequency", typeof(float)), SerializeReference]
        private InPort domainWarpFrequencyIn = null;
        
        [InPort("Domain Warp Fractal Type", typeof(DomainWarpFractalType)), SerializeReference]
        private InPort domainWarpFractalTypeIn = null;
        
        [InPort("Domain Warp Fractal Octaves", typeof(int)), SerializeReference]
        private InPort domainWarpFractalOctavesIn = null;
        
        [InPort("Domain Warp Fractal Lacunarity", typeof(float)), SerializeReference]
        private InPort domainWarpFractalLacunarityIn = null;
        
        [InPort("Domain Warp Fractal Gain", typeof(float)), SerializeReference]
        private InPort domainWarpFractalGainIn = null;

        [InPort("Coordinates", typeof(Vector2Int)), SerializeReference]
        private InPort coordinatesIn = null;

        [InPort("Points Per Unit", typeof(Vector2Int)), SerializeReference]
        private InPort ppuIn = null;
        
        [InPort("Z", typeof(float)), SerializeReference]
        private InPort zIn = null;
        
        [InPort("Z Points Per Unit", typeof(float)), SerializeReference]
        private InPort zPpuIn = null;
        
        [InPort("Rotation Type 3D", typeof(FastNoiseLite.RotationType3D)), SerializeReference]
        private InPort rotationType3dIn = null;
        
        [InPort("Domain Warp Rotation Type 3D", typeof(FastNoiseLite.RotationType3D)), SerializeReference]
        private InPort domainWarpRotationType3dIn = null;

        [InPort("RNG State", typeof(RngState)), SerializeReference]
        private InPort rngStateIn = null;


        [OutPort("Noise Data", typeof(float[])), SerializeReference] 
        private OutPort noiseDataOut = null;
        
        
        private Vector2Int size;
        private float[] noiseData;
        
#if UNITY_EDITOR
        public float[] NoiseData => noiseData;
        public Vector2Int Size => size;
#endif
        
        protected override void OnProcess()
        {
            var noise = Get<FastNoiseLite>();
            var warpNoise = Get<FastNoiseLite>();
            
            var rng = Get<Rng>();
            if (rngStateIn.IsConnected)
            {
                rng.SetState(rngStateIn.Get<RngState>());
            }

            var seed = rng.Next();
            
            if (noiseType.IsConnected) noise.SetNoiseType(noiseType.Get<FastNoiseLite.NoiseType>());
            if (rotationType3dIn.IsConnected) noise.SetRotationType3D(domainWarpRotationType3dIn.Get<FastNoiseLite.RotationType3D>());
            noise.SetSeed(seed);
            if (frequencyIn.IsConnected) noise.SetFrequency(frequencyIn.Get<float>());
            if (fractalTypeIn.IsConnected) noise.SetFractalType(FastNoiseEnum.Convert(fractalTypeIn.Get<FractalType>()));
            if (fractalOctavesIn.IsConnected) noise.SetFractalOctaves(fractalOctavesIn.Get<int>());
            if (fractalLacunarityIn.IsConnected) noise.SetFractalLacunarity(fractalLacunarityIn.Get<float>());
            if (fractalGainIn.IsConnected) noise.SetFractalGain(fractalGainIn.Get<float>());
            if (fractalWeightedStrengthIn.IsConnected) noise.SetFractalWeightedStrength(fractalWeightedStrengthIn.Get<float>());
            if (fractalPingPongStrengthIn.IsConnected) noise.SetFractalPingPongStrength(fractalPingPongStrengthIn.Get<float>());
            
            if (cellularDistanceFunctionIn.IsConnected) noise.SetCellularDistanceFunction(cellularDistanceFunctionIn.Get<FastNoiseLite.CellularDistanceFunction>());
            if (cellularReturnTypeIn.IsConnected) noise.SetCellularReturnType(cellularReturnTypeIn.Get<FastNoiseLite.CellularReturnType>());
            if (cellularJitterIn.IsConnected) noise.SetCellularJitter(cellularJitterIn.Get<float>());

            warpNoise.SetSeed(seed);
            var domainWarpType = domainWarpTypeIn.Get<DomainWarpType>();
            if (domainWarpTypeIn.IsConnected && domainWarpType != DomainWarpType.None) warpNoise.SetDomainWarpType(FastNoiseEnum.Convert(domainWarpType));
            if (domainWarpRotationType3dIn.IsConnected) warpNoise.SetRotationType3D(domainWarpRotationType3dIn.Get<FastNoiseLite.RotationType3D>());
            if (domainWarpAmpIn.IsConnected) warpNoise.SetDomainWarpAmp(domainWarpAmpIn.Get<float>());
            if (domainWarpFrequencyIn.IsConnected) warpNoise.SetFrequency(domainWarpFrequencyIn.Get<float>());
            if (domainWarpFractalTypeIn.IsConnected) warpNoise.SetFractalType(FastNoiseEnum.Convert(domainWarpFractalTypeIn.Get<DomainWarpFractalType>()));
            if (domainWarpFractalOctavesIn.IsConnected) warpNoise.SetFractalOctaves(domainWarpFractalOctavesIn.Get<int>());
            if (domainWarpFractalLacunarityIn.IsConnected) warpNoise.SetFractalLacunarity(domainWarpFractalLacunarityIn.Get<float>());
            if (domainWarpFractalGainIn.IsConnected) warpNoise.SetFractalGain(domainWarpFractalGainIn.Get<float>());
            
            var z = zIn.Get<float>();
            
            var invert = invertIn.Get<bool>();
            var coordinates = coordinatesIn.Get<Vector2Int>();
            
            size = sizeIn.Get<Vector2Int>();
            var ppu = ppuIn.Get<Vector2Int>();

            var ppuWidth = ppuIn.IsConnected ? ppu.x : size.x;
            var ppuHeight = ppuIn.IsConnected ? ppu.y : size.y;
            var ppuDepth = zPpuIn.Get<float>();

            noiseData = new float[size.x * size.y];
            
            for (var x = 0; x < size.x; x++)
            for (var y = 0; y < size.y; y++)
            {
                var noiseX = (x + coordinates.x) / (float) ppuWidth;
                var noiseY = (y + coordinates.y) / (float) ppuHeight;
                var noiseZ = z / ppuDepth;
                
                if (domainWarpType != DomainWarpType.None)
                {
                    if (zIn.IsConnected)
                    {
                        warpNoise.DomainWarp(ref noiseX, ref noiseY, ref noiseZ);
                    }
                    else
                    {
                        warpNoise.DomainWarp(ref noiseX, ref noiseY);
                    }
                }

                if (zIn.IsConnected)
                {
                    noiseData[y * size.x + x] = noise.GetNoise(noiseX, noiseY, noiseZ) * .5f + .5f;
                }
                else
                {
                    noiseData[y * size.x + x] = noise.GetNoise(noiseX, noiseY) * .5f + .5f;
                }

                if (invert)
                {
                    noiseData[y * size.x + x] = 1 - noiseData[y * size.x + x];
                }
            }

            noiseDataOut.Set(() => noiseData);
        }
    }
}