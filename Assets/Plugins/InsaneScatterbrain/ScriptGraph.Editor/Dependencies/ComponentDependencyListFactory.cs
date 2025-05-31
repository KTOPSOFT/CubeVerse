using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    public class ComponentDependencyListFactory
    {
        private const string GameObjectPropName = "gameObject";
        private const string ComponentPropName = "component";
        private const string SerializedTypePropName = "serializedType";
            
        private readonly SerializedProperty componentDependenciesProp;

        public ComponentDependencyListFactory(SerializedProperty componentDependenciesProp)
        {
            this.componentDependenciesProp = componentDependenciesProp;
        }
        
        public ReorderableList Create()
        {
            return new ReorderableList(
                componentDependenciesProp.serializedObject, 
                componentDependenciesProp, 
                false, true, true, true)
            {
                drawElementCallback = DrawElementCallback,
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, "Dependencies");
                },
                elementHeight = EditorGUIUtility.singleLineHeight * 3 + 18,
            };
        }
        
        private void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var gameObject = AddObjectFields(rect, index);
            
            if (gameObject == null)
            {
                // No gameobject is selected, so we can't show any of its components.
                return;
            }
            
            var component = AddComponentFields(rect, index, gameObject);
            
            if (component == null)
            {
                // No component is selected, so we can't show its type and interfaces.
                return;
            }

            AddTypeFields(rect, index, component);
        }

        private GameObject AddObjectFields(Rect rect, int index)
        {
            var dependencyProp = componentDependenciesProp.GetArrayElementAtIndex(index);
            var gameObjectProp = dependencyProp.FindPropertyRelative(GameObjectPropName);
            var componentProp = dependencyProp.FindPropertyRelative(ComponentPropName);
            var typeProp = dependencyProp.FindPropertyRelative(SerializedTypePropName);
            
            var yPos = rect.y + 6;
            
            EditorGUI.LabelField(
                new Rect(rect.x, yPos, rect.width / 2, EditorGUIUtility.singleLineHeight), "GameObject");
            
            EditorGUI.BeginChangeCheck();
            gameObjectProp.objectReferenceValue = (GameObject) EditorGUI.ObjectField(
                new Rect(rect.x + rect.width / 2, yPos, rect.width / 2, EditorGUIUtility.singleLineHeight),
                gameObjectProp.objectReferenceValue, typeof(GameObject), true);

            if (EditorGUI.EndChangeCheck())
            {
                // If the gameobject is changed, then the component and type should be reset.
                componentProp.objectReferenceValue = null;
                typeProp.stringValue = null;
                
                // If the newly selected gameobject has components, then select the first one.
                var gameObject = (GameObject) gameObjectProp.objectReferenceValue;
                if (gameObject != null)
                {
                    var components = gameObject.GetComponents<MonoBehaviour>();
                    if (components.Length > 0)
                    {
                        componentProp.objectReferenceValue = components[0];
                        typeProp.stringValue = components[0].GetType().AssemblyQualifiedName;
                    }
                }
            }

            return (GameObject)gameObjectProp.objectReferenceValue;
        }

        private MonoBehaviour AddComponentFields(Rect rect, int index, GameObject gameObject)
        {
            var dependencyProp = componentDependenciesProp.GetArrayElementAtIndex(index);
            var componentProp = dependencyProp.FindPropertyRelative(ComponentPropName);
            var typeProp = dependencyProp.FindPropertyRelative(SerializedTypePropName);
            
            // Get all components on the selected gameobject that aren't null. This can happen if a script
            // no longer exists.
            var monoBehaviours = gameObject.GetComponents<MonoBehaviour>()
                .Where(c => c != null).ToArray();
            
            var yPos = rect.y + EditorGUIUtility.singleLineHeight + 9;
            
            if (monoBehaviours.Length == 0)
            {
                return null;
            }
            
            // Get all the monobehaviour type names, for display in the dropdown.
            var componentTypeNames = monoBehaviours.Select(m => m.GetType().Name).ToArray();
            
            // Get the currently selected component.
            var selectedComponent = componentProp.objectReferenceValue as MonoBehaviour;
            
            var selectedIndex = 0;  // Select the first component by default.
            if (selectedComponent != null)
            {
                selectedIndex = Array.IndexOf(monoBehaviours, selectedComponent);
            }
            
            EditorGUI.LabelField(
                new Rect(rect.x, yPos, rect.width / 2, EditorGUIUtility.singleLineHeight), "Component");
                    
            EditorGUI.BeginChangeCheck();
            selectedIndex = EditorGUI.Popup(
                new Rect(rect.x + rect.width / 2, yPos, rect.width / 2, EditorGUIUtility.singleLineHeight),
                selectedIndex, componentTypeNames);

            if (!EditorGUI.EndChangeCheck())
            {
                return selectedComponent;
            }
            
            // If the selected component is changed, then the type should be set to the component's type.
            typeProp.stringValue = monoBehaviours[selectedIndex].GetType().AssemblyQualifiedName;
            
            selectedComponent = monoBehaviours[selectedIndex];
            componentProp.objectReferenceValue = selectedComponent;

            return selectedComponent;
        }

        private void AddTypeFields(Rect rect, int index, MonoBehaviour component)
        {
            var dependencyProp = componentDependenciesProp.GetArrayElementAtIndex(index);
            var typeProp = dependencyProp.FindPropertyRelative(SerializedTypePropName);
            
            // Create a list of types that the dependency can be registered under. The options consist of the
            // component's own type and all of its interfaces.
            var type = component.GetType();
            var interfaces = type.GetInterfaces().OrderBy(i => i.Name).ToArray();
            
            var typeNames = new string[interfaces.Length + 1];
            
            typeNames[0] = type.Name;
            for (var i = 0; i < interfaces.Length; i++)
            {
                typeNames[i + 1] = interfaces[i].Name;
            }
            
            var selectedTypeIndex = 0;  // Select the component's type by default.
            var selectedTypeName = typeProp.stringValue;
            for (var i = 0; i < interfaces.Length; i++)
            {
                if (selectedTypeName != interfaces[i].AssemblyQualifiedName)
                {
                    // If the selected type isn't an interface, then it must be the component's type.
                    continue;
                }
                
                // Add 1 to the index because the first element in the dropdown is the component's type.
                selectedTypeIndex = i + 1;
                break;
            }
            
            var yPos = rect.y + EditorGUIUtility.singleLineHeight * 2 + 12;
            
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