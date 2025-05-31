using System;
using InsaneScatterbrain.ScriptGraph;
using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace InsaneScatterbrain.MapGraph.Editor
{
    public static class ScriptNodeViewExtensions
    {
        /// <summary>
        /// Adds a default preview image element to the script node view.
        /// </summary>
        /// <param name="nodeView">The script node view.</param>
        /// <returns>The image element.</returns>
        public static Image AddPreviewImage(this ScriptNodeView nodeView)
        {
            var imageContainer = new VisualElement();
            return nodeView.AddPreviewImage(imageContainer);
        }
        
        public static Image AddPreviewImage(this ScriptNodeView nodeView, out VisualElement imageContainer)
        {
            imageContainer = new VisualElement();
            return nodeView.AddPreviewImage(imageContainer);
        }

        private static Image AddPreviewImage(this ScriptNodeView nodeView, VisualElement imageContainer)
        {
            imageContainer.style.backgroundColor = Color.gray;
            imageContainer.style.alignItems = Align.Center;
            
            var previewImage = new Image {scaleMode = ScaleMode.ScaleToFit};
            previewImage.name = "preview-image";
            previewImage.style.width = 200;
            previewImage.style.height = 200;
            
            imageContainer.Add(previewImage);

            nodeView.mainContainer.Add(imageContainer);
            return previewImage;
        }

        /// <summary>
        /// Sets a texture to the image element.
        /// </summary>
        /// <param name="nodeView">The script node view.</param>
        /// <param name="previewImage">The image element.</param>
        /// <param name="texture">The texture.</param>
        public static void UpdatePreviewImage(this ScriptNodeView nodeView, Image previewImage, Texture2D texture)
        {
            previewImage.image = texture;
        }

        public static Action<ProcessorNode> RegisterUpdateImage<TNode>(this ScriptNodeView nodeView, IScriptNode node, Image previewImage, Func<TNode, Texture2D> getTexture) where TNode : ProcessorNode
        {
            void ProcessingComplete(ProcessorNode nodeInstance)
            {
                if (nodeInstance.Id != node.Id) return;

                var texture = getTexture((TNode)nodeInstance);

                if (texture == null) return;

                nodeView.UpdatePreviewImage(previewImage, texture);
            }

            ProcessorNode.NodeProcessingCompleted += ProcessingComplete;

            return ProcessingComplete;
        }

        public static void UnregisterUpdateImage(this ScriptNodeView nodeView, Action<ProcessorNode> processingCompleteAction)
        {
            ProcessorNode.NodeProcessingCompleted -= processingCompleteAction;
        }

        public static void AddPreviewImage<TNode>(this ScriptNodeView nodeView, Func<TNode, Texture2D> getTexture) where TNode : ProcessorNode
        {
            var image = nodeView.AddPreviewImage();
            nodeView.RegisterUpdateImage(nodeView.Node, image, getTexture);
        }
        
        public static void AddPreview<TNode>(this ScriptNodeView nodeView, Func<TNode, Texture2D> getTexture) where TNode : ProcessorNode
        {
            var image = nodeView.AddPreviewImage();

            var processingCompleteAction = nodeView.RegisterUpdateImage(nodeView.Node, image, getTexture);
            nodeView.OnDispose += () =>
            {
                if (processingCompleteAction == null)
                {
                    return;
                }
                
                nodeView.UnregisterUpdateImage(processingCompleteAction);
            };
        }
    }
}