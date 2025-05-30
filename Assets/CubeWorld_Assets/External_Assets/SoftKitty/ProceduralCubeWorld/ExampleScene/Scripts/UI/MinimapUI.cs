using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace SoftKitty.PCW.Demo
{
    public class MinimapUI : MonoBehaviour
    {
        public static MinimapUI instance;
        public RawImage mMap;
        public MapIcon mIcon;
        public RectTransform mZoomRoot;
        public RectTransform mMapRoot;
        public RectTransform mPlayer;
        public UiHover mHover;
        private Dictionary<int, GameObject> islandMaps = new Dictionary<int, GameObject>();
        private float _zoom = 1.5F;

        void Start()
        {
            instance = this;
            BlockGenerator.instance.RegisterMinimapCreateCallback(CreateNewIslandMap);//Register callback
            BlockGenerator.instance.RegisterMinimapDeleteCallback(DeleteIslandMap);//Register callback
            mZoomRoot.transform.localScale = Vector3.one * _zoom;
        }

        
        void Update()
        {
            if (BlockGenerator.instance.Player == null) return;
            mMapRoot.transform.localPosition = new Vector3(-BlockGenerator.instance.Player.transform.position.x, -BlockGenerator.instance.Player.transform.position.z, 0F);
            mPlayer.transform.localPosition = new Vector3(BlockGenerator.instance.Player.transform.position.x, BlockGenerator.instance.Player.transform.position.z, 0F);
            mPlayer.localEulerAngles = new Vector3(0F, 0F, -BlockGenerator.instance.Player.transform.eulerAngles.y);
            mPlayer.transform.localScale = Vector3.one / mZoomRoot.transform.localScale.x;
            if (mHover.isHover) {
                if (Input.GetAxis("Mouse ScrollWheel")!=0F) {
                    Zoom(-Input.GetAxis("Mouse ScrollWheel")*2F);
                }
            }

        }
       
        public void CreateNewIslandMap(int _x, int _y, int _key, Texture2D _tex)
        {
            if (!islandMaps.ContainsKey(_key))
            {
                GameObject newMap = Instantiate(mMap.gameObject, new Vector3(_x, _y, 0F), Quaternion.identity, mMap.transform.parent);
                newMap.transform.localPosition = new Vector3(_x, _y, 0F);
                newMap.GetComponent<RawImage>().texture = _tex;
                newMap.gameObject.SetActive(true);
                islandMaps.Add(_key, newMap);
            }
            else
            {
                islandMaps[_key].GetComponent<RawImage>().texture = _tex;
                islandMaps[_key].transform.localPosition = new Vector3(_x, _y, 0F);
                islandMaps[_key].SetActive(true);
            }
        }

        public MapIcon CreateIcon(Vector3 _pos, Texture _icon, int _pixelSize = 32, bool _stayInsideView = true, bool _keepSameSizeWhenZoom = true)
        {
            GameObject newIcon = Instantiate(mIcon.gameObject, new Vector3(_pos.x, _pos.z, 0F), Quaternion.identity, mIcon.transform.parent);
            newIcon.transform.localPosition = ConvertPos(_pos);
            newIcon.GetComponent<MapIcon>().Init(ConvertPos(_pos), _icon, _pixelSize, _stayInsideView, _keepSameSizeWhenZoom);
            newIcon.gameObject.SetActive(true);
            mPlayer.transform.SetAsLastSibling();
            return newIcon.GetComponent<MapIcon>();
        }

        public void DeleteIslandMap(int _key)
        {
            if (islandMaps.ContainsKey(_key))
            {
                Destroy(islandMaps[_key]);
                islandMaps.Remove(_key);
            }

        }

        public void Zoom(float _value)
        {
            _zoom -= _value * 0.2F;
            _zoom = Mathf.Clamp(_zoom, 1F, 2F);
            mZoomRoot.transform.localScale = Vector3.one * _zoom;
        }

        public static Vector3 ConvertPos(Vector3 _pos)
        {
            return new Vector3(_pos.x, _pos.z, 0F);
        }

        
    }
}
