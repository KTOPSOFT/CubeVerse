using UnityEngine;

namespace MalbersAnimations.VargrMultiplayer
{
    public class ShapeshiftUtility : MonoBehaviour
    {
        public GameObject shapeshift_prefab;

        private bool active = true;

        public void ResetTrigger()
        {
            active = true;
        }

        public void TriggerShapeshift(GameObject target)
        {
            if(!active) return;
            active = false;

            AnimalInstance animal = target.FindComponent<AnimalInstance>();

            if(animal == null) return;
            Debug.Log("SHAPESHIFT DISABLED");
            //animal.ShapeShift(shapeshift_prefab);
        }
    }
}
