using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO;

[CreateAssetMenu(fileName = "HatProp", menuName = "Scriptable Objects/CubeVerse/HatProp")]
public class HatProp : ScriptableObject
{
    public enum HatCategory
    {
        Crown,
        Ears,
        Hats,
        Headband,
        Helmet,
        Horn,
        Mask,
        Misc,
        None
    }

    [Serializable]
    public class HatData
    {
        public string HatName = "";
        public GameObject HatProp;
    }

    [Serializable]
    public class HatDataList
    {
        public HatCategory Category;
        public List<string> Paths = new List<string>();
        public List<HatData> Data = new List<HatData>();
    }


    public List<HatDataList> hatDataList = new List<HatDataList>();
}

