using UnityEngine;

namespace InsaneScatterbrain.MapGraph
{
    public static class TextureDataExtensions
    {
        public static void Scale(this TextureData originalTextureData, ref TextureData result, int newWidth, int newHeight)
        {
            result.Set(newWidth, newHeight); 
            
            var scaleX = (float)newWidth / originalTextureData.Width;
            var scaleY = (float)newHeight / originalTextureData.Height;
            
            for (var x = 0; x < newWidth; ++x)
            {
                for (var y = 0; y < newHeight; ++y)
                {
                    var originalX = Mathf.FloorToInt(x / scaleX);
                    var originalY = Mathf.FloorToInt(y / scaleY);
                    var originalIndex = originalTextureData.Width * originalY + originalX;

                    var color = originalTextureData[originalIndex];

                    var index = newWidth * y + x;
                    result[index] = color;
                }
            }
        }
    }
}