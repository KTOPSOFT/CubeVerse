using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoftKitty.PCW
{
    public class BlockInfo : MonoBehaviour
    {
        #region public settings
        //Please select the closest type of this cube.
        public BlockPool.BlockType type;

        public enum CustomBlockType { Common , Rare , Epic , Legendary , Mythic }
        public CustomBlockType customBlockType;
        //The cube will only appear in the matching height of the terrain.
        [Range(0,4)]
        public int _height;
        //Set the main color of the top surface of this cube.(This is for LOD generation and mini map display)
        public Color _topColor;
        //Set the main color of the side surface of this cube.(This is for LOD generation)
        public Color _sideColor;
        //Set the speed of player walk on this cube in percentage.
        [Range(10,100)]
        public byte _walkableSpeed = 100; //100%
        //Set the _friction of player walk on this cube in percentage.
        [Range(0, 100)]
        public byte _friction = 100; //100%
        public int _customUid = 0;
        #endregion

        #region internal functions
        public BlockInstance _blockInstance;
        public Color GetColor()
        {
            if (transform.position.y <= -0.5F)
                return new Color(0.2F, 0.7F, 1F, 1F);
            else
                return _topColor;
        }

        public Color GetSideColor()
        {
            if (transform.position.y <= -0.5F)
                return new Color(0.2F, 0.7F, 1F, 1F);
            else
                return _sideColor;
        }

        public void UpdatePrefab()
        {
         #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
         #endif
        }
        #endregion

        #region public functions
        public void Destroy()
        {
            transform.SetParent(BlockPool.instance.mPoolRoot);
            transform.localPosition = Vector3.zero;
            if (type == BlockPool.BlockType.Custom)
            {
                BlockPool.instance.mBlockCustomPool._sets[_customUid]._models.Add(gameObject);
            }
            else
            {
                BlockPool.instance.mBlockPools[_height]._sets[(int)type]._models.Add(gameObject);
            }
            gameObject.SetActive(false);
        }
        #endregion
    }
}
