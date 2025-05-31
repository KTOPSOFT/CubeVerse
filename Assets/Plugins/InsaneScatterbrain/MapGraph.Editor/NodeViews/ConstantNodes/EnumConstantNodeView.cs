using System;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.ScriptGraph.Editor;
#if UNITY_2022_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEditor.UIElements;
#endif

namespace InsaneScatterbrain.MapGraph.Editor
{
    public abstract class EnumConstantNodeView<T> : ConstantNodeView where T : Enum
    {
        protected EnumConstantNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            var value = (T) node.Value;
            var field = AddDefaultField<Enum, EnumField>(value);
            field.Init(value);
        }
    }
}