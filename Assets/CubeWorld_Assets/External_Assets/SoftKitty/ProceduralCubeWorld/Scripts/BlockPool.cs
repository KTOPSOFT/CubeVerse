using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoftKitty.PCW
{
    public class BlockPool : MonoBehaviour
    {
        #region static variables
        public static BlockPool instance;
        #endregion

        #region public settings
        //Total visual styles of yourprops
        public int mTotalStyle = 2;
        public enum BlockType
        {
            Grass,
            Mud,
            Stone,
            Lava,
            Custom
        }

        public BlockModelSet[] mBlockPrefabs; //Terrain Cube Prefabs
       
        public PropInfo[] mProps; //Prop Prefabs
        public List<GameObject> mCustomCubePrefabs = new List<GameObject>();//Custom Cube Prefabs
        #endregion

        #region internal variables
        [HideInInspector]
        public Transform mPoolRoot;
        public BlockModelSet[] mBlockPools;
        public BlockModelSet mBlockCustomPool;
        public int ChildCount = 0;
        public List<PropCollection> mMudProps = new List<PropCollection>();
        public List<PropCollection> mSandProps = new List<PropCollection>();
        public List<PropCollection> mSnowProps = new List<PropCollection>();
        public List<PropCollection> mRockProps = new List<PropCollection>();
        public List<PropCollection> mWaterProps = new List<PropCollection>();
        public List<PropCollection> mLavaProps = new List<PropCollection>();
        public Dictionary<int, PropCollection> mPropPools = new Dictionary<int, PropCollection>();
        #endregion

        #region MonoBehaviour
        void Awake()
        {
            instance = this;
            GameObject _newRoot = new GameObject("PoolRoot");
            _newRoot.transform.SetParent(transform);
            _newRoot.transform.localPosition = Vector3.zero;
            _newRoot.transform.localEulerAngles = Vector3.zero;
            _newRoot.transform.localScale = Vector3.one;
            mPoolRoot = _newRoot.transform;
            Initialize();
            BlockInfo[] _infos = transform.GetComponentsInChildren<BlockInfo>(true);
            foreach (BlockInfo _info in _infos)
            {
                _info.gameObject.SetActive(false);
            }
            foreach (var _info in mProps)
            {
                _info.gameObject.SetActive(false);
            }
        }

        public void UpdatePrefab()
        {
           #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
           #endif
        }
        #endregion

        #region internal functions
        public void Initialize()
        {
            mProps = transform.GetComponentsInChildren<PropInfo>(true);
            mMudProps.Clear();
            mSandProps.Clear();
            mSnowProps.Clear();
            mRockProps.Clear();
            mWaterProps.Clear();
            mLavaProps.Clear();
            mPropPools.Clear();
            mCustomCubePrefabs.Clear();

            mTotalStyle = GetComponent<BlockGenerator>().mSettings.Count;
            for (int i = 0; i < mTotalStyle; i++)
            {
                mMudProps.Add(new PropCollection());
                mSandProps.Add(new PropCollection());
                mSnowProps.Add(new PropCollection());
                mRockProps.Add(new PropCollection());
                mWaterProps.Add(new PropCollection());
                mLavaProps.Add(new PropCollection());
            }


            for (int i = 0; i < mProps.Length; i++)
            {
                mProps[i].mUID = (byte)i;
                for (int u = 0; u < mProps[i].RandomChanceMulti; u++)
                {
                    if (mProps[i].CanPlaceOnMud) mMudProps[Mathf.Clamp(mProps[i].StyleID, 0, mTotalStyle-1)]._models.Add(mProps[i]);
                    if (mProps[i].CanPlaceOnSand) mSandProps[Mathf.Clamp(mProps[i].StyleID, 0, mTotalStyle-1)]._models.Add(mProps[i]);
                    if (mProps[i].CanPlaceOnSnow) mSnowProps[Mathf.Clamp(mProps[i].StyleID, 0, mTotalStyle-1)]._models.Add(mProps[i]);
                    if (mProps[i].CanPlaceOnRock) mRockProps[Mathf.Clamp(mProps[i].StyleID, 0, mTotalStyle-1)]._models.Add(mProps[i]);
                    if (mProps[i].CanPlaceOnWater) mWaterProps[Mathf.Clamp(mProps[i].StyleID, 0, mTotalStyle-1)]._models.Add(mProps[i]);
                    if (mProps[i].CanPlaceOnLava) mLavaProps[Mathf.Clamp(mProps[i].StyleID, 0, mTotalStyle - 1)]._models.Add(mProps[i]);
                }
                mPropPools.Add(mProps[i].mUID, new PropCollection());
            }

            for (int i = 1; i < mTotalStyle; i++)
            {
                mMudProps[i]._models.AddRange(mMudProps[0]._models);
                mSandProps[i]._models.AddRange(mSandProps[0]._models);
                mSnowProps[i]._models.AddRange(mSnowProps[0]._models);
                mRockProps[i]._models.AddRange(mRockProps[0]._models);
                mWaterProps[i]._models.AddRange(mWaterProps[0]._models);
                mLavaProps[i]._models.AddRange(mLavaProps[0]._models);
            }

            mBlockPrefabs = new BlockModelSet[5];
            mBlockPools = new BlockModelSet[5];
            for (int height = 0; height < 5; height++)
            {
                mBlockPrefabs[height] = new BlockModelSet();
                mBlockPools[height] = new BlockModelSet();
                mBlockPrefabs[height]._sets = new BlockModelGroup[4];
                mBlockPools[height]._sets = new BlockModelGroup[4];
                for (int _type = 0; _type < 4; _type++)
                {
                    mBlockPrefabs[height]._sets[_type] = new BlockModelGroup();
                    mBlockPools[height]._sets[_type] = new BlockModelGroup();
                    mBlockPrefabs[height]._sets[_type]._models.Clear();
                    mBlockPools[height]._sets[_type]._models.Clear();
                }
            }
            BlockInfo[] _infos = transform.GetComponentsInChildren<BlockInfo>(true);
            foreach (BlockInfo _info in _infos) {
                if(_info.gameObject.GetComponent<BoxCollider>()) _info.gameObject.GetComponent<BoxCollider>().size = Vector3.one;
                if (_info.type == BlockType.Custom)
                {
                    _info._customUid = mCustomCubePrefabs.Count;
                    mCustomCubePrefabs.Add(_info.gameObject);
                }
                else
                {
                    mBlockPrefabs[Mathf.Clamp(_info._height, 0, 4)]._sets[(int)_info.type]._models.Add(_info.gameObject);
                }
            }
            Debug.Log("CustomBlock Count is " + mCustomCubePrefabs.Count);
            mBlockCustomPool = new BlockModelSet();
            mBlockCustomPool._sets = new BlockModelGroup[5];
            for (int i = 0; i < mBlockCustomPool._sets.Length; i++)
            {
                mBlockCustomPool._sets[i] = new BlockModelGroup();
                mBlockCustomPool._sets[i]._models.Clear();
            }
            
            for (int i = 0; i < mCustomCubePrefabs.Count; i++) 
            {
                switch (mCustomCubePrefabs[i].GetComponent<BlockInfo>().customBlockType)
                {
                    case BlockInfo.CustomBlockType.Common:
                        {
                            mBlockCustomPool._sets[0]._models.Add(mCustomCubePrefabs[i]);
                            break;
                        }
                    case BlockInfo.CustomBlockType.Rare:
                        {
                            mBlockCustomPool._sets[1]._models.Add(mCustomCubePrefabs[i]);
                            break;
                        }
                    case BlockInfo.CustomBlockType.Epic:
                        {
                            mBlockCustomPool._sets[2]._models.Add(mCustomCubePrefabs[i]);
                            break;
                        }
                    case BlockInfo.CustomBlockType.Legendary:
                        {
                            mBlockCustomPool._sets[3]._models.Add(mCustomCubePrefabs[i]);
                            break;
                        }
                    case BlockInfo.CustomBlockType.Mythic:
                        {
                            mBlockCustomPool._sets[4]._models.Add(mCustomCubePrefabs[i]);
                            break;
                        }
                }
            }
        }

        public PropInfo CreateProp(BlockType _type, int _level,Transform _parent,int _angle,int _style,int _rnd,bool _isMountain)
        {
            List<PropInfo> _propPool = new List<PropInfo>();
            PropCollection _collection = GetCollection(_type, _level, _style, _isMountain);
            for (int i= 0;i< _collection._models.Count;i++) {
                if (_level>= _collection._models[i].HeightRange.x && _level <= _collection._models[i].HeightRange.y)
                {
                    _propPool.Add(_collection._models[i]);
                }
            }
            int _index = Mathf.FloorToInt(_rnd / 100F * _propPool.Count);
            if (_index >= _propPool.Count) _index = _propPool.Count - 1;
            if (_propPool.Count <= 0) return null;
            int _key = _propPool[_index].mUID;
            if (mPropPools[_key]._models.Count > 0)
            {
                PropInfo _result = mPropPools[_key]._models[0];
                _result.transform.SetParent(_parent);
                _result.transform.localPosition = Vector3.up * 0.5F;
                _result.transform.localEulerAngles = new Vector3(0F, _angle * 90F, 0F);
                _result.transform.localScale = Vector3.one;
                _result.gameObject.SetActive(true);
                mPropPools[_key]._models.Remove(_result);
                return _result;
            }
            else
            {
                GameObject _result = Instantiate(_propPool[_index].gameObject);
                _result.transform.SetParent(_parent);
                _result.transform.localPosition =  Vector3.up * 0.5F;
                _result.transform.localEulerAngles = new Vector3(0F, _angle * 90F, 0F);
                _result.transform.localScale = Vector3.one;
                _result.gameObject.SetActive(true);
                return _result.GetComponent<PropInfo>();
            }
        }


        private PropCollection GetCollection(BlockType _type, int _level,int _style,bool _isMountain)
        {
            if (_level <= -1F && !_isMountain)
            {
                return mWaterProps[_style];
            }
            else
            {
                if (_level >= 1 && BlockGenerator.instance.mSettings[_style].WithLava && _type== BlockType.Lava)
                    return mLavaProps[_style];
                else if (_level >= 4 && BlockGenerator.instance.mSettings[_style].WithSnow)
                    return mSnowProps[_style];
                else if (_level >= 3 && (_type == BlockType.Grass || _type == BlockType.Mud))
                    return BlockGenerator.instance.mSettings[_style].WithGrass?mMudProps[_style]: mRockProps[_style];
                else if (_level <= 0 && (_type== BlockType.Grass || _type == BlockType.Mud) && !_isMountain)
                    return mSandProps[_style];
                else if (_type == BlockType.Stone || _type == BlockType.Lava)
                    return mRockProps[_style];
                else if (_type == BlockType.Grass && _level>=2 && !_isMountain)
                    return mMudProps[_style];
                else if (_type == BlockType.Mud && _level >= 1 && !_isMountain)
                    return mMudProps[_style];
                else
                    return mSnowProps[_style];
            }
        }

        public BlockInfo GetBlockPrefab(BlockType _type, int _level,int _uid=0)
        {
            if (_type == BlockType.Custom)
            {
                return mCustomCubePrefabs[_uid].GetComponent<BlockInfo>();
            }
            else
            {
                _level = Mathf.Clamp(_level, 0, mBlockPools.Length - 1);
                return mBlockPrefabs[_level]._sets[(int)_type]._models[0].GetComponent<BlockInfo>();
            }
        }

        public BlockInfo CreateBlock(BlockType _type, int _level,Transform _parent,int _angle, int _rnd, Vector3 _localPos, int _uid=0)
        {
            if (_type == BlockType.Custom)
            {
                if (_uid < mBlockCustomPool._sets.Length)
                {
                    if (mBlockCustomPool._sets[_uid]._models.Count > 0)
                    {
                        GameObject _result = mBlockCustomPool._sets[_uid]._models[0];
                        mBlockCustomPool._sets[_uid]._models.Remove(_result);
                        _result.transform.SetParent(_parent);
                        _result.transform.localPosition = _localPos;
                        _result.transform.localEulerAngles = new Vector3(0F, _angle * 90F, 0F);
                        _result.transform.localScale = Vector3.one;
                        _result.SetActive(true);
                        return _result.GetComponent<BlockInfo>();
                    }
                    else
                    {
                        GameObject _result = Instantiate(mCustomCubePrefabs[_uid], _parent);
                        _result.transform.localPosition = _localPos;
                        _result.transform.localEulerAngles = new Vector3(0F, _angle * 90F, 0F);
                        _result.transform.localScale = Vector3.one;
                        _result.SetActive(true);
                        return _result.GetComponent<BlockInfo>();
                    }
                }
                else
                {
                    Debug.LogError("Trying to load Custom Cube with invalid UID: " + _uid);
                    return null;
                }
            }
            else
            {
                _level = Mathf.Clamp(_level, 0, mBlockPools.Length - 1);
                if (mBlockPools[_level]._sets[(int)_type]._models.Count > 0)
                {
                    GameObject _result = mBlockPools[_level]._sets[(int)_type]._models[0];
                    mBlockPools[_level]._sets[(int)_type]._models.Remove(_result);
                    _result.transform.SetParent(_parent);
                    _result.transform.localPosition = _localPos;
                    _result.transform.localEulerAngles = new Vector3(0F, _angle * 90F, 0F);
                    _result.transform.localScale = Vector3.one;
                    _result.SetActive(true);
                    return _result.GetComponent<BlockInfo>();
                }
                else
                {
                    int _rndId = Mathf.FloorToInt(_rnd / 100F * mBlockPrefabs[_level]._sets[(int)_type]._models.Count);
                    GameObject _result = Instantiate(mBlockPrefabs[_level]._sets[(int)_type]._models[_rndId], _parent);
                    _result.transform.localPosition = _localPos;
                    _result.transform.localEulerAngles = new Vector3(0F, _angle * 90F, 0F);
                    _result.transform.localScale = Vector3.one;
                    _result.SetActive(true);
                    return _result.GetComponent<BlockInfo>();
                }
            }
        }

        public BlockInfo CreateCustomBlock(BlockType _type, int _level, Transform _parent, int _angle, int _rnd, Vector3 _localPos, int _uid = 0, int CustomType = 0)
        {
            if (_type == BlockType.Custom)
            {
                GameObject _result = Instantiate(mBlockCustomPool._sets[CustomType]._models[_uid], _parent);
                _result.transform.localPosition = _localPos;
                _result.transform.localEulerAngles = new Vector3(0F, _angle * 90F, 0F);
                _result.transform.localScale = Vector3.one;
                _result.SetActive(true);
                Debug.Log(_result.gameObject.name); 
                return _result.GetComponent<BlockInfo>();
            }
            else
            {
                return null;
            }
        }
        #endregion

    }
}
