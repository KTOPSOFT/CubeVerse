using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    public class ScriptGraphDependencyInstaller
    {
        private List<string> packagesToInstall;
        
        private ListRequest listRequest;
        private AddRequest activeInstallRequest;

        public void StartInstallProcess()
        {
            packagesToInstall = new List<string>
            {
                "com.unity.editorcoroutines",
            };
            
            listRequest = Client.List();
            EditorApplication.update += ProcessPrepareInstallation;
        }

        private void ProcessPrepareInstallation()
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
                    packagesToInstall.Remove(package.packageId);
                }
            }

            EditorApplication.update -= ProcessPrepareInstallation;
            ProcessInstallRequest();
        }
        
        private void ProcessInstallRequest()
        {
            if (packagesToInstall.Count <= 0) return;
            
            activeInstallRequest = Client.Add(packagesToInstall[0]);
            EditorApplication.update += ProcessActiveInstallRequest;
        }
        
        private void ProcessActiveInstallRequest()
        {
            if (!activeInstallRequest.IsCompleted) return;

            if (activeInstallRequest.Status == StatusCode.Failure)
            {
                Debug.LogError(activeInstallRequest.Error.message);
            }

            EditorApplication.update -= ProcessActiveInstallRequest;
            packagesToInstall.RemoveAt(0);
            ProcessInstallRequest();
        }
    }
}