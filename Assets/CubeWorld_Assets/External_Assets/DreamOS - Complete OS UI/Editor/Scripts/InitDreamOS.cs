using UnityEngine;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine.Rendering;

namespace Michsky.DreamOS
{
	public class InitGlassOS : MonoBehaviour
	{
		[InitializeOnLoad]
		public class InitOnLoad
		{
			static InitOnLoad()
			{
				if (EditorPrefs.HasKey("DreamOSv1.Installed") && !EditorPrefs.HasKey("DreamOSv2.Installed"))
				{
					EditorPrefs.SetInt("DreamOSv2.Installed", 1);
					EditorUtility.DisplayDialog("Hello there!", "Thank you for upgrading DreamOS." +
						"\r\rIf you need help, feel free to contact us through our support channels or Discord.", "Got it");
				}

				else if (!EditorPrefs.HasKey("DreamOSv2.Installed"))
				{
					EditorPrefs.SetInt("DreamOSv2.Installed", 1);
					EditorUtility.DisplayDialog("Hello there!", "Thank you for purchasing DreamOS." +
						"\r\rIf you need help, feel free to contact us through our support channels or Discord.", "Got it");
				}

				if (!EditorPrefs.HasKey("DreamOS.HasCustomEditorData"))
				{
					EditorPrefs.SetInt("DreamOS.HasCustomEditorData", 1);

					string mainPath = AssetDatabase.GetAssetPath(Resources.Load("UI Manager/UI Manager"));
					mainPath = mainPath.Replace("Resources/UI Manager/UI Manager.asset", "").Trim();
					string darkPath = mainPath + "Editor/Glass Skin Dark.guiskin";
					string lightPath = mainPath + "Editor/Glass Skin Light.guiskin";

					EditorPrefs.SetString("DreamOS.CustomEditorDark", darkPath);
					EditorPrefs.SetString("DreamOS.CustomEditorLight", lightPath);
				}

				if (!EditorPrefs.HasKey("DreamOS.PipelineUpgrader") && GraphicsSettings.defaultRenderPipeline != null)
				{
					EditorPrefs.SetInt("DreamOS.PipelineUpgrader", 1);

					if (EditorUtility.DisplayDialog("DreamOS SRP Upgrader", "It looks like your project is using URP/HDRP rendering pipeline, " +
						"would you like to upgrade DreamOS UI Manager for your project?" +
						"\r\rNote that the blur shader is currently incompatible with URP/HDRP.", "Yes", "No"))
					{
						try
						{
							Preset defaultPreset = Resources.Load<Preset>("UI Manager/Presets/SRP Default");
							defaultPreset.ApplyTo(Resources.Load("UI Manager/UI Manager"));
						}

						catch { Debug.LogWarning("<b>[DreamOS]</b> Something went wrong while loading the SRP preset."); }
					}
				}
			}
		}
	}
}