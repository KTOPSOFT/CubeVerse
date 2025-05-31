using MalbersAnimations.Controller;
using UnityEngine;

namespace MalbersAnimations.Reactions
{
    [System.Serializable]
    [AddTypeMenu("Malbers/Animal/Mode")]
    public class ModeReaction : MReaction
    {
        public Mode_Reaction type = Mode_Reaction.Activate;
        public ModeID ID;

        [Tooltip("If set to -99 it will do a random ability from the Mode")]
        [Hide("type",
            (int)Mode_Reaction.Activate,
            (int)Mode_Reaction.ActivateForever,
            (int)Mode_Reaction.SetActiveIndex,
            (int)Mode_Reaction.ForceActivate,
            (int)Mode_Reaction.EnableAbility,
            (int)Mode_Reaction.DisableAbility
            )]
        public int Ability = -99;


        [Hide("type", (int)Mode_Reaction.CoolDown)]
        public bool HasCoolDown = true;

        [Hide("type", (int)Mode_Reaction.CoolDown)]
        public float coolDown = 0;

        [Hide("type", (int)Mode_Reaction.Activate, (int)Mode_Reaction.ForceActivate)]
        public AbilityStatus abilityStatus = AbilityStatus.PlayOneTime;

        [Hide("abilityStatus", (int)AbilityStatus.ActiveByTime)]
        public float AbilityTime = 3f;

        [Hide("type", (int)Mode_Reaction.Activate, (int)Mode_Reaction.ActivateForever, (int)Mode_Reaction.ForceActivate)]
        [Tooltip("Mode Power Value for the Animator Controller")]
        public float ModePower = 0;

        protected override bool _TryReact(Component component)
        {
            var animal = component as MAnimal;

            var mode = animal.Mode_Get(ID);
            if (mode == null || ID == null) return false;

            animal.Mode_SetPower(ModePower);

            switch (type)
            {
                case Mode_Reaction.Activate:
                    return animal.Mode_TryActivate(ID, Ability, abilityStatus, AbilityTime);
                case Mode_Reaction.ActivateForever:
                    return animal.Mode_TryActivate(ID, Ability, AbilityStatus.Forever);
                case Mode_Reaction.Interrupt:
                    if (animal.ActiveMode.ID == ID)
                    {
                        animal.Mode_Interrupt();
                        return true;
                    }
                    return false;
                case Mode_Reaction.SetActiveIndex:
                    mode.SetAbilityIndex(Ability);
                    return true;
                case Mode_Reaction.ResetActiveIndex:
                    mode.ResetAbilityIndex();
                    return true;
                case Mode_Reaction.Enable:
                    animal.Mode_Enable(ID);
                    return true;
                case Mode_Reaction.Disable:
                    animal.Mode_Disable(ID);
                    return true;
                case Mode_Reaction.CoolDown:
                    mode.HasCoolDown = HasCoolDown;
                    mode.CoolDown.Value = coolDown;
                    return true;
                case Mode_Reaction.ForceActivate:
                    animal.Mode_ForceActivate(ID, Ability, abilityStatus, AbilityTime);
                    return true;
                case Mode_Reaction.Stop:
                    animal.Mode_Stop();
                    return true;
                case Mode_Reaction.EnableAbility:
                    return animal.Mode_Ability_Enable(ID, Ability, true);
                case Mode_Reaction.DisableAbility:
                    return animal.Mode_Ability_Enable(ID, Ability, false);
                default:
                    return false;
            }
        }

        public enum Mode_Reaction
        {
            /// <summary>Tries to Activate the State of the Zone</summary>
            Activate,
            /// <summary>Activate  a Mode Forever</summary>
            ActivateForever,
            /// <summary>If the Animal will set the Mode Status to Interrput -2</summary>
            Interrupt,
            /// <summary>The Mode will be Stopped and its Input Reseted</summary>
            Stop,
            /// <summary>Force the State of the Zone to be enable even if it cannot be activate at the moment</summary>
            SetActiveIndex,
            /// <summary>Enable a  Disabled State </summary>
            Enable,
            /// <summary>Disable State </summary>
            Disable,
            /// <summary>Change the Status ID of the State in case the State uses its</summary>
            CoolDown,
            /// <summary>Change the Status ID of the State in case the State uses its</summary>
            ResetActiveIndex,
            /// <summary>Force a Mode to be activated.. ignoring if another mode is playing</summary>
            ForceActivate,
            /// <summary>Force a Mode to be activated.. ignoring if another mode is playing</summary>
            EnableAbility,
            /// <summary>Force a Mode to be activated.. ignoring if another mode is playing</summary>
            DisableAbility,
        }
    }
}
