using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BackPack;
using static Body;
using static BodyProp;
using static Face;
using static Hair;
using static HatProp;

public class CustomizationManager : MonoBehaviour
{
    public static CustomizationManager instance;

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
    public enum detail_color { none, hair, beard }
    public enum sex { male, female }
    public enum skin { white, brown, black, gorgon, spider, special }

    public sex sex_category;
    public skin skin_category;
    public int body_index = 0;
    public BackPackCategory backpack_category;
    public int backpack_index;
    public int hair_index = 0;
    public int beard_index = 0;
    public HatCategory hat_category;
    public int hat_index;
    public int face_index = 0;
    public int bodyprop_index = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // Prevent duplicate
        }
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
                sex_category = (sex)sex;
                skin_category = (skin)skin;
                body_index = index;
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

                backpack_category = category;
                backpack_index = index;
            }
        }
    }

    public void ApplyBeard(int index)
    {
        if ((Body.SexCategory)sex_category == Body.SexCategory.Female) return;
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

                beard_index = index;
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

        var hairDataList = GetHairData((Hair.SexCategory)sex_category);

        if (hairDataList != null && hairDataList.Count > index)
        {
            var selectedHair = hairDataList[index];

            if (selectedHair != null)
            {
                GameObject spawnedHair = Instantiate(selectedHair.HairObj, CubbyHair.transform, false);
                spawnedHair.transform.localPosition = Vector3.zero;

                hair_index = index;
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

                hat_category = category;
                hat_index = index;
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

                sex_category = (sex)sex;
                skin_category = (skin)skin;
                face_index = index;
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

                bodyprop_index = index;
            }
        }
    }

    public void RemoveHair()
    {
        if (CubbyHair.transform.childCount > 0)
        {
            foreach (Transform child in CubbyHair.transform)
            {
                Destroy(child.gameObject);
            }
            hair_index = -1;  // Reset hair index
        }
    }

    public void RemoveHat()
    {
        if (CubbyHat.transform.childCount > 0)
        {
            foreach (Transform child in CubbyHat.transform)
            {
                Destroy(child.gameObject);
            }
            hat_category = HatCategory.None;  // Reset hat category
            hat_index = -1;  // Reset hat index
        }
    }

    public void RemoveBackpack()
    {
        if (CubbyBackPack.transform.childCount > 0)
        {
            foreach (Transform child in CubbyBackPack.transform)
            {
                Destroy(child.gameObject);
            }
            backpack_category = BackPackCategory.None;  // Reset backpack category
            backpack_index = -1;  // Reset backpack index
        }
    }

    public void RemoveBeard()
    {
        if (CubbyBeard.transform.childCount > 0)
        {
            foreach (Transform child in CubbyBeard.transform)
            {
                Destroy(child.gameObject);
            }
            beard_index = -1;  // Reset beard index
        }
    }

    public void RemoveBodyProp()
    {
        if (CubbyBodyProp.transform.childCount > 0)
        {
            foreach (Transform child in CubbyBodyProp.transform)
            {
                Destroy(child.gameObject);
            }
            bodyprop_index = -1;  // Reset body prop index
        }
    }

    public void ResetBody()
    {
        // Get the current sex and skin category
        Body.SexCategory selectedSex = (Body.SexCategory)(int)sex_category;
        Body.SkinCategory selectedSkin = (Body.SkinCategory)(int)skin_category;

        // Clear current body if any
        if (CubbyBody.transform.childCount > 0)
        {
            foreach (Transform child in CubbyBody.transform)
            {
                Destroy(child.gameObject);
            }
        }

        // Get the body data for the selected sex and skin
        var bodyDataList = GetBodyData(selectedSex, selectedSkin);

        if (bodyDataList != null && bodyDataList.Count > 0)
        {
            // Set the body to the first index
            var selectedBody = bodyDataList[0];
            if (selectedBody != null && selectedBody.BodyMap != null)
            {
                // Apply the first body texture
                CubbyBody.materials[0].SetTexture("_BaseMap", selectedBody.BodyMap);
                body_index = 0;  // Set the body index to 0 (first body)
            }
        }
    }

    // Reset function for Face
    public void ResetFace()
    {
        // Get the current sex and skin category
        Face.SexCategory selectedSex = (Face.SexCategory)(int)sex_category;
        Face.SkinCategory selectedSkin = (Face.SkinCategory)(int)skin_category;

        // Clear current face if any
        if (CubbyFace.transform.childCount > 0)
        {
            foreach (Transform child in CubbyFace.transform)
            {
                Destroy(child.gameObject);
            }
        }

        // Get the face data for the selected sex and skin
        var faceDataList = GetFaceData(selectedSex, selectedSkin);

        if (faceDataList != null && faceDataList.Count > 0)
        {
            // Set the face to the first index
            var selectedFace = faceDataList[0];
            if (selectedFace != null && selectedFace.FaceMap != null)
            {
                // Apply the first face texture
                CubbyFace.GetComponent<MeshRenderer>().materials[0].SetTexture("_BaseMap", selectedFace.FaceMap);
                face_index = 0;  // Set the face index to 0 (first face)
            }
        }
    }


}
