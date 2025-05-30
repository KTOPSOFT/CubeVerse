using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "BackPack", menuName = "Scriptable Objects/CubeVerse/BackPack")]
public class BackPack : ScriptableObject
{
    public enum BackPackCategory
    {
        BackProps,
        Bags,
        Baskets,
        Wings,
        None
    }

    [Serializable]
    public class BackPackData
    {
        public string BackPackName = "";
        public GameObject BackPackProp;
    }

    [Serializable]
    public class BackPackList
    {
        public BackPackCategory Category;
        public List<string> Paths = new List<string>();
        public List<BackPackData> Data = new List<BackPackData>();
    }


    public List<BackPackList> backPackLists = new List<BackPackList>();
}
