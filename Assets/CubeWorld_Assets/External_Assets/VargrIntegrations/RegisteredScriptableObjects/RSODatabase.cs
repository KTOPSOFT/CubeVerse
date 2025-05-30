using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VargrIntegrations
{
    /*
    [Serializable]
    public struct RSOTypeRange
    {
        public string Type;
        public int Min;
        public int Max;

        public RSOTypeRange(string type, int min, int max)
        {
            Type = type;
            Min = min;
            Max = max;
        }
    }
    */
    [Serializable]
    public struct RSOSerialized
    {
        public int ID;
        public ScriptableObject Object;
        public RSOSerialized(int id, ScriptableObject scriptableObject)
        {
            ID = id;
            Object = scriptableObject;
        }
    }

    [CreateAssetMenu(fileName = "RSO_Database", menuName = "NetSyncedObjects/Database", order = 1)]
    public class RSODatabase : ScriptableObject, ISerializationCallbackReceiver
    {
        public const string DatabaseName = "RSO_Database";
        /*
        [SerializeField] 
        private RSOTypeRange[] m_RSORanges;
        */
        public Dictionary<int, ScriptableObject> Data = new Dictionary<int, ScriptableObject>();
        [SerializeField]
        private List<RSOSerialized> m_dataSerialized = new List<RSOSerialized>();
        private int m_uKeyiterator; // Min: 1 Max: 2,147,483,647 (Negitive Numbers should be reserverd for Mod Loading)
        public int GenerateUniqueKey()
        {
            if(m_uKeyiterator == 0) m_uKeyiterator = 1;
            int result = 0;
            bool done = false;
            while (!done)
            {
                result = m_uKeyiterator++;
                if (!Data.ContainsKey(result)) done = true;
            }
            return result;
        }

        public int GetCount() => Data.Count;

        /// <summary>
        /// Callback which will get the move data from the Dictionary (which cannot serialize) to the key/value Lists (which can be serialized)
        /// </summary>
        public void OnBeforeSerialize()
        {
            m_dataSerialized.Clear();
            foreach (KeyValuePair<int, ScriptableObject> d in Data) m_dataSerialized.Add(new RSOSerialized(d.Key, d.Value));
        }

        /// <summary>
        /// Callback which will move the data from the key/value Lists (which have been serialized) and create the Dictionaries (which can't serialize).
        /// </summary>
        public void OnAfterDeserialize()
        {
            Data = new Dictionary<int, ScriptableObject>();
            foreach (RSOSerialized item in m_dataSerialized) Data.Add(item.ID, item.Object);
        }
        /// <summary>
        /// Get a RSORoot from the Database.
        /// </summary>
        /// <param name="key">The unique key for the RSORoot.</param>
        /// <returns>RSORoot Ref / RSOLink Value, or Null if the key doesn't exist in the Database. (NOTE: make sure you type cast to what you are expecting on the output)</returns>
        public ScriptableObject Get(int key)
        {
            if(key == 0) return null;
            ScriptableObject output = Data.ContainsKey(key) ? Data[key] : null;
            return output;
        }

        public Dictionary<int, T> GetDictType<T>()
        {
            Dictionary<int, T> values = new Dictionary<int, T>();
            foreach (var item in Data)
            {
                if(item.Value is T correctValue) values.Add(item.Key, correctValue);
            }
            return values;
        }

        /// <summary>
        /// Get Key from the Database.
        /// </summary>
        /// <param name="key">The unique key for the RSORoot.</param>
        /// <returns>The RSORoot reference, or Null if the key doesn't exist in the Database.</returns>
        public int GetKey(ScriptableObject target)
        {
            if(target == null) return 0;

            if(target is RSORoot) return (target as RSORoot).Key;

            if(Data.ContainsValue(target)) return Data.FirstOrDefault(x => x.Value == target).Key;
/*            
            // Check RSOLink
            var output = m_dataSerialized.FindAll(x => x.Object is RSOLink);
            int index = output.FindIndex( link => link.Object == target);
            if(index != -1) return output[index].ID;
*/
            return 0;
        }

        /// <summary>
        /// Add an item to the Database. Using this at runtime is not really useful since it doesn't persist between sessions, but maybe so if you are hotloading in custom content at runtime every session.
        /// </summary>
        /// <param name="data">The data reference.</param>
        /// <param name="generateKey">If true, a new ID will be generated for the item.</param>
        public virtual void Add(ScriptableObject data, bool generateKey = true)
        {
            if(data == null){
                Debug.LogWarning("Can not add NULL data to Libary.");
                return;
            }

            if(data is RSORoot RsoData){
                int id = RsoData.GetKey();

                if(id == 0 || generateKey) id = GenerateUniqueKey();

                if (Data.ContainsKey(id))
                {
                    if(Get(id) == RsoData) return;
                    if(generateKey) return;
                    Add(RsoData);
                    return;
                }
                
                RsoData.SetKey(id);
                Data.Add(id, RsoData);
            }else{
                if(!Data.ContainsValue(data)){
                    Data.Add(GenerateUniqueKey(), data);
                }
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(data);
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        /// <summary>
        /// Remove an item from the Database. Using this at runtime is not really useful since it doesn't persist between sessions, but maybe so if you are hotloading in custom content at runtime every session.
        /// </summary>
        /// <param name="data">The data reference.</param>
        public virtual void Remove(ScriptableObject data)
        {
            int key = GetKey(data);
            if (Data.ContainsKey(key)) Data.Remove(key);
        }

        /// <summary>
        /// Remove an item from the Database. This is a fast O(1) operation. Using this at runtime is not really useful since it doesn't persist between sessions, but maybe so if you are hotloading in custom content at runtime every session.
        /// </summary>
        /// <param name="id">The unique key ID for the item.</param>
        public virtual void Remove(int id)
        {
            if (Data.ContainsKey(id)) Data.Remove(id);
        }
    }
/*
#if UNITY_EDITOR
    [CustomEditor(typeof(RSODatabase))]
    public class RSODatabaseEditor : Editor
    {
        protected SerializedProperty value;

        void OnEnable() {
            value = serializedObject.FindProperty("m_key");
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
#endif
*/
}