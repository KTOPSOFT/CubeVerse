using System.Threading;
using InsaneScatterbrain.Dependencies;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <inheritdoc cref="ScriptGraphGraph"/>
    public class MapGraphGraph : ScriptGraphGraph
    {
        [SerializeField, HideInInspector] private NamedColorSet namedColorSet = default;

        /// <summary>
        /// Gets the named color set for this map graph.
        /// </summary>
        public NamedColorSet NamedColorSet
        {
            get => namedColorSet;
            set => namedColorSet = value;
        }

        public override void RegisterDependencies(DependencyContainer container)
        {
            base.RegisterDependencies(container);
            container.Register(() =>
            {
                if (namedColorSet == null)
                {
                    Debug.LogError("Named Color Set is not set on graph.");    
                }
                
                return namedColorSet;
            });

            var instanceProvider = container.Get<IInstanceProvider>();
            
            var areaExtractor = new ThreadLocal<AreaExtractor>(() => new AreaExtractor(instanceProvider.Get<Area>));
            container.Register(() => areaExtractor.Value);
            
            var rng = container.Get<Rng>();
            var gameObjectsFromTextureFactory = new ThreadLocal<GameObjectsFromTextureFactory>(
                () => new GameObjectsFromTextureFactory(rng));
            container.Register(() => gameObjectsFromTextureFactory.Value);
            
            var fastNoise = new ThreadLocal<FastNoiseLite>(() => new FastNoiseLite());
            container.Register(() =>
            {
                var fastNoiseInstance = fastNoise.Value;

                // (Re)set default values
                fastNoiseInstance.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
                fastNoiseInstance.SetFrequency(1);
                fastNoiseInstance.SetFractalType(FastNoiseLite.FractalType.None);
                fastNoiseInstance.SetSeed(1337);
                fastNoiseInstance.SetCellularJitter(1.0f);
                fastNoiseInstance.SetFractalGain(.5f);
                fastNoiseInstance.SetFractalLacunarity(2.0f);
                fastNoiseInstance.SetFractalOctaves(3);
                fastNoiseInstance.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.EuclideanSq);
                fastNoiseInstance.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance);
                fastNoiseInstance.SetDomainWarpAmp(1.0f);
                fastNoiseInstance.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2);
                fastNoiseInstance.SetRotationType3D(FastNoiseLite.RotationType3D.None);
                fastNoiseInstance.SetFractalPingPongStrength(2.0f);
                fastNoiseInstance.SetFractalWeightedStrength(0.0f);

                return fastNoiseInstance;
            });
        }
    }
}