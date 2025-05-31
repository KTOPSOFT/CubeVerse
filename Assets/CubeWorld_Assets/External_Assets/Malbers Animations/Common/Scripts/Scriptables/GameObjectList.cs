using System;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Scriptables
{
    ///<summary>  Prefab Scriptable Variable. Based on the Talk - Game Architecture with Scriptable Objects by Ryan Hipple </summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Variables/Game Object List", order = 3000)]
    public class GameObjectList : GameObjectVar
    {
        public List<GameObject> list;

        readonly System.Random Random = new System.Random();

        /// <summary> Value of the Bool variable</summary>
        public override GameObject Value
        {
            get => GetValue();
            set
            {
               list.Add(value);
            }
        }

        public virtual GameObject GetValue()
        {
            int index = Random.Next(list.Count);
            return list[index];
        }
    }
}
