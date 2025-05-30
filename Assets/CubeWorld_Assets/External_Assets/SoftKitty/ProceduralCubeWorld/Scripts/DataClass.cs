using System.Collections;
using System.Collections.Generic;
using FishNet.Serializing;
using UnityEngine;
namespace SoftKitty.PCW
{

    #region pool class
    [System.Serializable]
    public class BlockModelGroup
    {
        public List<GameObject> _models = new List<GameObject>();
    }

    [System.Serializable]
    public class BlockModelSet
    {
        public BlockModelGroup[] _sets;
    }

    [System.Serializable]
    public class PropCollection
    {
        public List<PropInfo> _models = new List<PropInfo>();
    }
    #endregion

    #region cube class
    public class BlockInstance
    {
        public BlockInfo _info;
        public BlockPool.BlockType _type;
        public int _customUid = 0;
        public short _level;
        public Transform _parent;
        public byte _angle;
        public byte _rnd;
        public Vector3 _localPos;
        public byte _localY;
        public BlockCube _blockCube;
        public bool _forceToShow = false;
        public static BlockInstance CreateInstance(BlockPool.BlockType type, int level, Transform parent, int angle, int rnd, Vector3 localPos, byte localY,int _uid=0,bool _forceShow=false)
        {
            BlockInstance _newInstance = new BlockInstance();
            _newInstance._type = type;
            _newInstance._level = (short)level;
            _newInstance._parent = parent;
            _newInstance._angle = (byte)angle;
            _newInstance._rnd = (byte)rnd;
            _newInstance._localPos = localPos;
            _newInstance._localY = localY;
            _newInstance._customUid = _uid;
            _newInstance._forceToShow = _forceShow;
            return _newInstance;
        }

        public BlockInstance Copy()
        {
            BlockInstance _newInstance = new BlockInstance();
            _newInstance._info = null;
            _newInstance._type = this._type;
            _newInstance._level = this._level;
            _newInstance._parent = this._parent;
            _newInstance._angle = this._angle;
            _newInstance._rnd = this._rnd;
            _newInstance._localPos = this._localPos;
            _newInstance._localY = this._localY;
            _newInstance._customUid = this._customUid;
            _newInstance._forceToShow = this._forceToShow;
            return _newInstance;
        }

        public bool MoveDown()
        {
            if (_localY > 0)
            {
                _localY--;
                if (_localPos.y > -1F) _localPos.y -= 1F;
                return true;
            }
            else
            {
                return false;
            }
        }
        public Color GetTopColor()
        {
            if (_localPos.y <= -1F) return new Color(0.1F,0.5F,1F,1F);
             return BlockPool.instance.GetBlockPrefab(_type, _level,_customUid).GetColor();
        }

        public Color GetSideColor()
        {
            if (_localPos.y <= -1F) return new Color(0.1F, 0.5F, 1F, 1F);
            return BlockPool.instance.GetBlockPrefab(_type, _level, _customUid).GetSideColor();
        }

        public void Load(BlockCube _cube)
        {
            if (_info == null)
            {
                _blockCube = _cube;
                _info = BlockPool.instance.CreateBlock(_type, _level, _parent, 0, _rnd, _localPos, _customUid);
                if (_info != null) _info._blockInstance = this;
            }
        }

        public bool isLoaded()
        {
            return _info != null;
        }

        public void Unload()
        {
            if (_info != null) _info.Destroy();
            _info = null;
        }

        
    }
    public class BlockCube
    {
        public byte _localX;
        public byte _localY;
        public byte _localZ;
        public bool _walkable = true;
        public isLand _parentLand;
        public BlockPool.BlockType _type;
        public bool _isMountain = false;
        public short _level = 0;
        public bool Loaded = false;
        public List<BlockInstance> _instance = new List<BlockInstance>();
        public PropInstance _groundProp;

        public bool isInstanceVisible(int i)
        {
            if (_instance[i]._forceToShow) return true;
            if (_parentLand._Cubes[Mathf.Max(_localX - 1, 0), _localZ].isInstanceExist(_instance[i]._localY)
                && _parentLand._Cubes[Mathf.Min(_localX + 1, 199), _localZ].isInstanceExist(_instance[i]._localY)
                 && _parentLand._Cubes[_localX, Mathf.Max(_localZ - 1, 0)].isInstanceExist(_instance[i]._localY)
                 && _parentLand._Cubes[_localX, Mathf.Min(_localZ + 1, 199)].isInstanceExist(_instance[i]._localY)
                 && _parentLand._Cubes[_localX, _localZ].isInstanceExist(_instance[i]._localY+1)
                 && i != _instance.Count - 1 && _localX != 0 && _localX != 199 && _localZ != 0 && _localZ != 199
                ) return false;
            return true;
        }
       
        public void BuildNewInstance(BlockInstance _newInstance,bool _load,bool _modifyData=true)
        {
            int _startX = _instance.Count;
            BlockInstance _createInstance = _newInstance.Copy();
            _createInstance._forceToShow = true;
            _instance.Add(_createInstance);
            _instance.Sort(SortByHeight);
            if (_load)
            {
                int i = _instance.IndexOf(_createInstance);
                if (isInstanceVisible(i))
                    _instance[i].Load(this);

                if (_groundProp != null)
                {
                    _groundProp.Unload();
                    _groundProp = null;
                    _walkable = true;
                }
            }
            else
            {
                if (_groundProp != null) _groundProp.Move(1);
            }
            _localY = GetTopInstance()._localY;
            if (_modifyData)
            {
                _parentLand._modification.Add(new BlockModifyData()
                {
                    _addUID = _newInstance._customUid,
                    _mode = 0,
                    _addAngle = _newInstance._angle,
                    _addLevel = _newInstance._level,
                    _addType = (int)_newInstance._type,
                    _posX = _localX,
                    _posY = _newInstance._localPos.y,
                    _posZ = _localZ,
                    _y = _newInstance._localY
                });
            }
            _instance.Sort(SortByHeight);
        }

        public bool isInstanceExist(int _height)
        {
            for (int i = 0; i < _instance.Count; i++)
            {
                if (_instance[i] != null && _instance[i]._localY == _height) return true;
            }
            return false;
        }

        public void RemoveInstance(byte _y,bool _modifyData = true)
        {
            if (_instance.Count > 0)
            {
                if (_instance.Count == 1)
                {
                    if (!_instance[_instance.Count - 1].MoveDown())
                    {
                        return;
                    }
                }
                else
                {
                    for (int i = _instance.Count - 1; i >= 0; i--)
                    {
                        if (_instance[i]._localY == _y)
                        {
                            _instance[i].Unload();
                            _instance.RemoveAt(i);
                        }
                    }
                    _instance.Sort(SortByHeight);
                }
                if(GetTopNativeInstance()!=null)GetTopNativeInstance()._forceToShow = true;
                List<BlockCube> _reloadCubes = new List<BlockCube>();
                //Reload cubes around, because we only added cubes on the surface, when we remove a cube, the cubes around will leave holes on the side which we need to fix.
                for (int _x=_localX-1;_x<= _localX + 1;_x++) {
                    for (int _z = _localZ - 1; _z <= _localZ + 1; _z++)
                    {
                        if (_x>=0 && _x<200 && _z>=0 && _z<200 && (_x!= _localX || _z!=_localZ)) {
                            if (_parentLand._Cubes[_x,_z]!=null) {
                                _parentLand._Cubes[_x, _z].Unload();
                                _reloadCubes.Add(_parentLand._Cubes[_x, _z]);
                            }
                        }
                    }
                }
                //Patch cubes around

                _localY=GetTopInstance()._localY;
                if (_groundProp != null)
                {
                    _groundProp.Unload();
                    _groundProp = null;
                    _walkable = true;
                }
                if (Loaded)
                {
                    Unload();
                    Load();
                }
                foreach (BlockCube obj in _reloadCubes) {
                    obj.Load();
                }
                _reloadCubes.Clear();
                if (_modifyData)
                {
                    if (_parentLand != null)
                    {
                        _parentLand._modification.Add(new BlockModifyData()
                        {
                            _mode = 1,
                            _addAngle = 0,
                            _addUID = 0,
                            _addType = 0,
                            _addLevel = 0,
                            _posX = _localX,
                            _posY = _localY,
                            _posZ = _localZ,
                            _y = _y
                        });
                    }
                }
            }
        }

        public void insertCube(BlockInstance _newInstance, byte _height,Vector3 _newPos)
        {
            _newInstance._localY = _height;
            _newInstance._localPos = _newPos;
            _newInstance._info=null;
            _instance.Add(_newInstance);
            _instance.Sort(SortByHeight);
        }

        public int SortByHeight(BlockInstance _instance1, BlockInstance _instance2)
        {

            return _instance1._localY.CompareTo(_instance2._localY);
        }
        public BlockCube()
        {
            _level = 0;
            Loaded = false;
        }

        public BlockInstance GetInstanceByHeight(int _height)
        {
            for (int i = 0; i < _instance.Count; i++)
            {
                if (_instance[i] != null && _instance[i]._localY == _height) return _instance[i];
            }
            return GetTopInstance();
        }
        public BlockInstance GetTopInstance()
        {
            if (_instance.Count > 0)
                return _instance[_instance.Count - 1];
            else
                return null;
        }

        public BlockInstance GetTopNativeInstance()
        {

            if (_instance.Count > 0)
            {
                for (int i = _instance.Count - 1; i >= 0; i--)
                {
                    if (_instance[i]._customUid == 0)
                    {
                        return _instance[i];
                    }
                }
                return null;
            }
            else
                return null;
        }

        public void Load()
        {
            if (Loaded) return;
            for (int i = 0; i < _instance.Count; i++)
            {
                if (isInstanceVisible(i))
                    _instance[i].Load(this);
            }
            BlockGenerator.CubeLoaded++;
            if (_groundProp != null) _groundProp.Load(GetTopInstance()._info.transform,_isMountain);
            Loaded = true;
        }

        public void Unload()
        {
            foreach (BlockInstance obj in _instance)
            {
                obj.Unload();
            }
            if (_groundProp != null) _groundProp.Unload();
            Loaded = false;
        }
    }

    [System.Serializable]
    public class BlockModifyData
    {
        public int _posX;
        public float _posY;
        public int _posZ;
        public int _y;
        public int _mode = 0;//0-add 1-del
        public int _addUID;
        public int _addType;
        public int _addLevel;
        public int _addAngle;

    }


    public class isLand
    {
        public BlockCube[,] _Cubes = new BlockCube[200, 200];
        public List<BlockModifyData> _modification = new List<BlockModifyData>();
        public string _seed;
        public byte _style;
        public GameObject _root;
        public IslandInfo _info;
        public bool _loaded = false;
        public int _posX;
        public int _posY;
        public isLand()
        {
            _Cubes = new BlockCube[200, 200];
            _loaded = false;
        }
        public float GetSurfaceHeight(Vector3 _pos)
        {
            Vector3 _local = _root.transform.InverseTransformPoint(_pos);
            int _x = Mathf.RoundToInt(_local.x);
            int _y = Mathf.RoundToInt(_local.z);
            if (_x < 0 || _x >= 200 || _y < 0 || _y >= 200) return -100F;
            if (_Cubes[_x, _y].GetTopInstance() == null) return -100F;
            return _Cubes[_x, _y].GetTopInstance()._localPos.y + 0.5F;
        }

        public bool GetSurfaceWalkable(Vector3 _pos)
        {
            Vector3 _local = _root.transform.InverseTransformPoint(_pos);
            int _x = Mathf.RoundToInt(_local.x);
            int _y = Mathf.RoundToInt(_local.z);
            if (_x < 0 || _x >= 200 || _y < 0 || _y >= 200) return false;
            return _Cubes[_x, _y]._walkable;
        }
    }
    #endregion

    #region prop class
    public class PropInstance
    {
        public isLand _land;
        public PropInfo _info;
        public BlockPool.BlockType _type;
        public short _level;
        public byte _angle;
        public byte _style;
        public byte _rnd;
        public byte localX;
        public byte localY;
        public byte localZ;

        public void Move(byte _offset)
        {
            localY += _offset;
        }
        public static PropInstance CreateInstance(BlockPool.BlockType type, int level, int angle, int style, int rnd,
            byte localX, byte localY, byte localZ, isLand land)
        {
            PropInstance _newInstance = new PropInstance();
            _newInstance._type = type;
            _newInstance._level = (short)level;
            _newInstance._angle = (byte)angle;
            _newInstance._style = (byte)style;
            _newInstance._rnd = (byte)rnd;
            _newInstance.localX = localX;
            _newInstance.localY = localY;
            _newInstance.localZ = localZ;
            _newInstance._land = land;
            return _newInstance;
        }

        public void Set()
        {
            if (_info != null)
            {
                bool _shouldUnload = false;
                if (_info.GroundLeftTopCorner == Vector2.zero && _info.GroundRightBottomCorner == Vector2.zero)
                {
                    _land._Cubes[localX, localZ]._walkable = false;
                }
                else
                {
                    for (int _x = Mathf.FloorToInt(_info.GroundLeftTopCorner.x); _x <= Mathf.FloorToInt(_info.GroundRightBottomCorner.x); _x++)
                    {
                        for (int _z = Mathf.FloorToInt(_info.GroundLeftTopCorner.y); _z <= Mathf.FloorToInt(_info.GroundRightBottomCorner.y); _z++)
                        {
                            if (localX + _x >= 0 && localX + _x < 200 && localZ + _z >= 0 && localZ + _z < 200)
                            {
                                _land._Cubes[localX + _x, localZ + _z]._walkable = false;
                                if (_x != 0 || _z != 0)
                                {
                                    if (_land._Cubes[localX + _x, localZ + _z]._localY < localY)
                                    {
                                        _shouldUnload = true;
                                    }
                                }
                            }

                        }
                    }
                }
                if (_shouldUnload) Unload();
            }
        }

        public void Load(Transform _parent, bool _isMountain)
        {
            if (_info == null) _info = BlockPool.instance.CreateProp(_type, _level, _parent, _angle, _style, _rnd, _isMountain);
            Set();
        }

        public void Unload()
        {
            if (_info != null) _info.Destroy();
            _info = null;
        }
    }
    #endregion

    #region misc class
    [System.Serializable]
    public class WorldSaveData
    {
        public IslandSaveData [] TerrainArray;
        public Vector3 PlayerPos;
        public Vector3 PlayerRot;
    }
    [System.Serializable]
    public class IslandSaveData
    {
        public Vector2 Pos;
        public string Seed;
        public BlockModifyData[] Modifications;
    }


    [System.Serializable]
    public class CubeWorldSettings
    {
        public int StyleID = 0;
        public string Name;
        [Range(2, 8)]
        public int MoutainHeight = 4;
        public bool WithSnow = true;
        public bool WithGrass = true;
        public bool WithLava = true;
        public bool WithWater = true;
        public bool Expand = false;

        public CubeWorldSettings Copy()
        {
            CubeWorldSettings _copy = new CubeWorldSettings();

            return _copy;
        }
    }
    #endregion

    public static class BlockInstanceRPC
    {
        public static void WriteBlockInstanceRPC(this Writer writer, BlockInstance block)
        {
            writer.WriteInt16((short)block._type);
            writer.WriteInt16(block._level);
            writer.WriteTransform(block._parent);
            writer.WriteUInt8Unpacked(block._angle);
            writer.WriteUInt8Unpacked(block._rnd);
            writer.WriteVector3(block._localPos);
            writer.WriteUInt8Unpacked(block._localY);
            writer.WriteInt32(block._customUid);
            writer.WriteBoolean(block._forceToShow);
        }

        public static BlockInstance ReadBlockInstanceRPC(this Reader reader)
        {
            return new BlockInstance()
            {
                _type = (BlockPool.BlockType)reader.ReadInt16(),
                _level = reader.ReadInt16(),
                _parent = reader.ReadTransform(),
                _angle = reader.ReadUInt8Unpacked(),
                _rnd = reader.ReadUInt8Unpacked(),
                _localPos = reader.ReadVector3(),
                _localY = reader.ReadUInt8Unpacked(),
                _customUid = reader.ReadInt32(),
                _forceToShow = reader.ReadBoolean()
            };
        }
    }
}
