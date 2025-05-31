using UnityEditor;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    public class OutputParameterResultWindow : EditorWindow
    {
        private ScriptGraphRunner runner;
        private string outputName;

        public static void ShowResult(ScriptGraphRunner runner, string outputName)
        {
            var window = CreateInstance<OutputParameterResultWindow>();
            window.runner = runner;
            window.outputName = outputName;
            window.titleContent = new GUIContent(outputName);
            window.Show();
            
            // Set the window size
            var size = new Vector2(500, 500);
            
            // And place in the center
            var center = new Vector2(Screen.currentResolution.width / 2f, Screen.currentResolution.height / 2f);
            
            window.position = new Rect(center - size / 2f, size);
            
            window.Focus();
        }

        private void OnEnable()
        {
            EditorApplication.update += Repaint;
        }
        
        private void OnDisable()
        {
            EditorApplication.update -= Repaint;
        }

        private void OnGUI()
        {
            // On Escape, close the window.
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                Close();
            }
            
            var style = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true
            };
            
            var resultText = OutputParameterResultFormatter.Format(runner.LatestResult, outputName);
            EditorGUILayout.SelectableLabel(resultText, style, GUILayout.ExpandHeight(true));
            
            // Close button
            if (GUILayout.Button("Close"))
            {
                Close();
            }
        }
    }
}