using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    [ConstantNodeView(typeof(Color32))]
    public class ColorConstantNodeView : ConstantNodeView
    {
        public ColorConstantNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            var colorValue = (Color32) node.Value;

            var colorField = new ColorField
            {
                showAlpha = false, 
                hdr = false, 
                showEyeDropper = true
            };
            colorField.style.width = 50;
            colorField.SetValueWithoutNotify(colorValue);
            colorField.RegisterValueChangedCallback(e =>
            {
                var newColor = (Color32) e.newValue;
                newColor.a = 255;    // Make sure the color is always opaque.
                node.Value = newColor;
                EditorUtility.SetDirty(graphView.Graph);
            });
            inputContainer.Add(colorField);
        }
    }
}