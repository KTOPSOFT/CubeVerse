using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FishNet.Connection;
using FishNet.Object;
using MalbersAnimations.VargrMultiplayer;
using SoftKitty.PCW;
using UnityEngine;
using static BackPack;
using static Body;
using static BodyProp;
using static Hair;
using static HatProp;

public class CCSync : NetworkBehaviour
{
    public NetworkConnection owner => Owner;
    public bool isServer => IsServerStarted;
    public bool isServerOnly => IsServerOnlyStarted;
    public bool isClient => IsClientOnlyStarted;
    public bool isHost => IsHostStarted;
    public bool isController => base.Owner.IsLocalClient || (base.IsServerInitialized && !base.Owner.IsValid);
    public bool isOwner => IsOwner;
    public bool hasOwner => Owner.IsValid;

    private CustomCharacterInfo CharacterInfo = new CustomCharacterInfo();

    public BackPack BackPack;
    public Beard Beard;
    public Body Body;
    public Face Face;
    public Hair Hair;
    public HatProp HatProp;
    public BodyProp BodyProp;

    public SkinnedMeshRenderer CubbyBody;
    public GameObject CubbyBackPack;
    public GameObject CubbyBeard;
    public GameObject CubbyHair;
    public GameObject CubbyHat;
    public MeshFilter CubbyFace;
    public GameObject CubbyBodyProp;

    public override void OnStartClient()
    {
        base.OnStartClient();
        gameObject.GetComponent<PlayerInstance>().setName();
        InitCharacter();
        if (IsOwner)
        {
            BlockGenerator.instance.Player = gameObject;
        }
    }

    public void InitCharacter()
    {
        string fullString = gameObject.name;
        string updated_ID = fullString.Replace("-LOCAL", "");
        Debug.Log(updated_ID);
        RequestCharacterData(NetworkManager.ClientManager.Connection, updated_ID);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestCharacterData(NetworkConnection conn, string object_ID)
    {
        CustomCharacterInfo customCharacterInfo =
            GameObject.Find("ServerManagementObject(Clone)").GetComponent<ServerManagementObject>().GetCharacterData(object_ID);
        ResponseCharacterData(conn, customCharacterInfo);
    }

    [TargetRpc]
    public void ResponseCharacterData(NetworkConnection conn, CustomCharacterInfo customCharacterInfo)
    {
        UpdateCharacter(customCharacterInfo);
    }

    public void UpdateCharacter(CustomCharacterInfo characterInfo)
    {
        Debug.Log("Updating Character");

        CharacterInfo.skin_category = characterInfo.skin_category;
        CharacterInfo.sex_category = characterInfo.sex_category;

        CharacterInfo.body_index = characterInfo.body_index;

        CharacterInfo.backpack_category = characterInfo.backpack_category;
        CharacterInfo.backpack_index = characterInfo.backpack_index;

        CharacterInfo.hair_index = characterInfo.hair_index;
        CharacterInfo.beard_index = characterInfo.beard_index;

        CharacterInfo.hat_category = characterInfo.hat_category;
        CharacterInfo.hat_index = characterInfo.hat_index;

        CharacterInfo.face_index = characterInfo.face_index;
        CharacterInfo.bodyprop_index = characterInfo.bodyprop_index;

        SetupCustomizedCharacter();
    }

    public void SetupCustomizedCharacter()
    {
        ApplyBody((Body.SexCategory)CharacterInfo.sex_category, (Body.SkinCategory)CharacterInfo.skin_category , CharacterInfo.body_index);
        ApplyBackpack(CharacterInfo.backpack_category, CharacterInfo.backpack_index);
        ApplyHair(CharacterInfo.hair_index);
        ApplyBeard(CharacterInfo.beard_index);
        ApplyHat(CharacterInfo.hat_category , CharacterInfo.hat_index);
        ApplyFace((Face.SexCategory)CharacterInfo.sex_category, (Face.SkinCategory)CharacterInfo.skin_category , CharacterInfo.face_index);
        //ApplyBodyProp(CharacterInfo.bodyprop_index);
    }

    public List<BodyData> GetBodyData(Body.SexCategory sex, Body.SkinCategory skin)
    {
        if (Body == null) return new List<BodyData>(); // Safety check

        foreach (var sexData in Body.BodyList)
        {
            if (sexData.Sex == sex)
            {
                foreach (var skinData in sexData.Skins)
                {
                    if (skinData.Skin == skin)
                    {
                        return skinData.BodyData;
                    }
                }
            }
        }
        return new List<BodyData>(); // Return empty list if nothing found
    }

    public struct BackPackInfo
    {
        public BackPack.BackPackCategory Category;
        public int Index; // Index inside the Data list
        public BackPack.BackPackData Data;
    }

    public List<BackPackInfo> GetBackPackData()
    {
        List<BackPackInfo> allBackPacks = new List<BackPackInfo>();

        foreach (var backpackList in BackPack.backPackLists)
        {
            for (int i = 0; i < backpackList.Data.Count; i++)
            {
                BackPackInfo info = new BackPackInfo
                {
                    Category = backpackList.Category,
                    Index = i,
                    Data = backpackList.Data[i]
                };
                allBackPacks.Add(info);
            }
        }

        return allBackPacks;
    }

    public List<Beard.BeardData> GetBeardData()
    {
        if (Beard == null) return new List<Beard.BeardData>(); // Safety check

        return Beard.BeardDataList;
    }

    public List<HairData> GetHairData(Hair.SexCategory sex)
    {
        if (Hair == null) return new List<HairData>(); // Safety check

        foreach (var sexData in Hair.HairDataList)
        {
            if (sexData.Sex == sex)
            {
                return sexData.HairData;
            }
        }

        return new List<HairData>(); // Return empty list if nothing found
    }

    public struct HatInfo
    {
        public HatCategory Category;
        public int Index; // Index inside the Data list
        public HatData Data;
    }

    public List<HatInfo> GetAllHatData()
    {
        List<HatInfo> allHatData = new List<HatInfo>();

        foreach (var hatDataList in HatProp.hatDataList)
        {
            for (int i = 0; i < hatDataList.Data.Count; i++)
            {
                HatInfo hatInfo = new HatInfo
                {
                    Category = hatDataList.Category,
                    Index = i,
                    Data = hatDataList.Data[i]
                };
                allHatData.Add(hatInfo);
            }
        }

        return allHatData;
    }

    public List<Face.FaceData> GetFaceData(Face.SexCategory sex, Face.SkinCategory skin)
    {
        if (Face == null)
            return new List<Face.FaceData>(); // Safety check for null

        // Iterate through the sex data list
        foreach (var sexData in Face.FaceDataList)
        {
            // If we find a matching sex category
            if (sexData.Sex == sex)
            {
                // Iterate through the skin data for the matching sex
                foreach (var skinData in sexData.Details)
                {
                    // If we find a matching skin category, return the corresponding FaceData
                    if (skinData.Skin == skin)
                    {
                        return skinData.Details; // Return the list of FaceData associated with this sex and skin
                    }
                }
            }
        }

        // Return an empty list if no match is found
        return new List<Face.FaceData>();
    }

    public List<BodyPropData> GetBodyPropData()
    {
        if (BodyProp == null)
            return new List<BodyProp.BodyPropData>(); // Safety check for null

        // Simply return the entire list of BodyPropData
        return BodyProp.BodyPropList;
    }

    public void ApplyBody(Body.SexCategory sex, Body.SkinCategory skin, int index)
    {
        var bodyDataList = GetBodyData(sex, skin);

        if (bodyDataList != null && bodyDataList.Count > index)
        {
            var selectedBody = bodyDataList[index];

            if (selectedBody != null && selectedBody.BodyMap != null)
            {
                CubbyBody.materials[0].SetTexture("_BaseMap", selectedBody.BodyMap);
            }
        }
    }

    // Apply a specific Backpack
    public void ApplyBackpack(BackPackCategory category, int index)
    {
        if (CubbyBackPack.transform.childCount > 0)
        {
            foreach (Transform child in CubbyBackPack.transform)
                Destroy(child.gameObject);
        }

        var backpackData = BackPack.backPackLists.Find(x => x.Category == category);

        if (backpackData != null && backpackData.Data.Count > index)
        {
            var selectedBackPack = backpackData.Data[index];

            if (selectedBackPack != null && selectedBackPack.BackPackProp != null)
            {
                GameObject spawnedBackPack = Instantiate(selectedBackPack.BackPackProp, CubbyBackPack.transform, false);
                spawnedBackPack.transform.localPosition = Vector3.zero;
            }
        }
    }

    public void ApplyBeard(int index)
    {
        if ((Body.SexCategory)CharacterInfo.sex_category == Body.SexCategory.Female) return;
        if (CubbyBeard.transform.childCount > 0)
        {
            foreach (Transform child in CubbyBeard.transform)
                Destroy(child.gameObject);
        }

        var beardDataList = GetBeardData();

        if (beardDataList != null && beardDataList.Count > index)
        {
            var selectedBeard = beardDataList[index];

            if (selectedBeard != null)
            {
                GameObject spawnedBeard = Instantiate(selectedBeard.BeardObj, CubbyBeard.transform, false);
                spawnedBeard.transform.localPosition = Vector3.zero;
            }
        }
    }

    public void ApplyHair(int index)
    {
        if (CubbyHair.transform.childCount > 0)
        {
            foreach (Transform child in CubbyHair.transform)
                Destroy(child.gameObject);
        }

        var hairDataList = GetHairData((Hair.SexCategory)CharacterInfo.sex_category);

        if (hairDataList != null && hairDataList.Count > index)
        {
            var selectedHair = hairDataList[index];

            if (selectedHair != null)
            {
                GameObject spawnedHair = Instantiate(selectedHair.HairObj, CubbyHair.transform, false);
                spawnedHair.transform.localPosition = Vector3.zero;
            }
        }
    }

    public void ApplyHat(HatCategory category, int index)
    {
        if (CubbyHat.transform.childCount > 0)
        {
            foreach (Transform child in CubbyHat.transform)
                Destroy(child.gameObject);
        }

        var hatDataList = HatProp.hatDataList.FirstOrDefault(h => h.Category == category)?.Data;

        if (hatDataList != null && hatDataList.Count > index)
        {
            var selectedHat = hatDataList[index];

            if (selectedHat != null && selectedHat.HatProp != null)
            {
                GameObject spawnedHat = Instantiate(selectedHat.HatProp, CubbyHat.transform, false);
                spawnedHat.transform.localPosition = Vector3.zero;
            }
        }
    }

    public void ApplyFace(Face.SexCategory sex, Face.SkinCategory skin, int index)
    {
        if (CubbyFace.transform.childCount > 0)
        {
            foreach (Transform child in CubbyFace.transform)
                Destroy(child.gameObject);
        }

        var faceDataList = GetFaceData(sex, skin);

        if (faceDataList != null && faceDataList.Count > index)
        {
            var selectedFace = faceDataList[index];

            if (selectedFace != null && selectedFace.FaceMap != null)
            {
                CubbyFace.GetComponent<MeshRenderer>().materials[0].SetTexture("_BaseMap", selectedFace.FaceMap);
            }
        }
    }

    // Apply a specific Body Prop
    public void ApplyBodyProp(int index)
    {
        if (CubbyBodyProp.transform.childCount > 0)
        {
            foreach (Transform child in CubbyBodyProp.transform)
                Destroy(child.gameObject);
        }

        var bodyPropDataList = GetBodyPropData();

        if (bodyPropDataList != null && bodyPropDataList.Count > index)
        {
            var selectedBodyProp = bodyPropDataList[index];

            if (selectedBodyProp != null && selectedBodyProp.BodyPropObj != null)
            {
                GameObject spawnedBodyProp = Instantiate(selectedBodyProp.BodyPropObj, CubbyBodyProp.transform, false);
                spawnedBodyProp.transform.localPosition = Vector3.zero;
            }
        }
    }
}
