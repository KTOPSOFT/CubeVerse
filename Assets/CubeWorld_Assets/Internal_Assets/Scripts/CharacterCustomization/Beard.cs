using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "Beard", menuName = "Scriptable Objects/CubeVerse/Beard")]
public class Beard : ScriptableObject
{
    [Serializable]
    public class BeardData
    {
        public string BeardName = "";
        public GameObject BeardObj;
    }
    public List<BeardData> BeardDataList = new List<BeardData>();
}
