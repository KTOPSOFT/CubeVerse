using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using UnityEngine;



namespace MalbersAnimations.InputSystem
{
    public partial class MInputLink : MonoBehaviour, IInputSource
    {
#if ENABLE_INPUT_SYSTEM && UNITY_EDITOR
        [ContextMenu("Auto Connect All")]
        private void AutoConnectAll()
        {
            //Animal Connections
            Connect_Sprint();
            Connect_SpeedUp();
            Connect_SpeedDown();
            Connect_Strafe();
            Connect_Interact_Zone();
            Connect_Interact_Pick();
            Connect_Interact_Event();
            Connect_LockOnTarget();
            Connect_LockOnTargetNext();
        }


        [ContextMenu("Connect/'Sprint' -> MAnimal.Sprint")]
        private void Connect_Sprint()
        {
            var input = FindInputButton("Sprint");

            if (input != null)
            {
                var method = this.GetUnityAction<bool>("MAnimal", "Sprint");

                if (method != null)
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(input.OnInputChanged, method);
                MTools.SetDirty(this);

                Debug.Log("Input [Sprint] Connected to MAnimal.Sprint");
            }
            else
            {
                Debug.Log("Input 'Sprint' not found");
            }
        }

        [ContextMenu("Connect/'SpeedUp' -> MAnimal.SpeedUp")]
        private void Connect_SpeedUp()
        {
            var input = FindInputButton("SpeedUp");

            if (input != null)
            {
                input.interaction = MInputInteraction.Down;

                var method = this.GetUnityAction("MAnimal", "SpeedUp");

                if (method != null)
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(input.OnInputDown, method);
                MTools.SetDirty(this);

                Debug.Log("Input [SpeedUp] Connected to MAnimal.SpeedUp");
            }
            else
            {
                Debug.Log("Input 'SpeedUp' not found");
            }
        }

        [ContextMenu("Connect/'SpeedDown' -> MAnimal.SpeedDown")]
        private void Connect_SpeedDown()
        {
            var input = FindInputButton("SpeedDown");

            if (input != null)
            {
                input.interaction = MInputInteraction.Down;

                var method = this.GetUnityAction("MAnimal", "SpeedDown");

                if (method != null)
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(input.OnInputDown, method);
                MTools.SetDirty(this);

                Debug.Log("Input [SpeedDown] Connected to MAnimal.SpeedDown");
            }
            else
            {
                Debug.Log("Input 'SpeedDown' not found");
            }
        }

        [ContextMenu("Connect/'Strafe' -> MAnimal.Strafe")]
        private void Connect_Strafe()
        {
            var input = FindInputButton("Strafe");

            if (input != null)
            {
                input.interaction = MInputInteraction.Toggle;

                var method = this.GetUnityAction<bool>("MAnimal", "Strafe");

                if (method != null)
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(input.OnInputChanged, method);
                MTools.SetDirty(this);

                Debug.Log("Input [Strafe] Connected to MAnimal.Strafe");
            }
            else
            {
                Debug.Log("Input 'Strafe' not found");
            }
        }

        [ContextMenu("Connect/'Interact' -> MAnimal.Zone_Activate")]
        private void Connect_Interact_Zone()
        {
            var input = FindInputButton("Interact");

            if (input != null)
            {
                input.interaction = MInputInteraction.Down;

                var method = this.GetUnityAction("MAnimal", "Zone_Activate");

                if (method != null)
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(input.OnInputDown, method);
                MTools.SetDirty(this);

                Debug.Log("Input [Interact] Connected MAnimal.Zone_Activate");

            }
            else
            {
                Debug.Log("Input 'Interact' not found");
            }
        }

        [ContextMenu("Connect/'Interact' -> MPickUp.TryPickUpDrop")]
        private void Connect_Interact_Pick()
        {
            var input = FindInputButton("Interact");

            if (input != null)
            {
                input.interaction = MInputInteraction.Down;

                var method = this.GetUnityAction("MPickUp", "TryPickUpDrop");

                if (method != null)
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(input.OnInputDown, method);
                MTools.SetDirty(this);

                Debug.Log("Input [Interact] Connected MPickUp.TryPickUpDrop");

            }
            else
            {
                Debug.Log("Input 'Interact' not found");
            }
        }

        [ContextMenu("Connect/'Interact' -> Interact Event.Invoke")]
        private void Connect_Interact_Event()
        {
            var input = FindInputButton("Interact");

            if (input != null)
            {
                input.interaction = MInputInteraction.Down;

                var interactEvent = MTools.GetInstance<MEvent>("Interact");

                if (interactEvent != null)
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(input.OnInputDown, interactEvent.Invoke);
                MTools.SetDirty(this);
                Debug.Log("Input [Interact] Connected Interact Event.Invoke()");
            }
            else
            {
                Debug.Log("Input 'Interact' not found");
            }
        }


        [ContextMenu("Connect/'LockOn' -> LockOnTarget.LockTargetToggle")]
        private void Connect_LockOnTarget()
        {
            var input = FindInputButton("LockOn");

            if (input != null)
            {
                input.interaction = MInputInteraction.Down;

                var method = this.GetUnityAction("LockOnTarget", "LockTargetToggle");

                if (method != null)
                {
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(input.OnInputDown, method);
                    MTools.SetDirty(this);

                    Debug.Log("Input [LockTarget] Connected to LockOnTarget.LockTargetToggle");
                }
            }
            else
            {
                Debug.Log("Input 'LockTarget' not found");
            }
        }

        [ContextMenu("Connect/'LockNextTarget' -> LockOnTarget.Target_Scroll")]
        private void Connect_LockOnTargetNext()
        {
            var input = FindInputButton("LockNextTarget");

            if (input != null)
            {
                input.interaction = MInputInteraction.Vector2;

                var method = this.GetUnityAction<Vector2>("LockOnTarget", "Target_Scroll");

                if (method != null)
                {
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(input.OnInputV2Value, method);
                    MTools.SetDirty(this);

                    Debug.Log("Input [LockNextTarget] Connected to LockOnTarget.Target_Scroll");
                }
            }
            else
            {
                Debug.Log("Input 'LockNextTarget' not found");
            }
        }
        private MInputAction FindInputButton(string name)
        {
            foreach (var map in m_MapButtons)
            {
                var button = map.buttons.Find(x => x.name == name);
                if (button != null)
                {
                    //Debug.Log($"[{name}] connected");
                    return button;
                }
            }
            return null;
        }

        [ContextMenu("Update Action Maps Values")]
        internal void UpdateActionMapsInspector()
        {
            for (int i = 0; i < m_MapButtons.Count; i++)
            {
                var button = m_MapButtons[i];

                if (button != null)
                    button.ActionMap = playerInput.actions.actionMaps[i];
            }

            Debug.Log("Updated Action Maps values");
            MTools.SetDirty(this);
        }


        [ContextMenu("Connect/'Pause' -> Pause Event.Invoke")]
        private void Connect_Pause_Event()
        {
            var input = FindInputButton("Pause");

            if (input != null)
            {
                input.interaction = MInputInteraction.Toggle;

                var interactEvent = MTools.GetInstance<BoolVar>("Toggle Paused");

                if (interactEvent != null)
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(input.OnInputChanged, interactEvent.SetValue);
                MTools.SetDirty(this);
                Debug.Log("Input [Interact] Connected Interact Event.Invoke()");
            }
            else
            {
                Debug.Log("Input 'Interact' not found");
            }
        }

#endif
    }
}