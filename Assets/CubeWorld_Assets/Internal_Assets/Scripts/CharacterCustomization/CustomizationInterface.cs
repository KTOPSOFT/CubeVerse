using System;
using System.Collections.Generic;
using FishNet.Managing.Scened;
using MalbersAnimations.Utilities;
using Michsky.DreamOS;
using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using static Body;
using static Prop_List;
using static CustomizationManager;
using static Beard;
using static BodyProp;
using static Face;
using static BackPack;
using static Hair;
using static HatProp;
using Random = UnityEngine.Random;

public class CustomizationInterface : MonoBehaviour
{
    public static CustomizationInterface Instance { get; private set; }
    public GameObject Prop_List;

    public GameObject BodyListParent;
    public GameObject BackPackListParent;
    public GameObject BeardListParent;
    public GameObject HairListParent;
    public GameObject HatListParent;
    public GameObject FaceListParent;
    public GameObject BodyPropListParent;

    public GameObject BeardList;
    public GameObject ColorPicker;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GetAllBodyList();
        GetAllBackPackList();
        GetAllBeardList();
        GetAllHairList();
        GetAllHatList();
        GetAllFaceList();
        //GetAllBodyPropList();
    }

    public void OnClickMale()
    {
        CustomizationManager.instance.sex_category = CustomizationManager.sex.male;
        GetAllBodyList();
        GetAllBeardList();
        GetAllHairList();
        GetAllFaceList();
    }

    public void OnClickFemale()
    {
        CustomizationManager.instance.sex_category = CustomizationManager.sex.female;
        GetAllBodyList();
        GetAllHairList();
        GetAllFaceList();
        CustomizationManager.instance.RemoveBeard();
    }

    public void SetUpWhiteSkin()
    {
        CustomizationManager.instance.skin_category = CustomizationManager.skin.white;
        GetAllBodyList();
        GetAllFaceList();
    }

    public void SetUpBrownSkin()
    {
        CustomizationManager.instance.skin_category = CustomizationManager.skin.brown;
        GetAllBodyList();
        GetAllFaceList();
    }

    public void SetUpBlackSkin()
    {
        CustomizationManager.instance.skin_category = CustomizationManager.skin.black;
        GetAllBodyList();
        GetAllFaceList();
    }

    public void GetAllBodyList()
    {
        // Clear existing buttons first
        foreach (Transform child in BodyListParent.transform)
        {
            Destroy(child.gameObject);
        }

        // Map your enums
        Body.SexCategory selectedSex = (Body.SexCategory)CustomizationManager.instance.sex_category;
        Body.SkinCategory selectedSkin = (Body.SkinCategory)CustomizationManager.instance.skin_category;

        List<BodyData> bodyDataList = CustomizationManager.instance.GetBodyData(selectedSex, selectedSkin);

        for (int i = 0; i < bodyDataList.Count; i++)
        {
            var bodyData = bodyDataList[i];

            GameObject temp_go = Instantiate(Prop_List, BodyListParent.transform);

            ButtonManager buttonManager = temp_go.GetComponent<ButtonManager>();
            if (buttonManager != null)
            {
                buttonManager.buttonText = bodyData.BodyName;
                buttonManager.UpdateUI();
            }

            Prop_List propList = temp_go.GetComponent<Prop_List>();
            if (propList != null)
            {
                propList.sex_category = (sex)selectedSex;
                propList.skin_category = (skin)selectedSkin;
                propList.body_index = i;
            }
        }
    }

    public void GetAllBackPackList()
    {
        // Clear old buttons
        foreach (Transform child in BackPackListParent.transform)
        {
            Destroy(child.gameObject);
        }

        // Get all backpack items
        List<CustomizationManager.BackPackInfo> backpackList = CustomizationManager.instance.GetBackPackData();

        foreach (var backpackInfo in backpackList)
        {
            GameObject temp_go = Instantiate(Prop_List);
            temp_go.transform.SetParent(BackPackListParent.transform, false);

            temp_go.GetComponent<ButtonManager>().buttonText = backpackInfo.Data.BackPackName;
            temp_go.GetComponent<ButtonManager>().UpdateUI();

            // Save the category and index into Prop_List
            Prop_List propList = temp_go.GetComponent<Prop_List>();
            propList.prop_attr = attr.backpack;
            propList.backpack_category = backpackInfo.Category;
            propList.backpack_index = backpackInfo.Index;
        }
    }

    public void GetAllBeardList()
    {
        foreach (Transform child in BeardListParent.transform) { Destroy(child.gameObject); }

        List<BeardData> beardDataList = CustomizationManager.instance.GetBeardData();
        for (int i = 0; i < beardDataList.Count; i++)
        {
            var beardData = beardDataList[i];

            GameObject temp_go = Instantiate(Prop_List, BeardListParent.transform);

            ButtonManager buttonManager = temp_go.GetComponent<ButtonManager>();
            if (buttonManager != null)
            {
                buttonManager.buttonText = beardData.BeardName;
                buttonManager.UpdateUI();
            }

            Prop_List propList = temp_go.GetComponent<Prop_List>();
            if (propList != null)
            {
                propList.prop_attr = attr.beard;
                propList.beard_index = i;
            }
        }
    }

    public void GetAllHairList()
    {
        // Clear existing buttons
        foreach (Transform child in HairListParent.transform)
        {
            Destroy(child.gameObject);
        }

        Hair.SexCategory selectedSex = (Hair.SexCategory)CustomizationManager.instance.sex_category;

        // Get the list of hair data based on the sex category
        List<Hair.HairData> hairDataList = CustomizationManager.instance.GetHairData(selectedSex);

        // Iterate through the list and spawn buttons
        for (int i = 0; i < hairDataList.Count; i++)
        {
            var hairData = hairDataList[i];

            // Instantiate button prefab
            GameObject temp_go = Instantiate(Prop_List, HairListParent.transform);

            // Assign button properties
            ButtonManager buttonManager = temp_go.GetComponent<ButtonManager>();
            if (buttonManager != null)
            {
                buttonManager.buttonText = hairData.HairName;
                buttonManager.UpdateUI(); // Update button UI (e.g., set text, color, etc.)
            }

            // Optionally, assign additional data or functionality to the button (e.g., Hair index)
            Prop_List propList = temp_go.GetComponent<Prop_List>();
            if (propList != null)
            {
                propList.prop_attr = attr.hair; // Example of setting additional properties
                propList.hair_index = i;
            }
        }
    }

    public void GetAllHatList()
    {
        // Clear existing buttons first
        foreach (Transform child in HatListParent.transform)
        {
            Destroy(child.gameObject);
        }

        // Get all hat data using HatInfo
        List<HatInfo> hatInfoList = CustomizationManager.instance.GetAllHatData();

        // Iterate through the list and spawn buttons
        for (int i = 0; i < hatInfoList.Count; i++)
        {
            var hatInfo = hatInfoList[i];

            // Instantiate button prefab
            GameObject temp_go = Instantiate(Prop_List, HatListParent.transform);

            // Assign button properties
            ButtonManager buttonManager = temp_go.GetComponent<ButtonManager>();
            if (buttonManager != null)
            {
                buttonManager.buttonText = hatInfo.Data.HatName;
                buttonManager.UpdateUI(); // Update button UI (e.g., set text, color, etc.)
            }

            // Save the category and index into Prop_List
            Prop_List propList = temp_go.GetComponent<Prop_List>();
            if (propList != null)
            {
                propList.prop_attr = attr.hat; // Assuming you are using the 'hat' category attribute
                propList.hat_category = hatInfo.Category; // Save the hat category
                propList.hat_index = hatInfo.Index; // Save the hat index
            }
        }

    }

    public void GetAllFaceList()
    {
        // Clear existing buttons first
        foreach (Transform child in FaceListParent.transform)
        {
            Destroy(child.gameObject);
        }

        // Map your enums
        Face.SexCategory selectedSex = (Face.SexCategory)CustomizationManager.instance.sex_category;
        Face.SkinCategory selectedSkin = (Face.SkinCategory)CustomizationManager.instance.skin_category;

        List<Face.FaceData> faceDataList = CustomizationManager.instance.GetFaceData(selectedSex, selectedSkin);

        for (int i = 0; i < faceDataList.Count; i++)
        {
            var faceData = faceDataList[i];

            GameObject temp_go = Instantiate(Prop_List, FaceListParent.transform);

            ButtonManager buttonManager = temp_go.GetComponent<ButtonManager>();
            if (buttonManager != null)
            {
                buttonManager.buttonText = faceData.FaceName;
                buttonManager.UpdateUI();
            }

            Prop_List propList = temp_go.GetComponent<Prop_List>();
            if (propList != null)
            {
                propList.sex_category = (sex)selectedSex;
                propList.skin_category = (skin)selectedSkin;
                propList.prop_attr = attr.face;
                propList.face_index = i; // Store the index for the face
            }
        }
    }

    public void GetAllBodyPropList()
    {
        // Clear previous buttons
        foreach (Transform child in BodyPropListParent.transform)
        {
            Destroy(child.gameObject);
        }

        // Get body prop data list from CustomizationManager
        List<BodyProp.BodyPropData> bodyPropList = CustomizationManager.instance.GetBodyPropData();

        if (bodyPropList == null || bodyPropList.Count == 0)
        {
            Debug.LogWarning("No BodyProp data found!");
            return;
        }

        // Create buttons
        for (int i = 0; i < bodyPropList.Count; i++)
        {
            var bodyPropData = bodyPropList[i];
            GameObject temp_go = Instantiate(Prop_List, BodyPropListParent.transform);

            ButtonManager buttonManager = temp_go.GetComponent<ButtonManager>();
            if (buttonManager != null)
            {
                buttonManager.buttonText = bodyPropData.BodyPropName;
                buttonManager.UpdateUI();
            }

            Prop_List propList = temp_go.GetComponent<Prop_List>();
            if (propList != null)
            {
                propList.prop_attr = attr.body_prop;
                propList.bodyprop_index = i; // Store the index for the face
            }
        }

    }
    public void RandomizeCharacter()
    {
        // Randomize Sex & Skin first
        //var sexValues = System.Enum.GetValues(typeof(CustomizationManager.sex));
        //var randomSex = (CustomizationManager.sex)sexValues.GetValue(Random.Range(0, sexValues.Length));

        //var skinValues = System.Enum.GetValues(typeof(CustomizationManager.skin));
        //var randomSkin = (CustomizationManager.skin)skinValues.GetValue(Random.Range(0, skinValues.Length));


        var randomSex = CustomizationManager.instance.sex_category;
        var randomSkin = CustomizationManager.instance.skin_category;

        // Apply Body (same sex & skin)
        var bodyList = CustomizationManager.instance.GetBodyData((Body.SexCategory)randomSex, (Body.SkinCategory)randomSkin);
        if (bodyList.Count > 0)
        {
            int randomBodyIndex = Random.Range(0, bodyList.Count);
            CustomizationManager.instance.ApplyBody((Body.SexCategory)randomSex, (Body.SkinCategory)randomSkin, randomBodyIndex);
        }

        // Apply Face (using SAME sex & skin)
        var faceList = CustomizationManager.instance.GetFaceData((Face.SexCategory)randomSex, (Face.SkinCategory)randomSkin);
        if (faceList.Count > 0)
        {
            int randomFaceIndex = Random.Range(0, faceList.Count);
            CustomizationManager.instance.ApplyFace((Face.SexCategory)randomSex, (Face.SkinCategory)randomSkin, randomFaceIndex);
        }

        // Randomize Hair/Beard for this sex
        var hairList = CustomizationManager.instance.GetHairData((Hair.SexCategory)randomSex);
        if (hairList.Count > 0)
            CustomizationManager.instance.ApplyHair(Random.Range(0, hairList.Count));

        var beardList = CustomizationManager.instance.GetBeardData();
        if (beardList.Count > 0)
            CustomizationManager.instance.ApplyBeard(Random.Range(0, beardList.Count));

        // Randomize Hat/Backpack/BodyProp
        var hats = CustomizationManager.instance.GetAllHatData();
        if (hats.Count > 0)
        {
            var randomHat = hats[Random.Range(0, hats.Count)];
            CustomizationManager.instance.ApplyHat(randomHat.Category, randomHat.Index);
        }

        var backpacks = CustomizationManager.instance.GetBackPackData();
        if (backpacks.Count > 0)
        {
            var randomBackpack = backpacks[Random.Range(0, backpacks.Count)];
            CustomizationManager.instance.ApplyBackpack(randomBackpack.Category, randomBackpack.Index);
        }

        //var bodyPropList = CustomizationManager.instance.GetBodyPropData();
        //if (bodyPropList.Count > 0)
        //    CustomizationManager.instance.ApplyBodyProp(Random.Range(0, bodyPropList.Count));
    }

    public void OnConfirmButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }


}
