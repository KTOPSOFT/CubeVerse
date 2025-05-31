using System.Collections.Generic;
using InsaneScatterbrain.ScriptGraph;
using UnityEditor;
using UnityEngine;

namespace InsaneScatterbrain.MapGraph.Editor
{
    /// <summary>
    /// Custom inspector for named set assets.
    /// </summary>
    [CustomEditor(typeof(NamedColorSet), true)]
    public class NamedColorSetEditor : UnityEditor.Editor
    {
        private NamedColorSetList namedColorSetList;
        private readonly List<IDataSet> setsToUnlink = new List<IDataSet>();

        private void OnEnable()
        {
            var namedColorSet = (NamedColorSet) target;
            
            namedColorSetList = new NamedColorSetList(namedColorSet);
            namedColorSetList.OnNew += NewColorCreated;
            namedColorSetList.OnRemove += ColorRemoved;
        }

        private void NewColorCreated(string newColorId)
        {
            var namedColorSet = (NamedColorSet) target;
            
            namedColorSet.MarkManuallyCreated(newColorId);
        }
        
        private void ColorRemoved(string removedColorId)
        {
            var namedColorSet = (NamedColorSet) target;
            
            namedColorSet.UnmarkManuallyCreated(removedColorId);
        }

        private void OnDisable()
        {
            namedColorSetList.Dispose();
        }

        public override void OnInspectorGUI()
        {
            namedColorSetList.DoLayoutList();

            GUILayout.Space(20);

            LinkedSets();
        }

        private void LinkedSets()
        {
            var evt = Event.current;
            
            var namedColorSet = (NamedColorSet) target;

            var bold = new GUIStyle(EditorStyles.label) {fontStyle = FontStyle.Bold};

            GUILayout.Label("Linked Sets", bold);
            
            // Show the list of linked sets, including a button to unlink each list.
            foreach (var set in namedColorSet.LinkedSets)
            {
                GUILayout.BeginHorizontal();
                var setObject = (Object) set;
                GUILayout.Label(setObject.name);
                if (GUILayout.Button("Unlink", GUILayout.Width(100)))
                {
                    setsToUnlink.Add(set);
                }
                GUILayout.EndHorizontal();
            }

            // If any of the sets need to be unlinked, do so now.
            if (setsToUnlink.Count > 0)
            {
                foreach (var set in setsToUnlink)
                {
                    namedColorSet.Unlink(set);
                }
                
                namedColorSetList.UpdateList();
                setsToUnlink.Clear();
            }
            
            // Create the drag drop zone for linking tilesets and prefab sets.
            GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MinHeight(100));
            
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            var labelStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleRight
            };
            GUILayout.Label("Drag or", labelStyle);
            
            // Add a button to open an object picker with a filter that only shows the Tileset and PrefabSets
            // as those are the only types that are compatible.
            if (GUILayout.Button("Pick a Set", GUILayout.MaxWidth(75)))
            {
                EditorGUIUtility.ShowObjectPicker<Object>(null, false, "t:Tileset,t:PrefabSet", 0);
            }
            GUILayout.Label("to link.");
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            
            GUILayout.EndVertical();
            
            var linkedSetsRect = GUILayoutUtility.GetLastRect();

            var invalidTypeSet = false;
            switch (evt.type)
            {
                case EventType.ExecuteCommand when evt.commandName == "ObjectSelectorClosed":
                {
                    // If the object picker was closed, get the selected object and link it if
                    // it's a Tileset or Prefab set.
                    var selectedObject = EditorGUIUtility.GetObjectPickerObject();

                    if (selectedObject == null) return; // No selection was made, so don't do anything.

                    if (selectedObject is Tileset || selectedObject is PrefabSet)
                    {
                        var dataSet = (IDataSet) selectedObject;
                        if (!namedColorSet.IsLinked(dataSet))
                        {
                            namedColorSet.Link(dataSet);
                            namedColorSetList.UpdateList();
                        }
                    }
                    else
                    {
                        // If it's not a compatible type, a warning is shown later on.
                        invalidTypeSet = true;
                    }

                    break;
                }
                case EventType.DragPerform when linkedSetsRect.Contains(evt.mousePosition):
                {
                    // A selection has been dropped into the drag and drop zone.
                    DragAndDrop.AcceptDrag();

                    foreach (var draggedObject in DragAndDrop.objectReferences)
                    {
                        // Check for each dragged object if it's compatible to link, if so, link them to
                        // the named color set.
                        if (!(draggedObject is Tileset) && !(draggedObject is PrefabSet)) continue;
                    
                        var typeSet = (IDataSet) draggedObject;
                    
                        if (namedColorSet.IsLinked(typeSet)) continue;
                    
                        namedColorSet.Link(typeSet);
                        namedColorSetList.UpdateList();
                    }

                    break;
                }
                case EventType.DragUpdated:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                    break;
            }

            serializedObject.ApplyModifiedProperties();

            if (invalidTypeSet)
            {
                // If the selected type set was not compatible, show an error message.
                EditorUtility.DisplayDialog(string.Empty, "Only Tilesets and Prefab Sets can be linked.", "OK");
            }
        }
    }
}