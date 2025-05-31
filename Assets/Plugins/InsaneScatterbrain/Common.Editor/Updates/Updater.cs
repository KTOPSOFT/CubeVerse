using System;
using System.Collections.Generic;
using System.IO;
using InsaneScatterbrain.Editor.Services;
using InsaneScatterbrain.Extensions;
using InsaneScatterbrain.Versioning;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InsaneScatterbrain.Editor.Updates
{
    /// <summary>
    /// Class to update the data of versioned objects to conform to the latest version.
    /// </summary>
    public abstract class Updater
    {
        /// <summary>
        /// Gets all the available update actions for this updater.
        /// </summary>
        protected abstract IEnumerable<UpdateAction> UpdateActions { get; }
        
        /// <summary>
        /// Gets the tile displayed on the progress bar that's shown while updating.
        /// </summary>
        protected abstract string ProgressBarTitle { get; }
        
        /// <summary>
        /// Sets the content displayed on the progress bar that's shown while updating.
        /// </summary>
        protected abstract string ProgressBarText { get; }

        private List<UpdateAction> updateActionsToPerform;

        private int activeActionIndex;

        private static Updater instance;

        /// <summary>
        /// Event that's triggered whenever the updater completes updating.
        /// </summary>
        public event Action OnUpdateComplete;

        /// <summary>
        /// Gets the latest version that requires update actions to be performed.
        /// </summary>
        public Version LatestUpdateVersion { get; private set; }

        public void Initialize()
        {
            updateActionsToPerform = new List<UpdateAction>();
            
            foreach (var updateAction in UpdateActions)
            {
                updateActionsToPerform.Add(updateAction);
                
                if (LatestUpdateVersion == null || LatestUpdateVersion < updateAction.Version)
                {
                    LatestUpdateVersion = updateAction.Version;
                }
            }
        }
        
        /// <summary>
        /// Performs all update actions required to conform to the latest version.
        /// </summary>
        public void Update()
        {
            if (updateActionsToPerform.Count < 1) return;    // Nothing to update.
            
            updateActionsToPerform.Sort();
            
            try
            {
                for (var i = 0; i < updateActionsToPerform.Count; ++i)
                {
                    activeActionIndex = i;
                    var action = updateActionsToPerform[i];
                    action.UpdateAssets();
                }

                // After the required assets have been updated, make sure that all assets get the latest version number.
                var versionedAssets = Assets.Find<VersionedScriptableObject>();
                foreach (var asset in versionedAssets)
                {
                    asset.Version = LatestUpdateVersion;
                    EditorUtility.SetDirty(asset); 
                }
            
                AssetDatabase.SaveAssets();

                EditorApplication.update += UpdateScenes;
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                e.Rethrow();
            }
        }

        /// <summary>
        /// Sets the progress of the update, to display on the progress bar, this should be called from the associated
        /// update actions.
        /// </summary>
        /// <param name="progress">The current progress made.</param>
        public void SetActionProgress(float progress)
        {
            var progressPerAction = 1f / updateActionsToPerform.Count;
            var totalProgress = progressPerAction * activeActionIndex + progress * progressPerAction;
            EditorUtility.DisplayProgressBar(ProgressBarTitle, ProgressBarText, totalProgress);
        }

        private void UpdateScenes()
        {
            try
            {
                EditorApplication.update -= UpdateScenes;

                // Save the scenes before closing them, to make sure no unsaved changes are lost.
                EditorSceneManager.SaveOpenScenes();
                
                // Store info about which scenes are currently loaded so they can be restored when we're done.
                var currentScenePaths = new List<string>();
                var unloadedScenes = new HashSet<string>();
                for (var i = 0; i < SceneManager.sceneCount; ++i)
                {
                    var scene = SceneManager.GetSceneAt(i);
                    
                    if (string.IsNullOrEmpty(scene.path)) continue;
                    
                    currentScenePaths.Add(scene.path);

                    if (!scene.isLoaded)
                    {
                        unloadedScenes.Add(scene.path); 
                    }
                }
                
                // Get all the scenes.
                var sceneFiles = Directory.GetFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories);

                foreach (var sceneFile in sceneFiles)
                {
                    // Open each scene.
                    EditorSceneManager.OpenScene(sceneFile);
                    
                    // Perform the update actions on them.
                    foreach (var action in updateActionsToPerform)
                    {
                        action.UpdateScene();
                    }
                    
                    // After the required mono behaviours have been updated in this seen, make sure all of versioned mono behaviours get the latest version number.
                    var versionedBehaviours = Resources.FindObjectsOfTypeAll<VersionedMonoBehaviour>();
                    foreach (var behaviour in versionedBehaviours)
                    {
                         behaviour.Version = LatestUpdateVersion;
                         EditorUtility.SetDirty(behaviour);
                    }

                    // And save them so the update is persisted.
                    EditorSceneManager.SaveOpenScenes();
                }
                
                // Updating scenes is done. Open the scenes that the user had open before.
                Scene firstScene = default;
                for (var i = 0; i < currentScenePaths.Count; ++i)
                {
                    var scenePath = currentScenePaths[i];
                    var scene = EditorSceneManager.OpenScene(scenePath, 
                        i == 0 
                            ? OpenSceneMode.Single 
                            : unloadedScenes.Contains(scenePath) 
                                ? OpenSceneMode.AdditiveWithoutLoading 
                                : OpenSceneMode.Additive);

                    if (i == 0)
                    {
                        firstScene = scene;
                    }
                }

                // Make sure that any unloaded scenes are put back into their unloaded state.
                if (currentScenePaths.Count > 0 && unloadedScenes.Contains(currentScenePaths[0]))
                {
                    EditorSceneManager.CloseScene(firstScene, false); 
                }
                
                EditorUtility.ClearProgressBar();

                OnUpdateComplete?.Invoke();

                EditorUtility.DisplayDialog(ProgressBarTitle, "Please restart Unity to finish the update.", "OK");
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                e.Rethrow();
            }
        }
    }
}