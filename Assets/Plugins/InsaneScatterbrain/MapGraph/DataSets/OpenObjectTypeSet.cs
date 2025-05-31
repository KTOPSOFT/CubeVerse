using InsaneScatterbrain.ScriptGraph;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    /// <summary>
    /// A less restrictive version of the object type set, for protected use.
    /// </summary>
    /// <typeparam name="TType">The type of object type.</typeparam>
    /// <typeparam name="TEntry">The type of the entry.</typeparam>
    /// <typeparam name="TObject">The type of object stored in the entry.</typeparam>
    public abstract class OpenObjectTypeSet<TType, TEntry, TObject> : ObjectTypeSet<TType, TEntry, TObject>, IOpenDataSet<TType> 
        where TType : IObjectType<TEntry, TObject> 
        where TEntry : IObjectTypeEntry<TObject> where TObject : Object
    {
        /// <inheritdoc cref="IOpenDataSet{T}.Get"/>
        public new TType Get(string id)
        {
            return base.Get(id);
        }

        /// <inheritdoc cref="IOpenDataSet{T}.GetByName"/>
        public new TType GetByName(string name)
        {
            return base.GetByName(name);
        }
    }
}