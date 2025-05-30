using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SoftKitty.PCW.Demo
{
    public class DemoManager : MonoBehaviour
    {

        IEnumerator Start()
        {
            yield return 1;
            BlockGenerator.instance.GenerateRandomWorld();//Generate a random world for the demo
        }


    }
}
