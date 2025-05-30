using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "Body", menuName = "Scriptable Objects/CubeVerse/Body")]
public class Body : ScriptableObject
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
        Black,
        Gorgon,
        Spider,
        Special
    }

    [Serializable]
    public class BodyData
    {
        public string BodyName = "";
        public Texture BodyMap;
    }

    [Serializable]
    public class SkinData
    {
        public SkinCategory Skin;
        public string Paths;
        public List<BodyData> BodyData = new List<BodyData>();
    }

    [Serializable]
    public class SexData
    {
        public SexCategory Sex;
        public List <SkinData> Skins = new List<SkinData>();
    }

    public List<SexData> BodyList = new List<SexData>();
}
