using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SoftKitty.PCW.Demo
{
    [DefaultExecutionOrder(150)]
    public class BuildControl : MonoBehaviour
    {
        public static BuildControl Instance { get; private set; }
        public enum PlayModes
        {
            VitualCube,
            TapOrHold
        }
        public static PlayModes PlayMode = PlayModes.TapOrHold;
        public static bool BuildMode = false;
        public static bool DeleteMode = false;
        public LayerMask GroundLayer;
        public Transform RayPoint;
        public Transform AngleRoot;
        Camera Cam;
        Transform BuildCube;
        Transform DeleteCube;
        RaycastHit GroundHit;
        BlockCube HoverCube;
        byte HoverInstanceY;
        bool BuildMouseDown = false;
        bool Deleted = false;
        float BuildHeight = 0F;
        int Height=0;
        float BuildWait = 0F;
        float _buildTouchTime = 0F;


        private void Awake()
        {
            Instance = this;
        }
        void Start()
        {
            Cam = GetComponentInChildren<Camera>(true);
            BuildCube = Instantiate(Resources.Load<GameObject>("ProceduralCubeWorld/BuildCube")).transform;
            DeleteCube = Instantiate(Resources.Load<GameObject>("ProceduralCubeWorld/DeleteCube")).transform;
            HidePreviewCubes();
        }

        
        void Update()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.F)) {
                BuildMode = !BuildMode;
                AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("ProceduralCubeWorld/"+(BuildMode? "MenuActive": "Deactive")), Camera.main.transform.position);
            }
            if (Input.GetKeyDown(KeyCode.Escape) && BuildMode)
            {
                BuildMode = false;
                AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("ProceduralCubeWorld/Deactive"), Camera.main.transform.position);
            }

            if (PlayMode == PlayModes.VitualCube)
            {
                VitualCubeUpdate();
            }
            else if (PlayMode == PlayModes.TapOrHold)
            {
                TapOrHoldUpdate();
            }
        }



        private void GetHoverBuildCube(Vector3 _pos)
        {
            if (Physics.Raycast(Cam.transform.position, (_pos-Cam.transform.position).normalized , out GroundHit, 20F, GroundLayer, QueryTriggerInteraction.Ignore))
            {
                if (GeBlocktInfo(GroundHit) != null)
                {
                    Vector3 _buildPos = GeBlocktInfo(GroundHit).transform.position + GroundHit.normal * 1F;
                    if (Physics.Raycast(_buildPos, Vector3.down, out GroundHit, 20F, GroundLayer, QueryTriggerInteraction.Ignore))
                    {
                        BlockInfo _info = GeBlocktInfo(GroundHit);
                        if (_info != null && _info._blockInstance != null && _info._blockInstance._blockCube != null)
                        {
                            HoverCube = _info._blockInstance._blockCube;
                            BuildHeight = HoverCube._parentLand._root.transform.InverseTransformPoint(_buildPos).y;
                        }
                    }
                }
            }

        }

        private void GetHoverBuildCube(Vector3 cam_pos, Vector3 _pos)
        {
            if (Physics.Raycast(cam_pos, (_pos - cam_pos).normalized, out GroundHit, 20F, GroundLayer, QueryTriggerInteraction.Ignore))
            {
                if (GeBlocktInfo(GroundHit) != null)
                {
                    Vector3 _buildPos = GeBlocktInfo(GroundHit).transform.position + GroundHit.normal * 1F;
                    if (Physics.Raycast(_buildPos, Vector3.down, out GroundHit, 20F, GroundLayer, QueryTriggerInteraction.Ignore))
                    {
                        BlockInfo _info = GeBlocktInfo(GroundHit);
                        if (_info != null && _info._blockInstance != null && _info._blockInstance._blockCube != null)
                        {
                            HoverCube = _info._blockInstance._blockCube;
                            BuildHeight = HoverCube._parentLand._root.transform.InverseTransformPoint(_buildPos).y;
                        }
                    }
                }
            }

        }

        private void GetHoverDeleteCube(Vector3 _pos)
        {
            if (Physics.Raycast(Cam.transform.position, (_pos - Cam.transform.position).normalized, out GroundHit, 20F, GroundLayer, QueryTriggerInteraction.Ignore))
            {
                BlockInfo _info = GeBlocktInfo(GroundHit);
                if (_info != null && _info._blockInstance != null && _info._blockInstance._blockCube != null)
                {
                    HoverCube = _info._blockInstance._blockCube;
                    HoverInstanceY = _info._blockInstance._localY;
                }

            }

        }

        private void GetHoverDeleteCube(Vector3 cam_pos, Vector3 _pos)
        {
            if (Physics.Raycast(cam_pos, (_pos - cam_pos).normalized, out GroundHit, 20F, GroundLayer, QueryTriggerInteraction.Ignore))
            {
                BlockInfo _info = GeBlocktInfo(GroundHit);
                if (_info != null && _info._blockInstance != null && _info._blockInstance._blockCube != null)
                {
                    HoverCube = _info._blockInstance._blockCube;
                    HoverInstanceY = _info._blockInstance._localY;
                }

            }

        }

        private BlockInfo GeBlocktInfo(RaycastHit _hit)
        {
            BlockInfo _info = null;

            if (_hit.collider.GetComponent<BlockInfo>())
            {
                _info = _hit.collider.GetComponent<BlockInfo>();
            }
            else if (_hit.collider.GetComponentInParent<BlockInfo>())
            {
                _info = _hit.collider.GetComponentInParent<BlockInfo>();
            }

            return _info;
        }
        private void TapOrHoldUpdate()
        {
            if (BuildMode)
            {

                if (Input.GetMouseButtonDown(0))
                {
                    BuildMouseDown = !EventSystem.current.IsPointerOverGameObject();
                    Deleted = false;
                }

                if (BuildMouseDown)
                {
                    if (Input.GetMouseButton(0))
                    {
                        _buildTouchTime += Time.deltaTime;
                        if (!EventSystem.current.IsPointerOverGameObject())
                        {
                            Vector3 touchedPos = Cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
                            GetHoverDeleteCube(touchedPos);
                            if(_buildTouchTime > 0.2F) UiManager.instance.ShowProgess(Mathf.Clamp01((_buildTouchTime-0.2F)/0.8F));
                            if (_buildTouchTime > 1F && HoverCube != null)
                            {
                                SendDeleteData(HoverInstanceY, true, touchedPos, Cam.transform.position);
                                //HoverCube.RemoveInstance(HoverInstanceY, true);
                                _buildTouchTime = 0F;
                                Deleted = true;
                            }
                        }
                        else
                        {
                            UiManager.instance.HideProgress();
                            Deleted = false;
                            BuildMouseDown = false;
                            _buildTouchTime = 0F;
                        }
                    }
                    else if (Input.GetMouseButtonUp(0))
                    {
                        if (!EventSystem.current.IsPointerOverGameObject() && _buildTouchTime < 0.5F && !Deleted)
                        {
                            Vector3 touchedPos = Cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
                            GetHoverBuildCube(touchedPos);
                            if (HoverCube != null)
                            {
                                Vector3 _pos = HoverCube.GetTopInstance()._info.transform.position;
                                _pos.y = BuildHeight;
                                //Instantiate(Resources.Load<GameObject>("ProceduralCubeWorld/Smoke"), _pos, Quaternion.identity);
                                SendBuildData(BlockInstance.CreateInstance((BlockPool.BlockType)UiManager.previewType, UiManager.previewHeight, HoverCube._parentLand._root.transform,
                                        GetAngle(), Random.Range(0, 100), new Vector3(HoverCube.GetTopInstance()._localPos.x, BuildHeight, HoverCube.GetTopInstance()._localPos.z),
                                        (byte)(Mathf.Round(BuildHeight + 1)), UiManager.previewUid), true, _pos, touchedPos, Cam.transform.position);
                                //HoverCube.BuildNewInstance(BlockInstance.CreateInstance((BlockPool.BlockType)UiManager.previewType, UiManager.previewHeight, HoverCube._parentLand._root.transform,
                                //    GetAngle(), Random.Range(0, 100), new Vector3(HoverCube.GetTopInstance()._localPos.x, BuildHeight, HoverCube.GetTopInstance()._localPos.z),
                                //      (byte)(Mathf.Round(BuildHeight + 1)), UiManager.previewUid), true);

                            }
                        }
                        _buildTouchTime = 0F;
                    }


                }
                if (Input.GetMouseButtonUp(0))
                {
                    UiManager.instance.HideProgress();
                    BuildMouseDown = false;
                }
            }
            else
            {
                UiManager.instance.HideProgress();
                BuildMouseDown = false;
                _buildTouchTime = 0F;
            }
        }

        public void SendBuildData(BlockInstance blockInstance, bool _load, Vector3 _pos, Vector3 touch_pos, Vector3 cam_pos, bool _modify_data = true)
        {
            BlockGenerator.instance.Player.GetComponent<BuildSync>().SendBuildData(blockInstance, _load, _pos, touch_pos, cam_pos, _modify_data);
        }

        public void ResponseBuildData(BlockInstance blockInstance, bool _load, Vector3 _pos, Vector3 touch_pos, Vector3 cam_pos, bool _modify_data = true)
        {
            GetHoverBuildCube(cam_pos, touch_pos);
            AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("ProceduralCubeWorld/Build"), Camera.main.transform.position);
            Instantiate(Resources.Load<GameObject>("ProceduralCubeWorld/Smoke"), _pos, Quaternion.identity);
            HoverCube.BuildNewInstance(blockInstance, _load, _modify_data);
        }

        public void SendDeleteData(byte pos, bool status, Vector3 touch_pos, Vector3 cam_pos)
        {
            BlockGenerator.instance.Player.GetComponent<BuildSync>().SendDeleteData(pos, status, touch_pos, cam_pos);
        }

        public void ResponseDeleteData(byte pos, bool status, Vector3 touch_pos, Vector3 cam_pos)
        {
            GetHoverDeleteCube(cam_pos, touch_pos);
            AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("ProceduralCubeWorld/Build"), Camera.main.transform.position);
            Instantiate(Resources.Load<GameObject>("ProceduralCubeWorld/Smoke"), HoverCube.GetTopInstance()._info.transform.position, Quaternion.identity);
            HoverCube.RemoveInstance(pos, status);

        }



        private void VitualCubeUpdate()
        {
            if (Input.GetKeyDown(KeyCode.R) && BuildMode)
            {
                DeleteMode = !DeleteMode;
                AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("ProceduralCubeWorld/MenuActive"), Camera.main.transform.position);
            }
            BuildWait = Mathf.MoveTowards(BuildWait, 0F, Time.deltaTime);
            if (Input.GetMouseButton(0) && BuildMode && HoverCube != null && BuildWait <= 0F)
            {
                AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("ProceduralCubeWorld/Build"), Camera.main.transform.position);
                BuildWait = 0.3F;
                if (DeleteMode)
                {
                    Instantiate(Resources.Load<GameObject>("ProceduralCubeWorld/Smoke"), DeleteCube.transform.position, Quaternion.identity);
                    HoverCube.RemoveInstance((byte)(HoverCube.GetTopInstance()._localY + Height), true);
                    Height = 0;
                }
                else
                {
                    Instantiate(Resources.Load<GameObject>("ProceduralCubeWorld/Smoke"), BuildCube.transform.position, Quaternion.identity);
                    HoverCube.BuildNewInstance(BlockInstance.CreateInstance((BlockPool.BlockType)UiManager.previewType, UiManager.previewHeight, HoverCube._parentLand._root.transform,
                            GetAngle(), Random.Range(0, 100), new Vector3(HoverCube.GetTopInstance()._localPos.x, HoverCube.GetTopInstance()._localPos.y + 1F + Height, HoverCube.GetTopInstance()._localPos.z),
                            (byte)(HoverCube.GetTopInstance()._localY + 1 + Height), UiManager.previewUid, true), true);
                    Height = 0;
                }
            }
            if (BuildMode)
            {
                if (!DeleteMode)
                {
                    if (Input.GetAxis("Mouse ScrollWheel") < 0F)
                    {
                        for (int i = 1; i <= 5; i++)
                        {
                            if (isAnyCubeConnected(BuildCube.transform.position - Vector3.up * i))
                            {
                                Height = Mathf.Clamp(Height - i, -5, 5);
                                break;
                            }
                        }
                    }
                    else if (Input.GetAxis("Mouse ScrollWheel") > 0F)
                    {
                        for (int i = 1; i <= 5; i++)
                        {
                            if (isAnyCubeConnected(BuildCube.transform.position + Vector3.up * i))
                            {
                                Height = Mathf.Clamp(Height + i, -5, 5);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (Input.GetAxis("Mouse ScrollWheel") < 0F)
                    {
                        for (int i = 1; i <= 5; i++)
                        {
                            if (isAnyCubeOverlap(DeleteCube.transform.position - Vector3.up * i))
                            {
                                Height = Mathf.Clamp(Height - i, -5, 5);
                                break;
                            }
                        }
                    }
                    else if (Input.GetAxis("Mouse ScrollWheel") > 0F)
                    {
                        for (int i = 1; i <= 5; i++)
                        {
                            if (isAnyCubeOverlap(DeleteCube.transform.position + Vector3.up * i))
                            {
                                Height = Mathf.Clamp(Height + i, -5, 5);
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                Height = 0;
            }
            AngleRoot.localEulerAngles = new Vector3(40F, 0F, 0F);
        }



        private bool isAnyCubeOverlap(Vector3 _pos)
        {
            return Physics.OverlapBox(_pos, Vector3.one * 0.25F, Quaternion.identity, GroundLayer, QueryTriggerInteraction.Ignore).Length > 0;
        }

        private bool isAnyCubeConnected(Vector3 _pos)
        {
            if (_pos.y<0F) {
                return false;
            }
            else if (Physics.Raycast(_pos, Vector3.forward, 1F, GroundLayer, QueryTriggerInteraction.Ignore))
            {
                return !isAnyCubeOverlap(_pos);
            }
            else if (Physics.Raycast(_pos, Vector3.back, 1F, GroundLayer, QueryTriggerInteraction.Ignore))
            {
                return !isAnyCubeOverlap(_pos);
            }
            else if (Physics.Raycast(_pos, Vector3.left, 1F, GroundLayer, QueryTriggerInteraction.Ignore))
            {
                return !isAnyCubeOverlap(_pos);;
            }
            else if (Physics.Raycast(_pos, Vector3.right, 1F, GroundLayer, QueryTriggerInteraction.Ignore))
            {
                return !isAnyCubeOverlap(_pos);;
            }
            else if (Physics.Raycast(_pos, Vector3.up, 1F, GroundLayer, QueryTriggerInteraction.Ignore))
            {
                return !isAnyCubeOverlap(_pos);;
            }
            else if (Physics.Raycast(_pos, Vector3.down, 1F, GroundLayer, QueryTriggerInteraction.Ignore))
            {
                return !isAnyCubeOverlap(_pos);;
            }
            else
            {
                return false;
            }
        }

        private int GetAngle()
        {
            Vector3 _dir = RayPoint.forward;
            _dir.y = 0F;
            _dir.Normalize();
            if (_dir.z > 0.5F)
            {
                return 0;
            }
            else if (_dir.z < -0.5F)
            {
                return 2;
            }
            else if (_dir.x > 0.5F)
            {
                return 1;
            }
            else
            {
                return 3;
            }
        }

        private void LateUpdate()
        {
            if (BuildMode && PlayMode== PlayModes.VitualCube)
            {
                if (Physics.Raycast(RayPoint.transform.position, RayPoint.transform.forward, out GroundHit, 20F, GroundLayer,QueryTriggerInteraction.Ignore))
                {
                    BlockInfo _info=null;
                    
                    if (GroundHit.collider.GetComponent<BlockInfo>()) {
                        _info = GroundHit.collider.GetComponent<BlockInfo>();
                    } else if (GroundHit.collider.GetComponentInParent<BlockInfo>()) {
                        _info = GroundHit.collider.GetComponentInParent<BlockInfo>();
                    }

                    if (_info != null && _info._blockInstance != null && _info._blockInstance._blockCube != null)
                    {
                        HoverCube = _info._blockInstance._blockCube;
                        _info = HoverCube.GetTopInstance()._info;
                    }
                    else
                    {
                        _info = null;
                    }

                  
                    if (_info != null )
                    {
                         
                        Vector3 _pos = _info.transform.position;
                        if (!DeleteMode)
                        {
                            _pos.y += 1F;
                            BuildCube.position = _pos+Height*Vector3.up;
                        }
                        else
                        {
                            DeleteCube.position = _pos + Height * Vector3.up;
                        }
                        
                        BuildCube.gameObject.SetActive(!DeleteMode);
                        DeleteCube.gameObject.SetActive(DeleteMode);
                    }
                    else
                    {
                        HidePreviewCubes();
                    }
                }
                else
                {
                    HoverCube = null;
                    HidePreviewCubes();
                }
            }
            else
            {
                HidePreviewCubes();
            }
        }

        void HidePreviewCubes()
        {
            if (BuildCube.gameObject.activeSelf) BuildCube.gameObject.SetActive(false);
            if (DeleteCube.gameObject.activeSelf) DeleteCube.gameObject.SetActive(false);
        }

    }
}
