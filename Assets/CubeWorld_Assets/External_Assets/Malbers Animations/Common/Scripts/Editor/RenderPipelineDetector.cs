#if UNITY_EDITOR
using MalbersAnimations.Scriptables;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace MalbersAnimations
{
    [InitializeOnLoad]
    public class RenderPipelineDetector : Editor
    {
        static StringVar RenderVar;

        static RenderPipelineDetector()
        {
            DetectRenderPipeline();
        }

        static void DetectRenderPipeline()
        {
            if (RenderVar == null)
            {
                RenderVar = MTools.GetInstance<StringVar>("Render Pipeline");

                if (RenderVar != null && string.IsNullOrEmpty(RenderVar.Value)) //Change only the first time when RenderVar is Empty
                {
                    if (GraphicsSettings.defaultRenderPipeline != null)
                    {
                        var renderPipelineAsset = GraphicsSettings.defaultRenderPipeline;
                        RenderVar.Value = renderPipelineAsset.GetType().ToString();

                        if (renderPipelineAsset.GetType().ToString().Contains("UniversalRender"))
                        {
                            Debug.Log("Using Universal Render Pipeline (URP)");
                            if (EditorUtility.DisplayDialog("Using Universal Render Pipeline (URP)", "URP was detected. Please go to the Menu Tools/MalbersAnimations and install the correct URP Version of the Malbers Shaders", "OK"))
                            {

                            }
                        }
                        else if (renderPipelineAsset.GetType().ToString().Contains("HDRenderPipelineAsset"))
                        {
                            Debug.Log("Using High Definition Render Pipeline (HDRP)");

                            if (EditorUtility.DisplayDialog("Using High Definition Render Pipeline (HDRP)", "HDRP was detected. Please go to the Menu Tools/MalbersAnimations and install the correct HDRP version of the Malbers Shaders", "OK"))
                            {

                            }
                        }
                        else
                        {
                            Debug.Log("Using a custom Render Pipeline");
                        }
                    }
                    else
                    {
                        Debug.Log("Using Built-in Render Pipeline");
                        RenderVar.Value = "Built-in Render";
                    }
                }
            }
        }
    }
}
#endif