using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "Face", menuName = "Scriptable Objects/CubeVerse/Face")]
public class Face : ScriptableObject
{
    public enum SexCategory
    {
        Male,
        Female
    }    
    public enum SkinCategory
    {
        White,
        Brown,
        Black
    }

    [Serializable]
    public class FaceData
    {
        public string FaceName;
        public Texture FaceMap;
    }

    [Serializable]
    public class SkinFaceData
    {
        public SkinCategory Skin;
        public string Path;
        public List<FaceData> Details = new List<FaceData>();
    }

    [Serializable]
    public class SexFaceData
    {
        public SexCategory Sex;
        public List<SkinFaceData> Details = new List<SkinFaceData>();
    }

    

    public List<SexFaceData> FaceDataList = new List<SexFaceData>();
}
