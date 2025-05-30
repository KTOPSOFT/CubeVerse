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

        CharacterInfo.skin_category = CustomizationManager.instance.skin_category;
        CharacterInfo.sex_category = CustomizationManager.instance.sex_category;

        CharacterInfo.body_index = CustomizationManager.instance.body_index;

        CharacterInfo.backpack_category = CustomizationManager.instance.backpack_category;
        CharacterInfo.backpack_index = CustomizationManager.instance.backpack_index;

        CharacterInfo.hair_index = CustomizationManager.instance.hair_index;
        CharacterInfo.beard_index = CustomizationManager.instance.beard_index;

        CharacterInfo.hat_category = CustomizationManager.instance.hat_category;
        CharacterInfo.hat_index = CustomizationManager.instance.hat_index;

        CharacterInfo.face_index = CustomizationManager.instance.face_index;
        CharacterInfo.bodyprop_index = CustomizationManager.instance.bodyprop_index;
        Debug.Log("Character Data Received");
    }
}
