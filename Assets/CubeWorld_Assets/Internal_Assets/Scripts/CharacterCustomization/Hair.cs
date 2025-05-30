using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "Hair", menuName = "Scriptable Objects/CubeVerse/Hair")]
public class Hair : ScriptableObject
{
    public enum SexCategory
    {
        Male,
        Female
    }

    [Serializable]
    public class HairData
    {
        public string HairName = "";
        public GameObject HairObj;
    }

    [Serializable]
    public class SexHairData
    {
        public SexCategory Sex;
        public string Path;
        public List<HairData> HairData = new List<HairData>();
    }
    public List<SexHairData> HairDataList = new List<SexHairData>();
}
