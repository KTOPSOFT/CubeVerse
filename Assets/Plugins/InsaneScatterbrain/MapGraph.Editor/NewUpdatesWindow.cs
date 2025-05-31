using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    /// <summary>
    /// Window to show if there is a newer version of Map Graph available.
    /// </summary>
    public class NewUpdatesWindow : EditorWindow
    {
        private const string AssetStoreUrl = "content/177023#releases";
        private const string ChangelogUrl = "https://mapgraph.insanescatterbrain.com/changelog.html";
        
        public static void ShowWindow()
        {
            GetWindow<NewUpdatesWindow>(true, "Map Graph: New Version").Show();
        }

        private void OnGUI()
        {
            var size = new Vector2(250, 260);
            minSize = size;
            maxSize = size;

            var boxStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                alignment = TextAnchor.MiddleCenter
            };

            var warningBoxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };
            
            var headerStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16 
            };

            var bigButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 40, 
                fontSize = 14
            };
            
            var buttonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 25, 
                fontSize = 12
            };
            
            GUILayout.Label("A new version of Map Graph is available!", headerStyle);
            GUILayout.FlexibleSpace();

            var backupWarningContent = EditorGUIUtility.IconContent("Warning@2x");
            var defaultColor = GUI.contentColor;
            backupWarningContent.text = "Always backup your project before updating!";
            
            GUI.contentColor = new Color32(255, 193, 7, 255);
            GUILayout.Box(backupWarningContent, warningBoxStyle);
            GUI.contentColor = defaultColor;
            
            GUILayout.Space(10);
            GUILayout.Box("Check out the changelog for more details.", boxStyle);
            GUILayout.Space(10);
            if (GUILayout.Button("View changelog", buttonStyle))
            {
                Application.OpenURL(ChangelogUrl);
            }
            if (GUILayout.Button("View in Asset Store", buttonStyle))
            {
                AssetStore.Open(AssetStoreUrl);
            }
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Open Package Manager", bigButtonStyle))
            {
                UnityEditor.PackageManager.UI.Window.Open("Map Graph"); 
            }
        }
    }
}