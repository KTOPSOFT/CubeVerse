using UnityEngine;
using MalbersAnimations.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations
{
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/main-components/malbers-input")]
    [AddComponentMenu("Malbers/Input/Malbers Input")]
    public class MalbersInput : MInput
    {
        #region Variables
        private ICharacterMove mCharacterMove; //Reference for Malbers Character 

        public InputAxis Horizontal = new("Horizontal", true, true);
        public InputAxis Vertical = new("Vertical", true, true);
        public InputAxis UpDown = new("UpDown", false, true);
        //   protected IAIControl AI;  //Referece for AI Input Sources


        public float horizontal;        //Horizontal Right & Left   Axis X
        public float vertical;          //Vertical   Forward & Back Axis Z
        public float upDown;            //Up Down value    

        public Vector3Event MovementEvent = new();

        #endregion 


        protected void InitializeCharacter()
        {
            mCharacterMove = GetComponent<ICharacterMove>();
            MoveCharacter = true;       //Set that the Character can be moved
                                        //   AI = this.FindInterface<IAIControl>();
        }


        protected override void OnEnable()
        {
            base.OnEnable();

            if (UpDown.active)
            {
                try
                {
                    var UPDown = Input.GetAxis(UpDown.name);
                }
                catch
                {
                    // Debug.LogError($"<B>[Up Down]</B> input doesn't exist. Please select any Character with the Malbers Input Component and hit <b>UpDown -> [Create]</b>", this);
                    // enabled = false;
                }
            }

            mCharacterMove?.Move(Vector3.zero);       //When the Input is Disable make sure the character/animal is not moving.
        }

        private void CheckUpDown()
        {
            if (UpDown.active)
            {
                //Check if UP Down Exist
#if UNITY_EDITOR
                bool found = false;

                var InputManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
                var axesProperty = InputManager.FindProperty("m_Axes");
                for (int i = 0; i < axesProperty.arraySize; ++i)
                {
                    var property = axesProperty.GetArrayElementAtIndex(i);
                    if (property.FindPropertyRelative("m_Name").stringValue.Equals(UpDown.name))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Debug.LogError($"<B>[Up Down]</B> input doesn't exist. Please select any Character with the Malbers Input Component and hit <b>UpDown -> [Create]</b>", this);
                    enabled = false;
                }
#endif
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            mCharacterMove?.Move(Vector3.zero);       //When the Input is Disable make sure the character/animal is not moving.
        }

        protected override void Initialize()
        {
            base.Initialize();
            InitializeCharacter();
            Horizontal.InputSystem = Vertical.InputSystem = UpDown.InputSystem = Input_System;
        }



        public virtual void UpAxis(bool input)
        {
            if (upDown == -1) return;        //This means that the Down Button was pressed so ignore the Up button
            upDown = input ? 1 : 0;
        }

        public virtual void DownAxis(bool input)
        {
            upDown = input ? -1 : 0;
        }

        void Update() => SetInput();


        /// <summary>Send all the Inputs and Axis to the Animal</summary>
        protected override void SetInput()
        {
            if (IgnoreOnPause.Value && Time.timeScale == 0) return;

            horizontal = Horizontal.GetAxis;
            vertical = Vertical.GetAxis;
            upDown = UpDown.GetAxis;

            MoveAxis = new Vector3(horizontal, upDown, vertical);

            OnMoveAxis(MoveAxis); //BroadCast the Horizontal and vertical values
            MovementEvent.Invoke(MoveAxis); //Invoke the Event for the Movement AXis


            mCharacterMove?.SetInputAxis(MoveCharacter ? MoveAxis : Vector3.zero);

            base.SetInput();
        }

        protected override bool IsJoystickInput()
        {
            if (horizontal != 0 && Mathf.Abs(horizontal) < 1) return true; //Meaning the Stick on the Joystic is moving slowly horizontally
            if (vertical != 0 && Mathf.Abs(vertical) < 1) return true; //Meaning the Stick on the Joystic is moving slowly vertically

            return base.IsJoystickInput();
        }

        public virtual void Horizontal_Enable(bool value) => Horizontal.active = value;
        public virtual void UpDown_Enable(bool value) => UpDown.active = value;
        public virtual void Vertical_Enable(bool value) => Vertical.active = value;

        public void ResetInputAxis() => MoveAxis = Vector3.zero;

    }
}