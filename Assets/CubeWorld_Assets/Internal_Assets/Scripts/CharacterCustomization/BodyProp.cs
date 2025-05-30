using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "BodyProp", menuName = "Scriptable Objects/CubeVerse/BodyProp")]
public class BodyProp : ScriptableObject
{
    [Serializable]
    public class BodyPropData
    {
        public string BodyPropName;
        public GameObject BodyPropObj;
    }
    public List<BodyPropData> BodyPropList = new List<BodyPropData>();
}
