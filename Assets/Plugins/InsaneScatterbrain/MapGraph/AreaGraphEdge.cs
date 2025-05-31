using System;
using QuikGraph;

namespace InsaneScatterbrain.MapGraph
{
    public class AreaGraphEdge : IEdge<Area>
    {
        public AreaGraphEdge()
        {
        }

        [Obsolete("Please use the pool manager to get a AreaGraphEdge instance. This constructor will probably be removed in version 2.0. Alternatively, use the parameterless constructor and call the Set method.")]
        public AreaGraphEdge(Area source, Area target) => Set(source, target);

        public void Set(Area source, Area target)
        {
            Source = source;
            Target = target;
        }
        
        public Area Source { get; private set; }
        public Area Target { get; private set; }
    }
}