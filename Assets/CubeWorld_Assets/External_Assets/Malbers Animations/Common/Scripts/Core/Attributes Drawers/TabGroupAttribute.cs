
#if UNITY_EDITOR
#endif

using UnityEngine;

namespace MalbersAnimations
{
    public class TabGroupAttribute : PropertyAttribute
    {
        public string TabName;

        public TabGroupAttribute(string tabName) => TabName = tabName;
    }

    //#if UNITY_EDITOR
    //    [CustomEditor(typeof(UnityEngine.MonoBehaviour), true)]
    //    public class TabbedInspectorEditor : Editor
    //    {
    //        private Dictionary<string, List<SerializedProperty>> tabs;
    //        private List<string> tabNames;
    //        private int currentTab;

    //        private List<SerializedProperty> defaultProperties;
    //        private HashSet<string> processedProperties; // Tracks processed property paths

    //        private void OnEnable()
    //        {
    //            tabs = new Dictionary<string, List<SerializedProperty>>();
    //            tabNames = new List<string>();
    //            defaultProperties = new List<SerializedProperty>();
    //            processedProperties = new HashSet<string>();
    //            currentTab = 0;

    //            SerializedProperty iterator = serializedObject.GetIterator();
    //            if (!iterator.NextVisible(true)) // Safely check if there are visible properties
    //                return;

    //            do
    //            {
    //                // Skip Unity's internal fields and unexpected properties
    //                if (iterator.name == "m_Script" || iterator.name.StartsWith("m_EditorClassIdentifier"))
    //                    continue;

    //                // Avoid processing the same property multiple times
    //                if (processedProperties.Contains(iterator.propertyPath))
    //                    continue;

    //                // Get field info for the property
    //                var targetType = serializedObject.targetObject.GetType();
    //                var field = targetType.GetField(iterator.name,
    //                    System.Reflection.BindingFlags.Public |
    //                    System.Reflection.BindingFlags.NonPublic |
    //                    System.Reflection.BindingFlags.Instance);

    //                // Skip fields with the HideInInspector attribute or those without reflection field info
    //                if (field == null || System.Attribute.IsDefined(field, typeof(HideInInspector)))
    //                    continue;

    //                // Check for TabGroupAttribute
    //                var tabAttribute = (TabGroupAttribute)System.Attribute.GetCustomAttribute(field, typeof(TabGroupAttribute));
    //                if (tabAttribute != null)
    //                {
    //                    string tabName = tabAttribute.TabName;

    //                    if (!tabs.ContainsKey(tabName))
    //                    {
    //                        tabs[tabName] = new List<SerializedProperty>();
    //                        tabNames.Add(tabName);
    //                    }

    //                    // Add the property to the tab
    //                    tabs[tabName].Add(iterator.Copy());
    //                    processedProperties.Add(iterator.propertyPath); // Mark as processed
    //                }
    //                else
    //                {
    //                    // Add to default properties if no TabGroupAttribute is present
    //                    defaultProperties.Add(iterator.Copy());
    //                    processedProperties.Add(iterator.propertyPath); // Mark as processed
    //                }

    //            } while (MoveToNextVisible(iterator)); // Safely iterate over properties
    //        }

    //        private bool MoveToNextVisible(SerializedProperty iterator)
    //        {
    //            if (!iterator.Next(false)) return false; // No more properties
    //            if (iterator.propertyType == SerializedPropertyType.ArraySize) return MoveToNextVisible(iterator);
    //            return true;
    //        }

    //        public override void OnInspectorGUI()
    //        {
    //            serializedObject.Update();

    //            // Draw default properties not assigned to any tab
    //            foreach (var property in defaultProperties)
    //            {
    //                EditorGUILayout.PropertyField(property, true);
    //            }

    //            // Draw tab toolbar if there are tabs
    //            if (tabNames.Count > 0)
    //            {
    //                GUILayout.Space(10); // Add some space between default fields and tabs
    //                currentTab = GUILayout.Toolbar(currentTab, tabNames.ToArray());

    //                // Draw fields in the selected tab
    //                if (tabs.ContainsKey(tabNames[currentTab]))
    //                {
    //                    foreach (var property in tabs[tabNames[currentTab]])
    //                    {
    //                        EditorGUILayout.PropertyField(property, true);
    //                    }
    //                }
    //            }
    //            serializedObject.ApplyModifiedProperties();
    //        }
    //    }
    //#endif
}