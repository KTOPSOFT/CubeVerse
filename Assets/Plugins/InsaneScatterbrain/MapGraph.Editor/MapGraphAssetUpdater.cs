using System;
using InsaneScatterbrain.Editor.Services;
using InsaneScatterbrain.ScriptGraph.Editor;
using InsaneScatterbrain.Versioning;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InsaneScatterbrain.MapGraph.Editor
{
    public class MapGraphAssetUpdater
    {
        private MapGraphUpdater updater;
        
        public void Start()
        {
            updater = new MapGraphUpdater();
            updater.Initialize();
            // Make sure that update system is triggered now and whenever another scene is loaded. To check
            // if there are objects that aren't updated to the latest version.
            RegisterInitializeUpdateOnSceneLoad();
            InitializeUpdate();
        }
        
        private void InitializeUpdate(Scene scene, OpenSceneMode mode)
        {
            InitializeUpdate();
        }

        private void InitializeUpdate()
        {
            // Queue the update when ready.
            EditorApplication.update += StartUpdateWhenReady;
            
            // Make sure it's not queued again until the current process is complete.
            UnregisterInitializeOnSceneLoad();
        }

        private void RegisterInitializeUpdateOnSceneLoad()
        {
            EditorSceneManager.sceneOpened -= InitializeUpdate;
            EditorSceneManager.sceneOpened += InitializeUpdate;
        }

        private void UnregisterInitializeOnSceneLoad()
        {
            EditorSceneManager.sceneOpened -= InitializeUpdate;
        }

        private void StartUpdateWhenReady()
        {
            // Make sure everything else is ready, before updating.
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;

            EditorApplication.update -= StartUpdateWhenReady;
            
            Version lowestVersionFound = null;
            
            // Check all the versions of all the versioned mono behaviours in the current scene and find the lowest one.
            var versionedBehaviours = Resources.FindObjectsOfTypeAll<VersionedMonoBehaviour>();

            if (versionedBehaviours.Length > 0)
            {
                foreach (var versionedBehaviour in versionedBehaviours)
                {
                    if (lowestVersionFound != null && versionedBehaviour.Version >= lowestVersionFound) continue;
                
                    lowestVersionFound = versionedBehaviour.Version;
                }
            
                // If the lowest version is lower than the current version, it means that not everything is up-to-date yet
                // and the update process is started.
                if (lowestVersionFound == null || lowestVersionFound < updater.LatestUpdateVersion)
                {
                    StartUpdate();
                    return;
                }
            }

            // If the versioned mono behaviours are up-to-date, check the versioned scriptable object versions.
            var versionedAssets = Assets.Find<VersionedScriptableObject>();

            if (versionedAssets.Count > 0)
            {
                foreach (var versionedAsset in versionedAssets)
                {
                    if (lowestVersionFound != null && versionedAsset.Version >= lowestVersionFound) continue;

                    lowestVersionFound = versionedAsset.Version;
                }

                // Again, if there's a lower version than the current version, start updating.
                if (lowestVersionFound == null || lowestVersionFound < updater.LatestUpdateVersion)
                {
                    StartUpdate();
                    return;
                }
            }

            // No outdated objects are found, so the process is done and it's safe to re-enable the update check whenever
            // a new scene is loaded.
            RegisterInitializeUpdateOnSceneLoad();
        }

        private void StartUpdate() 
        {
            updater.OnUpdateComplete += () =>
            {
                // If the update process is complete, it's safe to re-enable the update check whenever a new scene is loaded.
                RegisterInitializeUpdateOnSceneLoad();

                ScriptGraphViewWindow.ReloadAll();
            };
            updater.Update();
        }
    }
}