using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SoftKitty.PCW
{
    public class PropInfo : MonoBehaviour
    {
        #region public settings
        public int mUID;
        //The prop with StyleID greater than 1 will only spawn in the area with the matching StyleID
        public int StyleID = 0;//0:All, >1:Specific ID
        //Define how many meters this prop will take place.
        //For example: GroundLeftTopCorner(-1,-1) GroundRightBottomCorner(1,1)means this prop will cover 2x2 square meters ground. If both value set to(0,0) means cover 1x1 ground.
        public Vector2 GroundLeftTopCorner = new Vector2(1,1);
        public Vector2 GroundRightBottomCorner = new Vector2(1, 1);
        //RandomChanceMulti will make this prop has x times more chance to spawn
        public int RandomChanceMulti = 1;
        //This prop will only spawn within the following height range.(0 is the water level)
        public Vector2 HeightRange=new Vector2(0,8);
        //This prop will only spawn on the following terrain:
        public bool CanPlaceOnMud = true;
        public bool CanPlaceOnSand = true;
        public bool CanPlaceOnSnow = true;
        public bool CanPlaceOnRock = true;
        public bool CanPlaceOnWater = true;
        public bool CanPlaceOnLava = true;
        #endregion

        #region public functions
        public void Destroy()
        {
            transform.SetParent(BlockPool.instance.mPoolRoot);
            transform.localPosition = Vector3.zero;
            BlockPool.instance.mPropPools[mUID]._models.Add(this);
            gameObject.SetActive(false);
        }

        public void UpdatePrefab()
        {
          #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
          #endif
        }
        #endregion

    }
}
