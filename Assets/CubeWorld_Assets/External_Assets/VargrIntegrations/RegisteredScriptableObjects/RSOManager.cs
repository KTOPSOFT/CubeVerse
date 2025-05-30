using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VargrIntegrations
{
    public partial class RSOManager : MonoBehaviour
    {
        public static RSOManager Instance;
        
        public static ScriptableObject Get(int key)
        {
            return Database.Get(key);
        }

        public static int GetKey(ScriptableObject target)
        {
            if(target is RSORoot) return (target as RSORoot).Key;
            
            return Database.GetKey(target);
        }

        public static Dictionary<int, T> GetDictType<T>()
        {
            return Database.GetDictType<T>();
        }

        private static RSODatabase m_database;

        public static RSODatabase Database
        {
            get
            {
#if UNITY_EDITOR
                if (m_database == null) m_database = FindDatabase();
                return m_database;
#else
                return m_database;
#endif
            }
            protected set => m_database = value;
        }

        [SerializeField]
        private RSODatabase m_databaseRef;

        private void Awake()
        {
             //If This Manager was somehow loaded twice.
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            Database = m_databaseRef;
        }

        /*
        For Future Mod Support we will want to have the ability to load in external Data.
        Question is do we save this or not but we will need to not effect the Database 
        */
#if UNITY_EDITOR
        private static RSODatabase FindDatabase()
        {
            RSODatabase temp = (RSODatabase)AssetDatabase.LoadAssetAtPath("Assets/RegisteredSO_Database.asset", typeof(RSODatabase));
            if(temp == null){
                Debug.LogError("Database Not Found");
                RSODatabase newDB = new RSODatabase();
                AssetDatabase.CreateAsset(newDB, "Assets/RegisteredSO_Database.asset");
                return (RSODatabase)AssetDatabase.LoadAssetAtPath("Assets/RegisteredSO_Database.asset", typeof(RSODatabase));
            }
            return temp;
        }
        
        public void OnValidate()
        {
            if(m_databaseRef == null) m_databaseRef = FindDatabase();
            if(m_database == null || m_database != m_databaseRef) m_database = m_databaseRef;
        }
#endif
    }
}