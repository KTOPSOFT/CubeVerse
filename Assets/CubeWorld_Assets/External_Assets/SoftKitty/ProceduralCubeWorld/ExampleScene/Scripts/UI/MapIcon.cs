using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace SoftKitty.PCW.Demo
{
    public class MapIcon : MonoBehaviour
    {
        public RawImage icon;
        public Transform zoomRoot;
        public Transform LeftBottom;
        public Transform TopRight;
        private bool StayInsideTheView = true;
        private bool KeepSameSizeWhenZoom = true;
        private Vector3 pos;
        private Vector3 LeftBottomPos;
        private Vector3 TopRightPos;
        public void Init(Vector3 _pos,Texture _tex,int _pixelSize,bool _stayInsideTheView,bool _keepSameSizeWhenZoom)
        {
            pos = _pos;
            icon.texture = _tex;
            icon.rectTransform.sizeDelta = new Vector2(_pixelSize, _pixelSize);
            StayInsideTheView = _stayInsideTheView;
            KeepSameSizeWhenZoom = _keepSameSizeWhenZoom;
        }

        void Update()
        {
            if(KeepSameSizeWhenZoom)transform.localScale = Vector3.one / zoomRoot.localScale.x;
            if (StayInsideTheView)
            {
                LeftBottomPos = transform.parent.InverseTransformPoint(LeftBottom.position);
                TopRightPos = transform.parent.InverseTransformPoint(TopRight.position);
                transform.localPosition = new Vector3(
                    Mathf.Clamp(pos.x, LeftBottomPos.x, TopRightPos.x),
                    Mathf.Clamp(pos.y, LeftBottomPos.y, TopRightPos.y),
                    0F
                    );
            }
        }

        public void UpdatePos(Vector3 _pos)
        {
            pos = MinimapUI.ConvertPos(_pos);
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}
