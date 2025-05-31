using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.IK
{
    [AddComponentMenu("Malbers/IK/IK Connector")]

    public class IKConnector : MonoBehaviour
    {
        [Tooltip("IK Set to enable or Disable on the IK Manager.\nUse coma',' to have check for multiple sets ")]
        public StringReference ikSet = new();

        [Tooltip("When a source is found, the IK Set Targets will be assigned too")]
        public bool SetTargetOnSource = true;
        [Tooltip("When a source is Enabled, the IK Set Targets will be assigned too")]
        public bool SetTargetOnEnable = false;
        public Transform[] targets;
        private IIKSource source;

        public virtual void Set_IKSource(GameObject owner)
        {
            source = owner.FindInterface<IIKSource>();
            if (source != null) Targets_Set();
        }

        public virtual void Set_IKSource(Component owner) => Set_IKSource(owner.gameObject);

        public virtual void Targets_Set()
        {
            source?.Target_Set(ikSet, targets);
        }

        public virtual void Targets_Clear() => source?.Target_Clear(ikSet);

        public virtual void Set_Enable(Component owner) => Set_Enable(owner.gameObject);

        public virtual void Set_Enable(GameObject owner)
        {
            source = owner.FindInterface<IIKSource>();
            source?.Set_Enable(ikSet, true);
            //  source?.Set_Weight(ikSet, true);
            if (SetTargetOnEnable) Targets_Set();
        }

        public virtual void Set_Disable(Component owner) => Set_Disable(owner.gameObject);

        public virtual void Set_Disable(GameObject owner)
        {
            source = owner.FindInterface<IIKSource>();
            source?.Set_Enable(ikSet, false);
        }

        public virtual void Set_Enable() => source?.Set_Enable(ikSet);
        public virtual void Set_Disable() => source?.Set_Disable(ikSet);


        [HideInInspector, SerializeField] private bool setName;
        [HideInInspector] public string IKSet;

        private void OnValidate()
        {
            if (!setName && string.IsNullOrEmpty(ikSet.Value))
            {
                ikSet.Value = IKSet;
                setName = true;
            }
        }
    }
}
