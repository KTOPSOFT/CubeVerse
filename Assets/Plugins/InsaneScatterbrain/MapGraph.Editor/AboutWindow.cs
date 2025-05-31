using System;
using System.Collections.Generic;
using InsaneScatterbrain.ScriptGraph.Editor;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    public class AboutWindow : EditorWindow
    {
        private const string TwitterIconBase64 =
            "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAABmJLR0QA/wD/AP+gvaeTAAABzUlEQVQ4jaWSQWsTURSFv/dmdDraWEpdCNEIQotG48aFmxYXLtxZVyJSpJuqBXf+Af+BbkRqoQtLcWnxJ4iCoATaaiISxIKIJhrbppqJmfeui8lMpppa0LN4cC/3nHvevRf+E6pXMjfXyGPNuKBGoiJ5q8RZXJ3KlP8qcGSmPtDW+h5wqYe4AA8930xXJoY2/hDokJ8CJ3ZwveL5ZjQW0XG203knMkDhZ1Pf3eIgN9fIizGvAHXmoMuzjyGhjQpunupjMr+bl58Ny18Mt4sBgGic46tTmbIGEGMuxGJjWZcH5/aS7Y/MXT/pMeApzuZcvgU2aSyYcQA3CtUwIgC8W7dM5j2eXMxQrIZ4Ttd7qZ4IIMJwdwbSYQMvPoVUf1h2aTh9wE0Im23h9VfTVVNIeoiVOF9tCqW6YbMtpHGn2OJ7Kqc6HB09ziOiPTMy6FCqW9ZbknS+9Tzg/korrWe11YvJFgAOza4tAJe1gsJ+h2y/Zi0QijVDEG51IyLzH64OXukOEfB8M91qOgUrFJZqhqWaYRss9+2xN+IgOaTKxNCG55tRhSzE3/kNVkTmPd+M9TzlNA7PNo6JmPOi1VEAQcqu0Y/fX9v3Zjtb/4xfOwe7tBT0mWkAAAAASUVORK5CYII=";
        private const string DiscordIconBase64 =
            "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAB1UlEQVQ4jZWTz2sTYRCGn9ls/Y1rQa2xIGm1RwvdQg8tRQgIBc9iDh4E0+Ax4Nn/QQpesvWenAoFISDkErwITUrTPyAUqlbbwHoIsm53PHy7STZasO9xeJ935pvdEcZUWPddlJIKeZRcXO6K0ACpVL3rrVG/DMGfV1DdUOGlDMtjUhQ8oFzznP4gwMDUEVbPIMdjmsBazXP6lqno2/+FTVdZBTYApFD0XYSd5DWZDCw8tLl9U2jthQC48zbfj5V2J+T0NJ5CFYRFW6E0+uZXLy6xsmQD8PzpxVTnT59D3r3/ZaYQQVVLFpBPDNNZawD/SytLNtNZa7SUt2DwqQbwSU+pbgUEAQQBVLcCTnoKwHK6QS4VN5vLALC7H7JdDzg4jDg4jNiuB+zum33cjz2JbKALzAFcu2p28Wh5grt3LB7MmPw3ry8zN2vAxBOrawONJKDdCZm5d4HjnvLlKCKKjOvbj4jJGxZTt4R2JxziQkMKRd9V2BExydkpC3c+w4ePv1OjPnk8QWsv5OuRxhUFWBSAZ0W/IiLrnEOKejXPKSVLLKPaPAfeBMoAFkBt0+krrKmqh+rZmCqq6qmaOwD+PrtC0XeBkpofLBcbuphlV6qbTuqc/wAwW6w+yzHv8gAAAABJRU5ErkJggg==";
        private const string SamplesPackagePath = "Assets/Plugins/InsaneScatterbrain/MapGraph.Samples/Samples.unitypackage";
        
        private bool installingSample;
        private List<string> packagesToImport;
        private AddRequest activeSampleInstallRequest;
        private Version latestVersion;

        private Texture2D twitterIcon;
        private Texture2D TwitterIcon
        {
            get
            {
                if (twitterIcon == null)
                {
                    twitterIcon = GetTextureFromBase64(TwitterIconBase64);
                }

                return twitterIcon;
            }
        }
        
        private Texture2D discordIcon;
        private Texture2D DiscordIcon
        {
            get
            {
                if (discordIcon == null)
                {
                    discordIcon = GetTextureFromBase64(DiscordIconBase64);
                }

                return discordIcon;
            }
        }
        
        private Texture2D GetTextureFromBase64(string base64)
        {
            var texture = new Texture2D(1, 1)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.LoadImage(Convert.FromBase64String(base64));
            return texture;
        }

        [MenuItem("Tools/Map Graph")]
        public static AboutWindow ShowWindow()
        {
            var window = GetWindow<AboutWindow>(true, "Map Graph");
            window.Show();
            return window;
        }
        
        private void OnEnable()
        {
            if (activeSampleInstallRequest != null && !activeSampleInstallRequest.IsCompleted)
            {
                EditorApplication.update += ProcessActiveSampleInstallRequest;
            }
            else
            {
                ProcessSampleInstallRequest();
            }

            MapGraphEditorInfo.GetLatestVersion().ContinueWith(task => latestVersion = task.Result);
        }

        private void StartSampleInstallProcess()
        {
            installingSample = true;
            packagesToImport = new List<string>
            {
                "com.unity.2d.tilemap@1.0.0",
                "com.unity.2d.tilemap.extras@1.5.0-preview",
                "com.unity.2d.pixel-perfect@2.0.4"
            };

            StartPrepareSampleInstallation();
        }

        private ListRequest listRequest;

        private void StartPrepareSampleInstallation()
        {
            listRequest = Client.List();
            EditorApplication.update += ProcessPrepareSampleInstallation;
        }

        private void ProcessPrepareSampleInstallation()
        {
            if (!listRequest.IsCompleted) return;

            if (listRequest.Status == StatusCode.Failure)
            {
                Debug.LogError(listRequest.Error.message);
            }
            else
            {
                foreach (var package in listRequest.Result)
                {
                    packagesToImport.Remove(package.packageId);
                }
            }

            EditorApplication.update -= ProcessPrepareSampleInstallation;
            ProcessSampleInstallRequest();
        }
        
        private void ProcessSampleInstallRequest()
        {
            if (!installingSample) return;

            if (packagesToImport.Count > 0)
            {
                activeSampleInstallRequest = Client.Add(packagesToImport[0]);
                EditorApplication.update += ProcessActiveSampleInstallRequest;
            }
            else
            {
                AssetDatabase.ImportPackage(SamplesPackagePath, false);
                AssetDatabase.Refresh();
                installingSample = false;
            }
        }
        
        private void ProcessActiveSampleInstallRequest()
        {
            if (!activeSampleInstallRequest.IsCompleted) return;

            if (activeSampleInstallRequest.Status == StatusCode.Failure)
            {
                Debug.LogError(activeSampleInstallRequest.Error.message);
            }

            EditorApplication.update -= ProcessActiveSampleInstallRequest;
            packagesToImport.RemoveAt(0);
            ProcessSampleInstallRequest();
        }

        private void OnGUI()
        {
            var size = new Vector2(350, 623);
            minSize = size;
            maxSize = size;

            var bigRichButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 35, 
                fontSize = 18, 
                richText = true
            };

            var mediumButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 25, 
                fontSize = 14
            };

            var centerLabelStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter
            };

            var headerLabelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 20, 
                fontStyle = FontStyle.Bold, 
                alignment = TextAnchor.MiddleCenter
            };

            GUILayout.Space(20);
            GUILayout.Label("Thank you for using Map Graph!", headerLabelStyle);
            GUILayout.Space(30);
            
            var installedVersion = MapGraphEditorInfo.Version;

            var latestVersionText = latestVersion == null ? "Unknown" : latestVersion.ToString();
            GUILayout.Label($"Installed version: {installedVersion}", centerLabelStyle);
            GUILayout.Label($"Latest version: {latestVersionText}", centerLabelStyle); 
            
            GUILayout.Space(30);
            GUILayout.Label("If you like Map Graph, please consider leaving a review.\nIt really helps a lot!", centerLabelStyle);
            GUILayout.Space(15);
            
            if (GUILayout.Button("Leave a review <color=#d54930ff>❤</color>", bigRichButtonStyle)) 
            {
                Application.OpenURL(Urls.Reviews);
            }
            
            GUILayout.Space(40);
            
            if (GUILayout.Button("Open Editor Settings", mediumButtonStyle))
            {
                MapGraphEditorSettingsWindow.ShowWindow();
            }
            
            GUILayout.Space(40);

            GUILayout.Label("Documentation", headerLabelStyle);
            GUILayout.Space(5);

            if (GUILayout.Button("Getting Started", mediumButtonStyle))
            {
                Application.OpenURL(Urls.GettingStarted);
            }
            if (GUILayout.Button("Manual", mediumButtonStyle))
            {
                Application.OpenURL(Urls.OnlineDocs);
            }

            if (installingSample) GUI.enabled = false;
            var buttonText = installingSample ? "Installing Samples ..." : "Install Samples";
            if (GUILayout.Button(buttonText, mediumButtonStyle))
            {
                if (EditorUtility.DisplayDialog("Install Samples?",
                    "It's recommended to import the samples into an empty project.",
                    "Continue", "Cancel"))
                {
                    StartSampleInstallProcess();
                }

                GUIUtility.ExitGUI();
            }
            if (installingSample) GUI.enabled = true;

            GUILayout.Space(40);

            GUILayout.Label("Support", headerLabelStyle);
            GUILayout.Space(5);
            
            var twitterTitle = new GUIContent(" Twitter", TwitterIcon);
            if (GUILayout.Button(twitterTitle, mediumButtonStyle))
            {
                Application.OpenURL(Urls.TwitterProfile);
            }
            
            if (GUILayout.Button("Unity Forums", mediumButtonStyle))
            {
                Application.OpenURL(Urls.UnityForum);
            }

            if (GUILayout.Button("E-mail", mediumButtonStyle))
            {
                Application.OpenURL(Urls.SupportMail);
            }
            
            var discordTitle = new GUIContent(" Discord", DiscordIcon);
            if (GUILayout.Button(discordTitle, mediumButtonStyle))
            {
                Application.OpenURL(Urls.DiscordInviteUrl);
            }
        } 
    }
}