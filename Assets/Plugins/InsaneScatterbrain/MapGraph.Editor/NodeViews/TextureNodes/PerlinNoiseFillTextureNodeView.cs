using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace InsaneScatterbrain.MapGraph.Editor
{
    [ScriptNodeView(typeof(PerlinNoiseFillTextureNode))]
    public class PerlinNoiseFillTextureNodeView : ScriptNodeView
    {
        private bool showNoisePreview = false;
        private Button togglePreviewButton;
        
        private Texture2D previewTexture;
        private Texture2D noisePreviewTexture;
        
        private Image previewImage;
        
        public PerlinNoiseFillTextureNodeView(PerlinNoiseFillTextureNode fillTextureNode, ScriptGraphView graphView) : base(fillTextureNode, graphView)
        {
            // Add a button to the main container to toggle the preview image.
            togglePreviewButton = new Button(() =>
            {
                showNoisePreview = !showNoisePreview;

                if (togglePreviewButton == null || previewImage == null) return;
                
                togglePreviewButton.text = showNoisePreview ? "Show Texture Data" : "Show Noise Data";

                previewImage.image = showNoisePreview
                    ? noisePreviewTexture
                    : previewTexture;
            })
            {
                text = "Show Noise Data"
            };
            togglePreviewButton.style.marginBottom = 0;
            togglePreviewButton.style.marginLeft = 0;
            togglePreviewButton.style.marginRight = 0;
            togglePreviewButton.style.marginTop = 0;
            togglePreviewButton.style.borderLeftWidth = 0;
            togglePreviewButton.style.borderRightWidth = 0;
            togglePreviewButton.style.borderBottomLeftRadius = 0;
            togglePreviewButton.style.borderBottomRightRadius = 0;
            togglePreviewButton.style.borderTopLeftRadius = 0;
            togglePreviewButton.style.borderTopRightRadius = 0;
            
            mainContainer.Add(togglePreviewButton);

            this.AddPreview<PerlinNoiseFillTextureNode>(GetPreviewTexture);
            
            // Find the preview image element and store it.
            previewImage = this.Q<Image>("preview-image");
            
        }

        // private Texture2D GetPreviewTexture(PerlinNoiseFillTextureNode node) => node.NoiseTextureData.ToTexture2D();
        private Texture2D GetPreviewTexture(PerlinNoiseFillTextureNode node)
        {
            previewTexture = node.TextureData.ToTexture2D();
            noisePreviewTexture = node.NoiseTextureData.ToTexture2D();
            
            return showNoisePreview ? noisePreviewTexture : previewTexture;
        }
    }
}