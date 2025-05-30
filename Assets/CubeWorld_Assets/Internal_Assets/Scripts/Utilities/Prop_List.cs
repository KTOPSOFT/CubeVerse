using System.Linq;
using UnityEngine;
using static BackPack;
using static Body;
using static CustomizationManager;
using static HatProp;

public class Prop_List : MonoBehaviour
{
    public enum attr { body, backpack, beard, hair, hat , body_prop, face }

    public attr prop_attr;

    public sex sex_category;
    public skin skin_category;
    public int body_index;
    public BackPackCategory backpack_category;
    public int backpack_index;
    public int beard_index;
    public int hair_index;
    public HatCategory hat_category;
    public int hat_index;
    public int face_index;
    public int bodyprop_index;

    public void ApplyProp()
    {
        switch (prop_attr)
        {
            case attr.body:
                CustomizationManager.instance.ApplyBody((SexCategory)(int)sex_category, (SkinCategory)(int)skin_category, body_index);
                break;

            case attr.backpack:
                CustomizationManager.instance.ApplyBackpack(backpack_category, backpack_index);
                break;

            case attr.beard:
                CustomizationManager.instance.ApplyBeard(beard_index);
                break;

            case attr.hair:
                CustomizationManager.instance.ApplyHair(hair_index);
                break;

            case attr.hat:
                CustomizationManager.instance.ApplyHat(hat_category, hat_index);
                break;

            case attr.face:
                CustomizationManager.instance.ApplyFace((Face.SexCategory)(int)sex_category, (Face.SkinCategory)(int)skin_category, face_index);
                break;

            case attr.body_prop:
                CustomizationManager.instance.ApplyBodyProp(bodyprop_index);
                break;
        }
    }
}
