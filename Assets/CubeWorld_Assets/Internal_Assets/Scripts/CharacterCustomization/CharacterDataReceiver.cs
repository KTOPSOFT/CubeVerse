using UnityEngine;

public class CharacterDataReceiver : MonoBehaviour
{

    public static CharacterDataReceiver instance;
    public CustomCharacterInfo CharacterInfo = new CustomCharacterInfo();

    private void Awake()
    {
        instance = this;
        InitData();
    }

    public void InitData()
    {
        //CharacterInfo.skin_index = 1;
        //CharacterInfo.sex_index = 0;
        //CharacterInfo.body_index = 4;
        //CharacterInfo.body_map_index = 2;
        //CharacterInfo.hair_index = 1;
        //CharacterInfo.beard_index = 1;
        //CharacterInfo.face_index = 1;
        //CharacterInfo.hair_color = new Color(0, 0, 0);
        //CharacterInfo.beard_color = new Color(0, 0, 0);

        CustomizationManager customizationManager = CustomizationManager.instance;
        if (customizationManager == null)
        {
            Debug.LogWarning(
                "CharacterDataReceiver started without a CustomizationManager. " +
                "Using the default character data.");
            return;
        }

        CharacterInfo.skin_category = customizationManager.skin_category;
        CharacterInfo.sex_category = customizationManager.sex_category;

        CharacterInfo.body_index = customizationManager.body_index;

        CharacterInfo.backpack_category = customizationManager.backpack_category;
        CharacterInfo.backpack_index = customizationManager.backpack_index;

        CharacterInfo.hair_index = customizationManager.hair_index;
        CharacterInfo.beard_index = customizationManager.beard_index;

        CharacterInfo.hat_category = customizationManager.hat_category;
        CharacterInfo.hat_index = customizationManager.hat_index;

        CharacterInfo.face_index = customizationManager.face_index;
        CharacterInfo.bodyprop_index = customizationManager.bodyprop_index;
        Debug.Log("Character Data Received");
    }
}
