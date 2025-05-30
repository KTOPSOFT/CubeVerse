using System.Collections;
using System.Collections.Generic;
using MalbersAnimations.Conditions;
using UnityEngine;

namespace SoftKitty.PCW
{
    public class IslandInfo : MonoBehaviour
    {
        #region variables
        public GameObject mModelRoot;
        public MeshRenderer mWater;
        public Renderer FakeGroundModel;
        public isLand mInfo;
        bool mVisible = true;
        #endregion

        #region MonoBehaviour
     
        private void Update()
        {
            if (Time.frameCount % 60 == 0)
            {
                mVisible = (BlockGenerator.instance.Player.transform.position.x < transform.position.x + 350F
                    && BlockGenerator.instance.Player.transform.position.x > transform.position.x - 150F
                     && BlockGenerator.instance.Player.transform.position.z > transform.position.z - 150F
                    && BlockGenerator.instance.Player.transform.position.z < transform.position.z + 350F);
            }

            if (mModelRoot.activeSelf != mVisible) mModelRoot.SetActive(mVisible);
            if (mVisible)
            {
                Vector3 pos = (BlockGenerator.instance.Player.transform.position - mWater.transform.position) * 0.0495F;
                //mWater.material.SetVector("_PlayerPos",new Vector4(pos.x, pos.z, 
                //    Mathf.Clamp01(-(-0.5F+BlockGenerator.instance.Player.transform.position.y)* BlockGenerator.instance.Player.GetComponent<Demo.DemoPlayerControl>().GetSpeed()*0.5F) 
                //    ,0F));
                mWater.material.SetVector("_PlayerPos", new Vector4(pos.x, pos.z,
                    Mathf.Clamp01(-(-0.5F + BlockGenerator.instance.Player.transform.position.y) * 2.0f * 0.5F)
                    , 0F));
            }
        }

        public void SetFakeGroundTexture(Texture _color,Texture _height)
        {
            _color.filterMode = FilterMode.Point;
            _height.filterMode = FilterMode.Point;
            if (FakeGroundModel)
            {
                FakeGroundModel.material.SetTexture("_ColorTex", _color);
                FakeGroundModel.material.SetTexture("_HeightTex", _height);
            }
        }
        #endregion

    }
}
