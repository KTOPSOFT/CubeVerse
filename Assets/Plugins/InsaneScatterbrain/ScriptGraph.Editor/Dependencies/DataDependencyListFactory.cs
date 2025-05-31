using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    public class DataDependencyListFactory
    {
        private const string ScriptableObjectPropName = "scriptableObject";
        private const string SerializedTypePropName = "serializedType";
            
        private readonly SerializedProperty dataDependenciesProp;

        public DataDependencyListFactory(SerializedProperty dataDependenciesProp)
        {
            this.dataDependenciesProp = dataDependenciesProp;
        }
        
        public ReorderableList Create()
        {
            return new ReorderableList(
                dataDependenciesProp.serializedObject, 
                dataDependenciesProp, 
                false, true, true, true)
            {
                drawElementCallback = DrawElementCallback,
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, "Dependencies");
                },
                elementHeight = EditorGUIUtility.singleLineHeight * 2 + 18
            };
        }
        
        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var scriptableObject = AddObjectFields(rect, index);
            
            if (scriptableObject == null)
            {
                // No scriptableObject is selected, so we can't show any of its components.
                return;
            }

            AddTypeFields(rect, index, scriptableObject);
        }

        private ScriptableObject AddObjectFields(Rect rect, int index)
        {
            var dependencyProp = dataDependenciesProp.GetArrayElementAtIndex(index);
            var scriptableObjectProp = dependencyProp.FindPropertyRelative(ScriptableObjectPropName);
            var typeProp = dependencyProp.FindPropertyRelative(SerializedTypePropName);
            
            var yPos = rect.y + 6;
            
            EditorGUI.LabelField(
                new Rect(rect.x, yPos, rect.width / 2, EditorGUIUtility.singleLineHeight), "Scriptable Object");
            
            EditorGUI.BeginChangeCheck();
            scriptableObjectProp.objectReferenceValue = (ScriptableObject) EditorGUI.ObjectField(
                new Rect(rect.x + rect.width / 2, yPos, rect.width / 2, EditorGUIUtility.singleLineHeight),
                scriptableObjectProp.objectReferenceValue, typeof(ScriptableObject), true);

            if (EditorGUI.EndChangeCheck())
            {
                // If the scriptable object is changed, then the type should be reset.
                typeProp.stringValue = null;
                
                // If the newly selected scriptable object is not null, then set the type to the assembly qualified name
                // of the scriptable object's type.
                var scriptableObject = (ScriptableObject) scriptableObjectProp.objectReferenceValue;
                if (scriptableObject != null)
                {
                    typeProp.stringValue = scriptableObject.GetType().AssemblyQualifiedName;
                }
            }

            return (ScriptableObject) scriptableObjectProp.objectReferenceValue;
        }

        private void AddTypeFields(Rect rect, int index, ScriptableObject scriptableObject)
        {
            var dependencyProp = dataDependenciesProp.GetArrayElementAtIndex(index);
            var typeProp = dependencyProp.FindPropertyRelative(SerializedTypePropName);
            
            // Create a list of types that the dependency can be registered under. The options consist of the
            // scriptableObject's own type and all of its interfaces.
            var type = scriptableObject.GetType();
            var interfaces = type.GetInterfaces().OrderBy(i => i.Name).ToArray();
            
            var typeNames = new string[interfaces.Length + 1];
            
            typeNames[0] = type.Name;
            for (var i = 0; i < interfaces.Length; i++)
            {
                typeNames[i + 1] = interfaces[i].Name;
            }
            
            var selectedTypeIndex = 0;  // Select the scriptableObject's type by default.
            var selectedTypeName = typeProp.stringValue;
            for (var i = 0; i < interfaces.Length; i++)
            {
                if (selectedTypeName != interfaces[i].AssemblyQualifiedName)
                {
                    // If the selected type isn't an interface, then it must be the scriptableObject's type.
                    continue;
                }
                
                // Add 1 to the index because the first element in the dropdown is the scriptableObject's type.
                selectedTypeIndex = i + 1;
                break;
            }
            
            var yPos = rect.y + EditorGUIUtility.singleLineHeight + 12;
            
            EditorGUI.LabelField(
                new Rect(rect.x, yPos, rect.width / 2, EditorGUIUtility.singleLineHeight), "Type");
            
            EditorGUI.BeginChangeCheck();
            selectedTypeIndex = EditorGUI.Popup(
                new Rect(rect.x + rect.width / 2, yPos, rect.width / 2, EditorGUIUtility.singleLineHeight),
                selectedTypeIndex, typeNames);
            
            if (EditorGUI.EndChangeCheck()) 
            {
                typeProp.stringValue = selectedTypeIndex == 0 
                    ? type.AssemblyQualifiedName 
                    : interfaces[selectedTypeIndex - 1].AssemblyQualifiedName;
            }
        }
    }
}