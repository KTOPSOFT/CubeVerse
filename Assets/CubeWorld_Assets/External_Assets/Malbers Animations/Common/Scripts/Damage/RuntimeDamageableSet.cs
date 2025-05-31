using UnityEngine;
using System;
using UnityEngine.Events;
 

namespace MalbersAnimations.Scriptables
{
    [Serializable] public class DamageableEvent : UnityEvent<MDamageable> { }

    [CreateAssetMenu(menuName = "Malbers Animations/Collections/Runtime Damageable Set", order = 1000, fileName = "New Runtime Damageable Set")]
    public class RuntimeDamageableSet : RuntimeCollection<MDamageable>
    {
        public DamageableEvent OnItemAdded = new DamageableEvent();
        public DamageableEvent OnItemRemoved = new DamageableEvent();

        

        /// <summary>Return the Closest game object from an origin</summary>
        public MDamageable Item_GetClosest(MDamageable origin)
        {
            items.RemoveAll(x => x == null); //Remove all Assets that are Empty/ Type Mismatch error

            MDamageable closest = null;

            float minDistance = float.MaxValue;

            foreach (var item in items)
            {
                var Distance = Vector3.Distance(item.transform.position, origin.transform.position);

                if (Distance < minDistance)
                {
                    closest = item;
                    minDistance = Distance;
                }
            }
            return closest;
        }


        public void ItemAdd(Component newItem)
        {
            var s = newItem.FindComponent<MDamageable>();
            if (s) Item_Add(s);
        }

        public void Item_Add(GameObject newItem)
        {
            var s = newItem.FindComponent<MDamageable>();
            if (s)  Item_Add(s);
        }
 

        protected override void OnAddEvent(MDamageable newItem) => OnItemAdded.Invoke(newItem);
        protected override void OnRemoveEvent(MDamageable newItem) => OnItemRemoved.Invoke(newItem);
         

        public void ItemRemove(Component newItem)
        {
            var s = newItem.FindComponent<MDamageable>();
            if (s) Item_Remove((MDamageable)s);
        }

        public void Item_Remove(GameObject newItem)
        {
            if (newItem)
            {
                var s = newItem.FindComponent<MDamageable>();
                if (s) Item_Remove(s);
            }
        }
    }



#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(RuntimeDamageableSet))]
    public class RuntimeDamageableSetEditor : RuntimeCollectionEditor<MDamageable> { }
#endif


    //#if UNITY_EDITOR
    //    [CustomEditor(typeof(RuntimeStats))]
    //    public class RuntimeStatsEditor : Editor
    //    {
    //        public override void OnInspectorGUI()
    //        {
    //            serializedObject.Update();
    //            var M = (RuntimeStats)target;

    //            if (Application.isPlaying)
    //            {
    //                MalbersEditor.DrawHeader(M.name + " - List");

    //                EditorGUI.BeginDisabledGroup(true);
    //                for (int i = 0; i < M.Items.Count; i++)
    //                {
    //                    EditorGUILayout.ObjectField("Item " + i, M.Items[i], typeof(Stats), false);
    //                }
    //                EditorGUI.EndDisabledGroup();
    //            }


    //            EditorGUILayout.PropertyField(serializedObject.FindProperty("Description"));
    //            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnSetEmpty"));
    //            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnItemAdded"));
    //            EditorGUILayout.PropertyField(serializedObject.FindProperty("OnItemRemoved"));

    //            if (!Application.isPlaying && M.Items != null &&  M.Items.Count > 0 && GUILayout.Button("Clear Set - " + M.Items.Count))
    //            {
    //                M.Clear();
    //                MTools.SetDirty(target);
    //                serializedObject.ApplyModifiedProperties();
    //            }

    //            serializedObject.ApplyModifiedProperties();
    //        }
    //    }
    //#endif
}

