using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

namespace SoftKitty.PCW
{
    public class BlockGenerator : MonoBehaviour
    {
        #region static variables
        public static BlockGenerator instance;
        public static int CubeLoaded = 0;
        #endregion

        #region callback delegate
        public delegate void OnMinimapCreateCallback(int _offsetX, int _offsetY,int _key, Texture2D _tex);
        public delegate void OnMinimapDeleteCallback(int _key);
        #endregion

        #region public variables
        //Assign your player gameobject here
        public GameObject Player;
       //System will only load cubes around player within sight(meters)
        [Range(5,50)]
        public int Sight = 24;
       //Higher value make performance worse,Lower value make load time longer.
        [Range(10, 100)]
        public int LoadBlockPerFrame = 50;
       //Terrain Settings
        public List<CubeWorldSettings> mSettings=new List<CubeWorldSettings>();
        [Range(0.1F, 1F)]
        public float MoveSpeedInWater = 0.5F;
       //Assets
        public Texture2D mTerrainMask;
        public Texture2D mTerrainMask2;
        public bool mUseWorldMap = false;
        public Texture2D mWorldMap;
        public int mWorldMapScale = 10;
        public Vector3 PlayerPos;
        #endregion

        #region private variables
        private GameObject mIslandPrefab;
        private Dictionary<int, Texture2D> mMap = new Dictionary<int, Texture2D>();
        private Dictionary<int, isLand> mWorld = new Dictionary<int, isLand>();
        private List<BlockCube> mLoadedCube = new List<BlockCube>();
        private OnMinimapCreateCallback mMinimapCreateCallback;
        private OnMinimapDeleteCallback mMinimapDeleteCallback;
        private Vector2 PlayerCoordinate = new Vector2(0, 0);
        private bool initialized = false;
        private bool teleporting = false;
        private int _mapX=0;
        private int _mapY=0;
        private isLand standingLand = null;
        private Color[,] mTerrainMaskPixels;
        private Color[,] mTerrainMask2Pixels;
        private Color[,] mWorldMapPixels;
        #endregion

        #region MonoBehaviour
        private void Awake()
        {
            instance = this;
            initialized = false;
            mIslandPrefab = GetComponentInChildren<IslandInfo>(true).gameObject;

            mTerrainMaskPixels = new Color[mTerrainMask.width, mTerrainMask.height];
            mTerrainMask2Pixels = new Color[mTerrainMask2.width, mTerrainMask2.height];
            Color[] _mTerrainMaskPixels = mTerrainMask.GetPixels();
            Color[] _mTerrainMask2Pixels = mTerrainMask2.GetPixels();
            int _mw = mTerrainMask.width;
            int _mh = mTerrainMask.height;

            for (int x = 0; x < _mw; x++)
            {
                for (int y = 0; y < _mh; y++)
                {
                    mTerrainMaskPixels[x, y] = _mTerrainMaskPixels[y * _mw + x];
                    mTerrainMask2Pixels[x, y] = _mTerrainMask2Pixels[y * _mw + x];
                }
            }

            mWorldMapPixels = new Color[mWorldMap.width, mWorldMap.height];
            Color[] _mWorldMapPixels = mWorldMap.GetPixels();
            int _width = mWorldMap.width;
            int _height = mWorldMap.height;
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    mWorldMapPixels[x, y] = _mWorldMapPixels[y * _width + x];
                }
            }
        }

        void Update()
        {
            if (!initialized || !Player.activeSelf) return;
            PlayerPos = Player.transform.position;
            PlayerCoordinate = new Vector2(Mathf.FloorToInt(PlayerPos.x / 200F), Mathf.FloorToInt(PlayerPos.z / 200F));
            LoadCubesAround(Sight, 2);
            if (Time.frameCount % 120 == 0 && CubeLoaded > 400)
            {
                CubeLoaded = 0;
                _mapX = Mathf.FloorToInt(PlayerPos.x / 200F);
                _mapY = Mathf.FloorToInt(PlayerPos.z / 200F);
            }
            standingLand = GetIslandByPosition(PlayerPos);
            if (standingLand != null && standingLand._info!=null && standingLand._info.FakeGroundModel!=null)
            {
                GetIslandByPosition(PlayerPos)._info.FakeGroundModel.material.SetVector("_playerPos",
                    new Vector4(PlayerPos.x- standingLand._root.transform.position.x-100F, PlayerPos.z - standingLand._root.transform.position.z-100F,Sight-1F));
            }
        }

        public void UpdatePrefab()
        {
          #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
          #endif
        }
        #endregion

        #region Register Callbacks
        public void RegisterMinimapCreateCallback(OnMinimapCreateCallback _callback)
        {
            mMinimapCreateCallback = _callback;
        }

        public void RegisterMinimapDeleteCallback(OnMinimapDeleteCallback _callback)
        {
            mMinimapDeleteCallback = _callback;
        }
        #endregion

        #region Save/Load/New/Reset World
        public string SaveWorld()
        {
            WorldSaveData _newSave = new WorldSaveData();
            List<IslandSaveData> _terrainArray = new List<IslandSaveData>();
            foreach (var _island in mWorld.Values)
            {
                IslandSaveData _newIslandData = new IslandSaveData();
                _newIslandData.Pos = new Vector2(_island._posX, _island._posY);
                _newIslandData.Seed = _island._seed;
                _newIslandData.Modifications = _island._modification.ToArray();
                _terrainArray.Add(_newIslandData);
            }
            _newSave.TerrainArray = _terrainArray.ToArray();
            _newSave.PlayerPos = Player.transform.position;
            _newSave.PlayerRot = Player.transform.eulerAngles;
            string _json= JsonUtility.ToJson(_newSave);
            Debug.Log(_json);
            return _json;
            //File.WriteAllText(_path, _json, System.Text.Encoding.UTF8);
        }

        public void LoadSavedWorld(string _json)
        {
            WorldSaveData _save= JsonUtility.FromJson<WorldSaveData>(_json);
            //Player.gameObject.SetActive(false);
            foreach (var obj in _save.TerrainArray)
            {
                int _landX = Mathf.FloorToInt(obj.Pos.x);
                int _landY = Mathf.FloorToInt(obj.Pos.y);
                int _key = GetIslandKey(_landX, _landY);
                string _seed = obj.Seed;
                GameObject newLandRoot = Instantiate(mIslandPrefab);
                newLandRoot.name = "isLand_" + _landX.ToString() + "_" + _landY.ToString();
                newLandRoot.transform.position = new Vector3(200 * _landX, 0F, 200 * _landY);
                isLand newLandClass = new isLand();
                newLandClass._root = newLandRoot;
                newLandClass._info = newLandRoot.GetComponent<IslandInfo>();
                newLandClass._posX = _landX;
                newLandClass._posY = _landY;
                newLandClass._info.mInfo = newLandClass;
                newLandClass._modification.AddRange(obj.Modifications);
                newLandRoot.SetActive(true);
                newLandClass._seed = _seed;
                //mWorld.Add(_key, newLandClass);
                StartCoroutine(LoadIsLand(mWorld[_key], GetOffsetIsland(_landX - 1, _landY), GetOffsetIsland(_landX + 1, _landY), GetOffsetIsland(_landX, _landY + 1), GetOffsetIsland(_landX, _landY - 1),true));
            }
            Player.transform.eulerAngles = _save.PlayerRot;
            initialized = true;
            StartCoroutine(FullLoadBlockInSight(Sight));
            //Teleport(_save.PlayerPos);
        }

        public void GenerateRandomWorld()
        {
            ResetCurrentWorld();
            Player.transform.position = new Vector3(100F, 15F,100F);
            StartCoroutine(LoadCo());
        }

        public bool isTeleporting()
        {
            return teleporting;
        }
        public void Teleport(Vector3 _pos)
        {
            if (!teleporting) StartCoroutine(TeleportCo(_pos));
        }

        IEnumerator TeleportCo(Vector3 _pos)
        {
            teleporting = true;
            initialized = false;
            yield return 1;
            Player.SetActive(false);
            int _landX = Mathf.FloorToInt(_pos.x / 200F);
            int _landY = Mathf.FloorToInt(_pos.z / 200F);
            _pos.y = 5F;
            PlayerPos = _pos;
            PlayerCoordinate = new Vector2(Mathf.FloorToInt(PlayerPos.x / 200F), Mathf.FloorToInt(PlayerPos.z / 200F));
            yield return 1;
            if (!mWorld.ContainsKey(GetIslandKey(_landX, _landY))) GenerateIsland(_landX, _landY, true);
            yield return 1;
            if (!mWorld.ContainsKey(GetIslandKey(_landX-1, _landY))) GenerateIsland(_landX-1, _landY, true);
            yield return 1;
            if (!mWorld.ContainsKey(GetIslandKey(_landX+1, _landY))) GenerateIsland(_landX+1, _landY, true);
            yield return 1;
            if (!mWorld.ContainsKey(GetIslandKey(_landX, _landY-1))) GenerateIsland(_landX, _landY-1, true);
            yield return 1;
            if (!mWorld.ContainsKey(GetIslandKey(_landX, _landY+1))) GenerateIsland(_landX, _landY+1, true);
            yield return 1;
            if (!mWorld.ContainsKey(GetIslandKey(_landX-1, _landY-1))) GenerateIsland(_landX-1, _landY-1, true);
            yield return 1;
            if (!mWorld.ContainsKey(GetIslandKey(_landX-1, _landY+1))) GenerateIsland(_landX-1, _landY+1, true);
            yield return 1;
            if (!mWorld.ContainsKey(GetIslandKey(_landX+1, _landY-1))) GenerateIsland(_landX+1, _landY-1, true);
            yield return 1;
            if (!mWorld.ContainsKey(GetIslandKey(_landX+1, _landY+1))) GenerateIsland(_landX+1, _landY+1, true);
            yield return 1;
            _pos.y = GetSurfaceHeightByPosition(_pos) + 1F;
            PlayerPos = _pos;
            PlayerCoordinate = new Vector2(Mathf.FloorToInt(PlayerPos.x / 200F), Mathf.FloorToInt(PlayerPos.z / 200F));
            yield return 1;
            StartCoroutine(FullLoadBlockInSight(Sight));
            yield return new WaitForSeconds(1F);
            yield return new WaitForFixedUpdate();
            Player.transform.position = _pos;
            Player.SetActive(true);
            yield return 1;
            initialized = true;
            teleporting = false;
        }

        public void ResetCurrentWorld()
        {
            initialized = false;
            foreach (var key in mWorld.Keys)
            {
                UnloadIsland(key);
            }
            mWorld.Clear();
            mMap.Clear();
        }

        public void UnloadIsland(int key)
        {
            if (mWorld[key]._info != null) Destroy(mWorld[key]._info.gameObject);
            if (mMap.ContainsKey(key))
            {
                Destroy(mMap[key]);
                mMap.Remove(key);
            }
            mMinimapDeleteCallback(key);
        }
#endregion

        #region Internal Functions
        IEnumerator LoadCo()
        {
            initialized = false;
            CubeLoaded = 0;
            PlayerPos = Player.transform.position;
            PlayerCoordinate = new Vector2(Mathf.FloorToInt(PlayerPos.x / 200F), Mathf.FloorToInt(PlayerPos.z / 200F));
            int _x = Mathf.FloorToInt(PlayerCoordinate.x);
            int _y = Mathf.FloorToInt(PlayerCoordinate.y);
            _mapX = _x;
            _mapY = _y;
            yield return GenerateIsland(_x, _y,true);
            yield return 1;
            StartCoroutine(FullLoadBlockInSight(Sight));
            Player.gameObject.SetActive(true);
            yield return new WaitForSeconds(1F);
            initialized = true;
        }
        private string GetSeed(int _min, int _max, int _count)
        {
            string result = "";
            for (int i = 0; i < _count; i++)
            {
                result += Random.Range(_min, _max).ToString() + "_";
            }
            return result;
        }
       
        private void ReleaseIsland(isLand _land)
        {
            for (int x = 0; x < _land._Cubes.GetLength(0); x++)
            {
                for (int y = 0; y < _land._Cubes.GetLength(1); y++)
                {
                    if (_land._Cubes[x, y] != null && _land._Cubes[x, y]._instance.Count > 0)
                    {
                        foreach (BlockInstance obj in _land._Cubes[x, y]._instance)
                        {
                            obj.Unload();
                        }
                    }
                }
            }
            _land._Cubes = new BlockCube[200, 200];
            Destroy(_land._root);
            _land._root = null;
            _land._loaded = false;
        }
        private BlockCube GetPlayerRelativeBlock(int _x, int _y)
        {
            int _landX = Mathf.FloorToInt((PlayerPos.x + _x) / 200F);
            int _landY = Mathf.FloorToInt((PlayerPos.z + _y) / 200F);
            int _cubeX = Mathf.FloorToInt(PlayerPos.x + _x) - _landX * 200;
            int _cubeY = Mathf.FloorToInt(PlayerPos.z + _y) - _landY * 200;
            if (!mWorld.ContainsKey(GetIslandKey(_landX, _landY)))
            {
                GenerateIsland(_landX, _landY,true);
                StartCoroutine(FullLoadBlockInSight(Sight));
            }
            _mapX = _landX;
            _mapY = _landY;
            return mWorld[GetIslandKey(_landX, _landY)]._Cubes[_cubeX, _cubeY];
        }

        private void LoadAroundIsland(int _x, int _y)
        {
            int _landX = Mathf.FloorToInt((PlayerPos.x + _x) / 200F);
            int _landY = Mathf.FloorToInt((PlayerPos.z + _y) / 200F);
            if (!mWorld.ContainsKey(GetIslandKey(_landX, _landY)))
            {
                GenerateIsland(_landX, _landY, false);
            }
        }

        IEnumerator FullLoadBlockInSight(int _size)
        {
            for (int _y = -_size; _y <= _size; _y++)
            {
                for (int _x = -_size; _x <= _size; _x++)
                {
                    BlockCube _cube = GetPlayerRelativeBlock(_x, _y);
                    if (!mLoadedCube.Contains(_cube))
                    {
                        _cube.Load();
                        mLoadedCube.Add(_cube);
                    }
                }
                yield return 1;
            }
        }

        private bool isBlockInSight(BlockCube _block, int _sight, int _px, int _py)
        {
            int _cx = (_block._parentLand._posX * 200 + _block._localX - _px);
            int _cy = (_block._parentLand._posY * 200 + _block._localZ - _py);
            return ( _cx <= _sight && -_cx<=_sight && _cy <= _sight &&  -_cy<=_sight);
        }
        private void LoadCubesAround(int _size, int _border)
        {
            int loadedBlock = 0;
            int _px = Mathf.FloorToInt(PlayerPos.x);
            int _py = Mathf.FloorToInt(PlayerPos.z);

            if (PlayerPos.x % 200 < 80) LoadAroundIsland(-80, 0);
            if (PlayerPos.x % 200 > 120) LoadAroundIsland(80, 0);
            if (PlayerPos.z % 200 < 80) LoadAroundIsland(0, -80);
            if (PlayerPos.z % 200 > 120) LoadAroundIsland(0, 80);
            if (PlayerPos.x % 200 < 80 && PlayerPos.z % 200 < 80) LoadAroundIsland(-80, -80);
            if (PlayerPos.x % 200 > 120 && PlayerPos.z % 200 < 80) LoadAroundIsland(80, -80);
            if (PlayerPos.x % 200 < 80 && PlayerPos.z % 200 > 120) LoadAroundIsland(-80, 80);
            if (PlayerPos.x % 200 > 120 && PlayerPos.z % 200 > 120) LoadAroundIsland(80, 80);

            for (int i = 0; i < mLoadedCube.Count; i++)
            {
                if (!isBlockInSight(mLoadedCube[i], Sight, _px, _py))
                {
                    mLoadedCube[i].Unload();
                    mLoadedCube.RemoveAt(i);
                }
            }
            for (int _y = -_size; _y <= _size; _y++)
            {
                for (int _x = -_size; _x <= _size; _x++)
                {
                    if (loadedBlock >= LoadBlockPerFrame) return;
                    if (_x < -_size + _border || _x > _size - _border || _y < -_size + _border || _y > _size - _border)
                    {
                        BlockCube _cube = GetPlayerRelativeBlock(_x, _y);
                        if(!_cube.Loaded)
                        {
                            _cube.Load();
                            mLoadedCube.Add(_cube);
                            loadedBlock++;
                        }
                    }
                }
            }

        }

        IEnumerator LoadIslandMapTextures(isLand _land)
        {
            bool _mapAdded = false;
            int _key = GetIslandKey(_land._posX, _land._posY);
            if (!mMap.ContainsKey(_key))
            {
                mMap.Add(_key, new Texture2D(200, 200, TextureFormat.RGBA32, false, false));
                _mapAdded = true;
            }

            Texture2D _heightmap = new Texture2D(200, 200, TextureFormat.RGB24, false, true);
            for (int x = 0; x < 200; x++)
            {
                for (int y = 0; y < 200; y++)
                {
                    mMap[_key].SetPixel(x, y,
                     (_land._Cubes[x, y].GetTopInstance()._localPos.y > 2F || _land._Cubes[x, y]._isMountain) ? _land._Cubes[x, y].GetTopInstance().GetSideColor() : _land._Cubes[x, y].GetTopInstance().GetTopColor());
                    float _height = _land._Cubes[x, y].GetTopInstance()._localPos.y + 1F;
                    _heightmap.SetPixel(x, y, new Color(Mathf.Clamp(_height, 0F, 5F) / 5F, Mathf.Clamp(_height - 5F, 0F, 5F) / 5F, Mathf.Clamp(_height - 10F, 0F, 5F) / 5F, 1F));
                }
                yield return 1;
            }

            yield return 1;
            mMap[_key].Apply();
            yield return 1;
            _heightmap.Apply();

            _land._info.SetFakeGroundTexture(mMap[_key], _heightmap);
            if (mMinimapCreateCallback != null && _mapAdded) mMinimapCreateCallback(_land._posX * 200, _land._posY * 200,_key, mMap[_key]);
        }

        private isLand GenerateIsland(int _x, int _y,bool _fastMode)
        {
            GameObject newLandRoot = Instantiate(mIslandPrefab);
            newLandRoot.name = "isLand_" + _x.ToString() + "_" + _y.ToString();
            newLandRoot.transform.position = new Vector3(200 * _x, 0F, 200 * _y);
            isLand newLandClass = new isLand();
            newLandClass._root = newLandRoot;
            newLandClass._info = newLandRoot.GetComponent<IslandInfo>();
            newLandClass._posX = _x;
            newLandClass._posY = _y;
            newLandClass._info.mInfo = newLandClass;
            newLandRoot.SetActive(true);

            int _key = GetIslandKey(_x, _y);
            if (mWorld.ContainsKey(_key))
            {
                newLandClass._seed = mWorld[_key]._seed;
                if (mWorld[_key]._root != null)
                {
                    newLandClass._root = mWorld[_key]._root;
                    Destroy(newLandRoot);
                }
                mWorld[_key] = newLandClass;
            }
            else
            {
                Random.InitState(System.DateTime.Now.Millisecond + System.DateTime.Now.Minute * 1000 + System.DateTime.Now.Second * 100);
                newLandClass._seed = GetSeed(0, 800, 8) + Random.Range(1,BlockPool.instance.mTotalStyle+1);
                newLandClass._seed = "0_0_0_0_0_0_0_0_1";
                mWorld.Add(_key, newLandClass);
            }
            if (!mWorld[_key]._loaded)
            {
               StartCoroutine(LoadIsLand(mWorld[_key], GetOffsetIsland(_x - 1, _y), GetOffsetIsland(_x + 1, _y), GetOffsetIsland(_x, _y + 1), GetOffsetIsland(_x, _y - 1),_fastMode));
            }
            return newLandClass;
        }

        private isLand GetOffsetIsland(int _x, int _y)
        {
            int _key = GetIslandKey(_x, _y);
            if (mWorld.ContainsKey(_key))
            {
                if (mWorld[_key]._loaded && mWorld[_key]._root != null) return mWorld[_key];
            }
            return null;
        }

        IEnumerator LoadIsLand(isLand _land, isLand _left, isLand _right, isLand _top, isLand _bottom,bool _fastMode)
        {
            string[] _seeds = _land._seed.Split('_');
            int[] _seedNum = new int[_seeds.Length];
            for (int i = 0; i < _seedNum.Length; i++)
            {
                _seedNum[i] = int.Parse(_seeds[i]);
            }
            _land._Cubes = new BlockCube[200, 200];
            _land._style = (byte)(_seedNum[8]-1);

            for (int x = 0; x < 200; x++)
            {
                for (int y = 0; y < 200; y++)
                {
                    _land._Cubes[x, y] = new BlockCube();
                    _land._Cubes[x, y]._parentLand = _land;
                    BlockPool.BlockType _type = BlockPool.BlockType.Grass;
                    Color _color1 = mTerrainMaskPixels[_seedNum[0] + x, _seedNum[1] + y];
                    Color _color2 = mTerrainMaskPixels[_seedNum[2] + x, _seedNum[3] + y];
                    Color _color3 = mTerrainMaskPixels[_seedNum[4] + x, _seedNum[5] + y];
                    Color _color4 = mTerrainMaskPixels[_seedNum[6] + x, _seedNum[7] + y];
                    float _rndMuti = mTerrainMask2Pixels[_seedNum[6] + x, _seedNum[7] + y].a;
                    int _rnd = Mathf.FloorToInt((x * 54 + y * 76) * _rndMuti) % 100;
                    int _style = _land._style;
                    if (mUseWorldMap)
                    {
                        int _halfWidth = Mathf.FloorToInt(mWorldMap.width / 2F);
                        int _px = Mathf.Clamp(Mathf.RoundToInt((_land._posX * 200 + x)*1F /mWorldMapScale),-_halfWidth, _halfWidth);
                        int _py = Mathf.Clamp(Mathf.RoundToInt((_land._posY * 200 + y)*1F /mWorldMapScale),-_halfWidth, _halfWidth);
                        Color _worldColor = mWorldMapPixels[_halfWidth + _px, _halfWidth+ _py];
                        if (_rnd < _worldColor.r * 100)
                        {
                            _style = 0;
                        }
                        else if (_rnd < _worldColor.g * 100 && BlockPool.instance.mTotalStyle >= 1)
                        {
                            _style = 1;
                        }
                        else if (_rnd < _worldColor.b * 100 && BlockPool.instance.mTotalStyle >= 2)
                        {
                            _style = 2;
                        }
                        else if (_rnd < _worldColor.a * 100 && BlockPool.instance.mTotalStyle >= 3)
                        {
                            _style = 3;
                        }
                        else if (_worldColor.a > 0F && BlockPool.instance.mTotalStyle >= 3)
                        {
                            _style = 3;
                        }
                        else if (_worldColor.b > 0F && BlockPool.instance.mTotalStyle >= 2)
                        {
                            _style = 2;
                        }
                        else if (_worldColor.g > 0F && BlockPool.instance.mTotalStyle >= 1)
                        {
                            _style = 1;
                        }
                        else
                        {
                            _style = 0;
                        }
                    }

                    if (_color2.r < 0.5F)
                    {
                        _type = BlockPool.BlockType.Stone;
                    }
                    else
                    {
                        if (_color3.g < 0.5F)
                        {
                            if (mSettings[_style].WithLava)
                            {
                                _type = BlockPool.BlockType.Lava;
                            }
                            else
                            {
                                _type = BlockPool.BlockType.Mud;
                            }
                        }
                        else
                        {
                            if (mSettings[_style].WithGrass)
                            {
                                _type = BlockPool.BlockType.Grass;
                            }
                            else
                            {
                                _type = BlockPool.BlockType.Stone;
                            }
                        }
                    }

                    
                    int _level = Mathf.Clamp(Mathf.FloorToInt(_color4.a * 7F) - 1, mSettings[_style].WithWater ? -1 : 0, 4);
                    bool isMoutain = mTerrainMask2Pixels[_seedNum[0] + x, _seedNum[1] + y].r > 0.5F;
                    float _mixMuti = _color1.a;
                    if (x <= 5 && _left != null)
                    {
                        if (_left._loaded)
                        {
                            float _lerp = Mathf.Clamp(1F - x * 0.2F + _mixMuti, 0F, 1F);
                            _level = Mathf.FloorToInt(Mathf.Lerp(_level * 1F, _left._Cubes[199, y]._level * 1F, _lerp));
                            if (_mixMuti * 0.5F + _lerp * 0.5F > 0.5F)
                            {
                                isMoutain = _left._Cubes[199, y]._isMountain;
                                _type = _left._Cubes[199, y]._type;
                            }
                        }
                    }

                    if (x >= 195 && _right != null)
                    {
                        if (_right._loaded)
                        {
                            float _lerp = Mathf.Clamp((x - 195) * 0.2F + _mixMuti, 0F, 1F);
                            _level = Mathf.FloorToInt(Mathf.Lerp(_level * 1F, _right._Cubes[0, y]._level * 1F, _lerp));
                            if (_mixMuti * 0.5F + _lerp * 0.5F > 0.5F)
                            {
                                isMoutain = _right._Cubes[0, y]._isMountain;
                                _type = _right._Cubes[0, y]._type;
                            }
                        }
                    }

                    if (y <= 5 && _bottom != null)
                    {
                        if (_bottom._loaded)
                        {
                            float _lerp = Mathf.Clamp(1F - y * 0.2F + _mixMuti, 0F, 1F);
                            _level = Mathf.FloorToInt(Mathf.Lerp(_level * 1F, _bottom._Cubes[x, 199]._level * 1F, _lerp));
                            if (_mixMuti * 0.5F + _lerp * 0.5F > 0.5F)
                            {
                                isMoutain = _bottom._Cubes[x, 199]._isMountain;
                                _type = _bottom._Cubes[x, 199]._type;
                            }
                        }
                    }

                    if (y >= 195 && _top != null)
                    {
                        if (_top._loaded)
                        {
                            float _lerp = Mathf.Clamp((y - 195) * 0.2F + _mixMuti, 0F, 1F);
                            _level = Mathf.FloorToInt(Mathf.Lerp(_level * 1F, _top._Cubes[x, 0]._level * 1F, _lerp));
                            if (_mixMuti * 0.5F + _lerp * 0.5F > 0.5F)
                            {
                                isMoutain = _top._Cubes[x, 0]._isMountain;
                                _type = _top._Cubes[x, 0]._type;
                            }
                        }
                    }

                    if (_level == -1) _type = BlockPool.BlockType.Grass;
                    bool isBorder = x >= 194 || x <= 6 || y >= 194 || y <= 6;
                    _land._Cubes[x, y]._localX = (byte)x;
                    _land._Cubes[x, y]._localZ = (byte)y;
                    _land._Cubes[x, y]._level = (short)_level;
                    _land._Cubes[x, y]._type = _type;
                    _land._Cubes[x, y]._isMountain = isMoutain;

                    int _angle = (x + y) % 4;
                    int _angleProp = (x * 2 + y * 6) % 4;
                    
                    int _rndProp = Mathf.FloorToInt((x * 56 + y * 68) * _rndMuti) % 100;

                    if (isMoutain)
                    {
                        _level = Mathf.Clamp(_level, 0, 10);
                        _land._Cubes[x, y]._localY = (byte)(_level + mSettings[_style].MoutainHeight);
                    }
                    else
                    {
                        _land._Cubes[x, y]._localY = (byte)(_level + 1);
                    }

                    int _setLevel = _level;
                    BlockPool.BlockType _newType = _type;
                    for (int i = 0; i <= _land._Cubes[x, y]._localY; i++)
                    {
                        if (isMoutain) _newType = (_type == BlockPool.BlockType.Grass && i < mSettings[_style].MoutainHeight) ? BlockPool.BlockType.Stone : _type;
                        if (isBorder) _newType = (i > 1 && _type == BlockPool.BlockType.Grass) ? BlockPool.BlockType.Stone : _type;
                        _setLevel = Mathf.Min(4, i - 1);
                        if (isMoutain)
                        {
                            if (i < _land._Cubes[x, y]._localY && i > 2)
                            {
                                _setLevel = 2;
                            }
                            else if(i>= _land._Cubes[x, y]._localY)
                            {
                                _setLevel = Mathf.Min(4, _level+ mSettings[_style].MoutainHeight);
                            }
                        }
                        if (_setLevel == 4 && !mSettings[_style].WithSnow && _newType != BlockPool.BlockType.Lava) _setLevel = 3;
                        if (_setLevel == 3 && !mSettings[_style].WithGrass && (_newType == BlockPool.BlockType.Grass || _newType == BlockPool.BlockType.Mud)) _newType = BlockPool.BlockType.Stone;
                        _land._Cubes[x, y]._instance.Add(BlockInstance.CreateInstance(_newType, _setLevel, _land._root.transform,
                        _angle, _rnd, new Vector3(x, i - 1, y), (byte)i));
                    }

                    if (mTerrainMask2Pixels[_seedNum[2] + x, _seedNum[3] + y].g > 0.5F)
                    {
                        _land._Cubes[x, y]._groundProp = PropInstance.CreateInstance(_newType, _setLevel, _angleProp, _style, _rndProp,
                           _land._Cubes[x, y]._localX, _land._Cubes[x, y]._localY, _land._Cubes[x, y]._localZ, _land);
                    }
                    else
                    {
                        _land._Cubes[x, y]._groundProp = null;
                    }
                    _land._Cubes[x, y].GetTopNativeInstance()._forceToShow = true;

                }
                if (!_fastMode) yield return 1;
            }
            for (int i=0;i< _land._modification.Count;i++) {
                if (_land._Cubes[_land._modification[i]._posX, _land._modification[i]._posZ]._groundProp != null) {
                    _land._Cubes[_land._modification[i]._posX, _land._modification[i]._posZ]._groundProp.Unload();
                    _land._Cubes[_land._modification[i]._posX, _land._modification[i]._posZ]._groundProp = null;
                }
                if (_land._modification[i]._mode==0)
                {
                    //_land._Cubes[_land._modification[i]._posX, _land._modification[i]._posZ].GetTopInstance()._forceToShow = true;
                    _land._Cubes[_land._modification[i]._posX, _land._modification[i]._posZ].BuildNewInstance(
                        BlockInstance.CreateInstance((BlockPool.BlockType)_land._modification[i]._addType, _land._modification[i]._addLevel, _land._root.transform,
                            _land._modification[i]._addAngle, Random.Range(0, 100), new Vector3(_land._modification[i]._posX, _land._modification[i]._posY, _land._modification[i]._posZ), (byte)_land._modification[i]._y, _land._modification[i]._addUID,true)
                             , false,false);
                }
                else 
                {
                    _land._Cubes[_land._modification[i]._posX, _land._modification[i]._posZ].RemoveInstance((byte)_land._modification[i]._y, false);
                }
               
            }
            _land._root.SetActive(true);
            _land._loaded = true;
            yield return 1;
            StartCoroutine(LoadIslandMapTextures(_land));
        }


        #endregion

        #region Useful Public Datas
        public bool isInitialized()
        {
            return initialized;
        }

        public static int GetIslandKey(int _x, int _y)
        {
            return _x*1000+_y;
        }

        public float GetRunningSpeedByPosition(Vector3 _pos)
        {
            if (_pos.y < -0.1F)
                return MoveSpeedInWater;
            else
            {
                BlockInfo _info = GetCubeByPosition(_pos);
                return _info!=null? _info._walkableSpeed * 0.01F:1F;
            }
        }

        public isLand GetIslandByPosition(Vector3 _pos)
        {
            int _landX = Mathf.FloorToInt(_pos.x / 200F);
            int _landY = Mathf.FloorToInt(_pos.z / 200F);
            if (!mWorld.ContainsKey(GetIslandKey(_landX, _landY))) return null;
            if (!mWorld[GetIslandKey(_landX, _landY)]._loaded) return null;
            return mWorld[GetIslandKey(_landX, _landY)];
        }

        public BlockInfo GetCubeByPosition(Vector3 _pos)
        {
            int _landX = Mathf.FloorToInt(_pos.x / 200F);
            int _landY = Mathf.FloorToInt(_pos.z / 200F);
            if (!mWorld.ContainsKey(GetIslandKey(_landX, _landY))) return null;
            if (!mWorld[GetIslandKey(_landX, _landY)]._loaded) return null;
            Vector3 _local = mWorld[GetIslandKey(_landX, _landY)]._root.transform.InverseTransformPoint(_pos + new Vector3(0.5F, 0F, 0.5F));
            if (_local.x < 200F && _local.x > 0F && _local.z < 200F && _local.z > 0F)
            {
                if (mWorld[GetIslandKey(_landX, _landY)]._Cubes[Mathf.FloorToInt(_local.x), Mathf.FloorToInt(_local.z)].GetTopInstance() != null)
                {
                    if (mWorld[GetIslandKey(_landX, _landY)]._Cubes[Mathf.FloorToInt(_local.x), Mathf.FloorToInt(_local.z)].GetTopInstance()._info == null) return null;
                    return mWorld[GetIslandKey(_landX, _landY)]._Cubes[Mathf.FloorToInt(_local.x), Mathf.FloorToInt(_local.z)].GetTopInstance()._info;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public float GetFrictionByPosition(Vector3 _pos)
        {
            if (_pos.y < -0.1F)
                return 1F;
            else
            {
                BlockInfo _info = GetCubeByPosition(_pos);
                return _info!=null?_info._friction * 0.01F:1F;
            }
        }
        public bool GetSurfaceWalkableByPosition(Vector3 _pos)
        {
            int _landX = Mathf.FloorToInt(_pos.x / 200F);
            int _landY = Mathf.FloorToInt(_pos.z / 200F);
            int _key = GetIslandKey(_landX, _landY);
            if (!mWorld.ContainsKey(_key)) return false;
            if (!mWorld[_key]._loaded) return false;
            Vector3 _local = mWorld[_key]._root.transform.InverseTransformPoint(_pos);
            int _x = Mathf.RoundToInt(Mathf.Clamp(_local.x, 0, 199));
            int _y = Mathf.RoundToInt(Mathf.Clamp(_local.z, 0, 199));
            if (_x < 0 || _x >= 200 || _y < 0 || _y >= 200) return false;
            return mWorld[_key]._Cubes[_x, _y]._walkable;
        }

        public float GetSurfaceHeightByPosition(Vector3 _pos)
        {
            int _landX = Mathf.FloorToInt(_pos.x / 200F);
            int _landY = Mathf.FloorToInt(_pos.z / 200F);
            int _key = GetIslandKey(_landX, _landY);
            if (!mWorld.ContainsKey(_key)) return 0.5F;
            if (!mWorld[_key]._loaded) return 0.5F;
            Vector3 _local = mWorld[_key]._root.transform.InverseTransformPoint(_pos);
            int _x = Mathf.RoundToInt(Mathf.Clamp(_local.x,0,199));
            int _y = Mathf.RoundToInt(Mathf.Clamp(_local.z, 0, 199));
            if (mWorld[_key]._Cubes[_x, _y].GetTopInstance() == null) return 0.5F;
            return mWorld[_key]._Cubes[_x, _y].GetTopInstance()._localPos.y + 0.5F;
        }

        #endregion
    }
}
