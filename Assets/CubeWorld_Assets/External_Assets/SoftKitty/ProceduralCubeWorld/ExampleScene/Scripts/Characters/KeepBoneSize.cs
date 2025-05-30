using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SoftKitty.PCW
{
    public class KeepBoneSize : MonoBehaviour
    {
        public Vector3 Size = Vector3.one;

        void LateUpdate()
        {
            transform.localScale = Size;
        }
    }
}
