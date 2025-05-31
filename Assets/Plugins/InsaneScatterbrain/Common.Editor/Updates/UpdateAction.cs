using System;
using System.Reflection;
using InsaneScatterbrain.Versioning;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InsaneScatterbrain.Editor.Updates
{
    /// <summary>
    /// Action to execute to update data to the version associated with this action.
    /// </summary>
    public abstract class UpdateAction : IComparable<UpdateAction>
    {
        /// <summary>
        /// Gets the version to update to.
        /// </summary>
        public abstract Version Version { get; }
        
        /// <summary>
        /// Runs the action to update the current scene. 
        /// </summary>
        public abstract void UpdateScene();
        
        /// <summary>
        /// Runs the action to update the assets.
        /// </summary>
        public abstract void UpdateAssets();

        /// <summary>
        /// Save update changes to the object.
        /// </summary>
        /// <param name="obj">The object to update.</param>
        protected void Save(VersionedScriptableObject obj)
        {
            obj.Version = Version;

            EditorUtility.SetDirty(obj);
        }

        /// <summary>
        /// Save update changes to the object.
        /// </summary>
        /// <param name="obj">The object to update.</param>
        protected void Save(VersionedMonoBehaviour obj)
        {
            obj.Version = Version;

            EditorUtility.SetDirty(obj);
        }

        /// <summary>
        /// Gets field info of the given private field.
        /// </summary>
        /// <param name="fieldName">The private field's name.</param>
        /// <typeparam name="ClassType">The class containing the private field.</typeparam>
        /// <returns>The field info for the private field.</returns>
        protected static FieldInfo GetPrivateField<ClassType>(string fieldName)
        {
            var paramsInField = typeof(ClassType).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (paramsInField == null)
            {
                Debug.LogError($"Can't find \"{fieldName}\" to update: maybe it got renamed?");
            }

            return paramsInField;
        }

        public int CompareTo(UpdateAction other) => Version.CompareTo(other.Version);
    }
}