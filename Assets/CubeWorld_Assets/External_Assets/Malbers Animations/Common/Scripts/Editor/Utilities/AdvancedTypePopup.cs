
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MalbersAnimations
{
    public class AdvancedTypePopupItem : AdvancedDropdownItem
    {
        public Type Type { get; }

        public AdvancedTypePopupItem(Type type, string name) : base(name) => Type = type;
    }

    public readonly struct TypePopupCache
    {
        const int k_MaxTypePopupLineCount = 10;

        static readonly Type k_UnityObjectType = typeof(UnityEngine.Object);

        public AdvancedTypePopup TypePopup { get; }
        public AdvancedDropdownState State { get; }
        public TypePopupCache(AdvancedTypePopup typePopup, AdvancedDropdownState state)
        {
            TypePopup = typePopup;
            State = state;
        }

        public static TypePopupCache GetTypePopup(SerializedProperty property,
            Dictionary<string, TypePopupCache> m_TypePopups, SerializedProperty m_TargetProperty)
        {
            // Cache this string. This property internally call Assembly.GetName, which result in a large allocation.
            string managedReferenceFieldTypename = property.managedReferenceFieldTypename;

            if (!m_TypePopups.TryGetValue(managedReferenceFieldTypename, out TypePopupCache result))
            {
                var state = new AdvancedDropdownState();

                Type baseType = MSerializedTools.GetType(managedReferenceFieldTypename);
                var popup = new AdvancedTypePopup(
                    TypeCache.GetTypesDerivedFrom(baseType).Append(baseType).Where(p =>
                        (p.IsPublic || p.IsNestedPublic) &&
                        !p.IsAbstract &&
                        !p.IsGenericType &&
                        !k_UnityObjectType.IsAssignableFrom(p) &&
                        Attribute.IsDefined(p, typeof(SerializableAttribute))
                    ),
                    k_MaxTypePopupLineCount, state);

                popup.OnItemSelected += item =>
                {
                    Type type = item.Type;
                    object obj = m_TargetProperty.SetManagedReference(type);
                    m_TargetProperty.isExpanded = (obj != null);
                    m_TargetProperty.serializedObject.ApplyModifiedProperties();
                    m_TargetProperty.serializedObject.Update();
                };

                result = new TypePopupCache(popup, state);
                m_TypePopups.Add(managedReferenceFieldTypename, result);
            }
            return result;
        }

        public static GUIContent GetTypeName(SerializedProperty property, Dictionary<string, GUIContent> m_TypeNameCaches)
        {
            // Cache this string.
            string managedReferenceFullTypename = property.managedReferenceFullTypename;

            if (string.IsNullOrEmpty(managedReferenceFullTypename))
            {
                return new GUIContent("[Null]");
            }
            if (m_TypeNameCaches.TryGetValue(managedReferenceFullTypename, out GUIContent cachedTypeName))
            {
                return cachedTypeName;
            }

            Type type = MSerializedTools.GetType(managedReferenceFullTypename);
            string typeName = null;

            AddTypeMenuAttribute typeMenu = TypeMenuUtility.GetAttribute(type);
            if (typeMenu != null)
            {
                typeName = typeMenu.GetTypeNameWithoutPath();
                if (!string.IsNullOrWhiteSpace(typeName))
                {
                    typeName = ObjectNames.NicifyVariableName(typeName);
                }
            }

            if (string.IsNullOrWhiteSpace(typeName))
            {
                typeName = ObjectNames.NicifyVariableName(type.Name);
            }

            GUIContent result = new(typeName);
            m_TypeNameCaches.Add(managedReferenceFullTypename, result);
            return result;
        }

    }

    /// <summary> A type popup with a fuzzy finder. </summary>
    public class AdvancedTypePopup : AdvancedDropdown
    {
        const int kMaxNamespaceNestCount = 16;

        public static void AddTo(AdvancedDropdownItem root, IEnumerable<Type> types)
        {
            int itemCount = 0;

            // Add null item.
            var nullItem = new AdvancedTypePopupItem(null, TypeMenuUtility.k_NullDisplayName)
            {
                id = itemCount++
            };
            root.AddChild(nullItem);

            Type[] typeArray = types.OrderByType().ToArray();

            // Single namespace if the root has one namespace and the nest is unbranched.
            bool isSingleNamespace = true;
            string[] namespaces = new string[kMaxNamespaceNestCount];
            foreach (Type type in typeArray)
            {
                string[] splittedTypePath = TypeMenuUtility.GetSplittedTypePath(type);
                if (splittedTypePath.Length <= 1)
                {
                    continue;
                }
                // If they explicitly want sub category, let them do.
                if (TypeMenuUtility.GetAttribute(type) != null)
                {
                    isSingleNamespace = false;
                    break;
                }
                for (int k = 0; (splittedTypePath.Length - 1) > k; k++)
                {
                    string ns = namespaces[k];
                    if (ns == null)
                    {
                        namespaces[k] = splittedTypePath[k];
                    }
                    else if (ns != splittedTypePath[k])
                    {
                        isSingleNamespace = false;
                        break;
                    }
                }

                if (!isSingleNamespace) break;
            }

            // Add type items.
            foreach (Type type in typeArray)
            {
                string[] splittedTypePath = TypeMenuUtility.GetSplittedTypePath(type);
                if (splittedTypePath.Length == 0)
                {
                    continue;
                }

                AdvancedDropdownItem parent = root;

                // Add namespace items.
                if (!isSingleNamespace)
                {
                    for (int k = 0; (splittedTypePath.Length - 1) > k; k++)
                    {
                        AdvancedDropdownItem foundItem = GetItem(parent, splittedTypePath[k]);
                        if (foundItem != null)
                        {
                            parent = foundItem;
                        }
                        else
                        {
                            var newItem = new AdvancedDropdownItem(splittedTypePath[k])
                            {
                                id = itemCount++,
                            };
                            parent.AddChild(newItem);
                            parent = newItem;
                        }
                    }
                }

                // Add type item.
                var item = new AdvancedTypePopupItem(type, ObjectNames.NicifyVariableName(splittedTypePath[splittedTypePath.Length - 1]))
                {
                    id = itemCount++
                };
                parent.AddChild(item);
            }
        }

        static AdvancedDropdownItem GetItem(AdvancedDropdownItem parent, string name)
        {
            foreach (AdvancedDropdownItem item in parent.children)
            {
                if (item.name == name) return item;
            }
            return null;
        }

        static readonly float k_HeaderHeight = EditorGUIUtility.singleLineHeight * 2f;

        Type[] m_Types;

        public event Action<AdvancedTypePopupItem> OnItemSelected;

        public AdvancedTypePopup(IEnumerable<Type> types, int maxLineCount, AdvancedDropdownState state) : base(state)
        {
            SetTypes(types);
            minimumSize = new Vector2(minimumSize.x, EditorGUIUtility.singleLineHeight * maxLineCount + k_HeaderHeight);
        }

        public void SetTypes(IEnumerable<Type> types) => m_Types = types.ToArray();

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Select Type");
            AddTo(root, m_Types);
            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            base.ItemSelected(item);
            if (item is AdvancedTypePopupItem typePopupItem)
            {
                OnItemSelected?.Invoke(typePopupItem);
            }
        }
    }
}
#endif
