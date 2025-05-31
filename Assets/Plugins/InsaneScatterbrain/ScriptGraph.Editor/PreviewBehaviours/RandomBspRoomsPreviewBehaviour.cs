using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.Services;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    public class RandomBspRoomsPreviewBehaviour : IPreviewBehaviour
    {
        public bool IsCompatibleWith(ScriptGraphGraph graph)
        {
            if (!graph.InputParameters.ContainsName("Size")) return false;
            if (!graph.OutputParameters.ContainsName("Rectangles")) return false;
            return true; 
        }

        public Texture2D GetPreviewTexture(ProcessGraphNode nodeInstance)
        {
            var size = nodeInstance.GetInPort("Size").Get<Vector2Int>();
            var rects = nodeInstance.GetOutPort("Rectangles").Get<RectInt[]>();
            
            var previewTexture = Texture2DFactory.CreateDefault(size.x, size.y);
            previewTexture.Fill(Color.black);
            
            previewTexture.DrawRects(rects, Color.white);
            previewTexture.Apply();
            
            return previewTexture;
        }
    }
}