using MalbersAnimations.Events;
using UnityEngine;
using UnityEngine.Events;
using MalbersAnimations.HAP;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.VargrMultiplayer
{
    public class NetRider : MRider
    {
        protected AnimalInstance m_netAnimal;
        protected EventSync m_eventSync;
        private bool m_inputChecked = false;
        private bool m_death = false;
        public bool isController => m_netAnimal != null ? m_netAnimal.isController : false;
        public bool isPlayerOwned => m_netAnimal == null ? false : (m_netAnimal is PlayerInstance playerInstance && playerInstance.isPlayerOwned);
        public void OwnerChanged(bool owner)
        {
            this.SetEnable(isController);
            // Look at possible Input grabs and setup from here??
        }
        public override void ForceDismount()
        {
            if(!isController) return;

            SendMountState(false, 0);
            base.ForceDismount();
        }
        public void TryMount()
        {
            if(m_death || !CanMount || !enabled || Montura == null) return;
            if(!isController) return;
            if( Montura is NetMount netMount && !netMount.TryMount(m_netAnimal.gameObject)) return;
            
            SendMountState(true, MountTrigger.MountID);
            base.MountAnimal();
        }

        public void TryDismount()
        {
            if(!CanDismount || !enabled || !Mounted) return;
            if(!isController) return;

            SendMountState(false, MountTrigger.DismountID);
            base.DismountAnimal();
        }

        public void RiderDeath()
        {
            if(!isController || !Mounted) return;
            m_death = true;
            TryDismount();
        }

        protected override void SetRiderParent(Transform target)
        {
            m_netAnimal.SetParent(target);
        }

        public override void MountTriggerEnter(Mount mount, MountTriggers mountTrigger)
        {
            if(isController) m_netAnimal.MonturaSync(mount.Animal.gameObject, mountTrigger.MountID);
            mount.NearbyRider = this;
            base.MountTriggerEnter(mount, mountTrigger);
        }

        public override void MountTriggerExit()
        {
            if (Montura){
                if(Montura is NetMount netMount){
                    netMount.ExitMountTrigger(this);
                }else{
                    Debug.LogWarning("Net Warning: Mounts need to have NetMount", Montura);
                }
            }

            Montura = null;

            base.MountTriggerExit();
        }

        public override void Start_Mounting()
        {
            if(isController){
                base.Start_Mounting();
                return;
            }

            Montura.StartMounting(this);

            IsOnHorse = false;
            Mounted = true; 

            ToogleColliders(false);
            ToggleCall = false; 

            if (!MountTrigger)
                MountTrigger = Montura.GetComponentInChildren<MountTriggers>();
        
            OnStartMounting.Invoke();
            RiderStatus?.Invoke(RiderAction.StartMount);
            UpdateCanMountDismount();
        }

        public override void End_Mounting()
        {
            if(isController){
                base.End_Mounting();
                m_inputChecked = false;
                return;
            }
            IsOnHorse = true;
            OnEndMounting.Invoke();
            Montura.End_Mounting();
            RiderStatus?.Invoke(RiderAction.EndMount);
            UpdateCanMountDismount();
        }

        public override void Start_Dismounting()
        {
            if(isController){
                base.Start_Dismounting();
                return;
            }

            Montura.Start_Dismounting();
            Mounted = false;
            OnStartDismounting.Invoke();
            RiderStatus?.Invoke(RiderAction.StartDismount);
            UpdateCanMountDismount();
        }

        public override void End_Dismounting()
        {
            if(isController){
                base.End_Dismounting();
                m_inputChecked = false;
                return;
            }

            IsOnHorse = false;
            if (Montura) Montura.EndDismounting();
            Montura = null;
            MountTrigger = null;
            ToggleCall = false;
            ToogleColliders(true);
            OnEndDismounting.Invoke();
            RiderStatus?.Invoke(RiderAction.EndDismount);
            UpdateCanMountDismount();
        }

        public void JoinSync(GameObject mount, bool isMounted)
        {
            if(isController) return;
            
            Set_StoredMount(mount);

            if(isMounted){
                IsOnHorse = true;
                mounted = true;
                Montura = MountStored;
                ToggleCall = false;
                ToogleColliders(false);
            }else{
                IsOnHorse = false;
                mounted = false;
                Montura = null;
                ToggleCall = false;
                ToogleColliders(true);
            }
            UpdateCanMountDismount();
        }

        public override void MountAnimal() => TryMount();

        public override void DismountAnimal() => TryDismount();
        
        public override void Set_StoredMount(GameObject mount)
        {
            // Run sync call 
            base.Set_StoredMount(mount);
            if(isController) m_netAnimal.UpdateStoredMount(mount);
        }

        public override void ClearStoredMount()
        {
            base.ClearStoredMount();
            if(isController) m_netAnimal.UpdateStoredMount(null);
        }

        public Vector3 SpawnRaycast() => TeleportRaycast();
        public override void Start_Mounted() {
            if(isController){            
                if(m_MountStored.Value.IsPrefab()){
                    m_netAnimal.SpawnMount(true);
                    return;
                }else{
                    base.Start_Mounted();
                }
            }
            StartMounted.Value = false;
        }
        public override void CallAnimal(bool call)
        {
            if (!CanCallAnimal) return;

            ToggleCall = call;
            
            if(ToggleCall){
                if (m_MountStored.Value.IsPrefab()){

                    if (!m_MountStored.Value.FindComponent<NetMount>()) return;

                    m_netAnimal.SpawnMount();
                    return;
                }

                if(MountStored){
                    if (Vector3.Distance(transform.position, MountStored.Animal.transform.position) > CallRadius){
                        Vector3 InstantiatePosition = TeleportRaycast();
                        MountStored.Animal.Teleport(InstantiatePosition);
                    }

                    MountStored.AI.SetActive(true);
                    MountStored.AI.SetTarget(RiderRoot, true); //Set the Rider as the Target to follow
                    MountStored.AI.Move(); //Move the Animal 

                    if (CallAnimalA != null && RiderAudio != null)
                        RiderAudio.PlayOneShot(CallAnimalA);

                    MountStored.OnCalled.Invoke(RiderRoot.gameObject);
                    RiderStatus?.Invoke(RiderAction.CallMount);
                }
            }else{
                if(MountStored){
                    StopMountAI();

                    if (StopAnimalA != null && RiderAudio != null)
                        RiderAudio.PlayOneShot(StopAnimalA);

                    RiderStatus?.Invoke(RiderAction.CallMountStop);
                }
            }            
        }

        public void RiderInputCheck()
        {
            if(!isPlayerOwned || m_inputChecked) return;
            m_PlayerOnInputCheck?.Invoke(Mounted);
            m_inputChecked = true;
        }

        #region UnityEvents
        [SerializeField] protected GameObjectEvent m_PlayerOnFindMount = new GameObjectEvent();
        [SerializeField] protected BoolEvent m_PlayerOnInputCheck = new BoolEvent();
        [SerializeField] protected BoolEvent m_PlayerOnCanMount = new BoolEvent();
        [SerializeField] protected BoolEvent m_PlayerOnCanDismount = new BoolEvent();
        [SerializeField] protected BoolEvent m_PlayerCanCallMount = new BoolEvent();

        [SerializeField] protected UnityEvent m_PlayerOnStartMounting = new UnityEvent();
        [SerializeField] protected UnityEvent m_PlayerOnEndMounting = new UnityEvent();
        [SerializeField] protected UnityEvent m_PlayerOnStartDismounting = new UnityEvent();
        [SerializeField] protected UnityEvent m_PlayerOnEndDismounting = new UnityEvent();
        #endregion
#if UNITY_EDITOR
        protected void OnValidate() {
            this.SetEnable(false);
		}
#endif
        public override void Awake()
        {
            m_netAnimal = GetComponent<AnimalInstance>();
            m_eventSync = gameObject.FindComponent<EventSync>();
            m_netAnimal.OwnerEvent.AddListener(OwnerChanged);

            base.Awake();

            OnFindMount.AddListener((data) => { if(isPlayerOwned) m_PlayerOnFindMount.Invoke(data); }); 
            OnCanMount.AddListener((data) => { if(isPlayerOwned) m_PlayerOnCanMount.Invoke(data); }); 
            OnCanDismount.AddListener((data) => { if(isPlayerOwned) m_PlayerOnCanDismount.Invoke(data); }); 
            CanCallMount.AddListener((data) => { if(isPlayerOwned) m_PlayerCanCallMount.Invoke(data); }); 
            OnStartMounting.AddListener(() => { if(isPlayerOwned) m_PlayerOnStartMounting.Invoke(); });
            OnEndMounting.AddListener(() => { if(isPlayerOwned) m_PlayerOnEndMounting.Invoke(); }); 
            OnStartDismounting.AddListener(() => { if(isPlayerOwned) m_PlayerOnStartDismounting.Invoke(); }); 
            OnEndDismounting.AddListener(() => { if(isPlayerOwned) m_PlayerOnEndDismounting.Invoke(); });

            OnEndDismounting.AddListener(() => { if(m_death) RiderAnimal.State_Activate(10); }); 

            m_eventSync.NetEventSync += RecivedEventSync;
        }
        private void SendMountState(bool state, int side)
        {
            if(!isController) return;
            if(Montura.InstantMount) return;

            if(state){
                m_eventSync?.EventBroadcast(side, 0, NetEventSource.Rider);
            }else{
                m_eventSync?.EventBroadcast(side, 1, NetEventSource.Rider);
            }
        }

        protected void SendRiderStatus(RiderAction status)
        {
            if(!isController)return;
            m_eventSync?.EventBroadcast((int)status, 2, NetEventSource.Rider);
        }

        protected virtual void RecivedEventSync(object data, int index, NetEventSource target)
        {
            if(target != NetEventSource.Rider) return;
            if(isController) return;
            switch(index)
            {
                case 0: ForceMountAnim((int)data); break;
                case 1: ForceDismountAnim((int)data); break;
                case 2: RecivedRiderStatus((RiderAction)data); break;
            }
        }
        private void RecivedRiderStatus(RiderAction status)
        {
            if(isController) return;
            RiderStatus.Invoke(status);
        }

        private void ForceMountAnim(int state)
        {
            if(isController) return;
            SetAnimParameter(MountHash, true);
            SetAnimParameter(MountSideHash, state);
        }

        private void ForceDismountAnim(int state)
        {
            if(isController) return;
            SetAnimParameter(MountHash, false);
            SetAnimParameter(MountSideHash, state);
        }
    }
    #region INSPECTOR
#if UNITY_EDITOR
    [CustomEditor(typeof(NetRider), true)]
    public class NetRiderEditor : MRiderEd
    {
        protected SerializedProperty
            PlayerOnStartMounting, PlayerOnEndMounting, PlayerOnStartDismounting, PlayerOnEndDismounting, PlayerOnCanMount, PlayerOnCanDismount, PlayerOnFindMount, PlayerCanCallMount, PlayerOnInputCheck
            ;


        protected override void OnEnable()
        {
            base.OnEnable();
            PlayerOnInputCheck = serializedObject.FindProperty("m_PlayerOnInputCheck");
            PlayerOnStartMounting = serializedObject.FindProperty("m_PlayerOnStartMounting");
            PlayerOnEndMounting = serializedObject.FindProperty("m_PlayerOnEndMounting");
            PlayerOnStartDismounting = serializedObject.FindProperty("m_PlayerOnStartDismounting");
            PlayerOnEndDismounting = serializedObject.FindProperty("m_PlayerOnEndDismounting");
            PlayerOnCanMount = serializedObject.FindProperty("m_PlayerOnCanMount");
            PlayerOnCanDismount = serializedObject.FindProperty("m_PlayerOnCanDismount");
            PlayerOnFindMount = serializedObject.FindProperty("m_PlayerOnFindMount");
            PlayerCanCallMount = serializedObject.FindProperty("m_PlayerCanCallMount");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            int Selection = Editor_Tabs1.intValue;

            if (Selection == 1) DrawEvents();

            serializedObject.ApplyModifiedProperties();
            //EditorGUILayout.EndVertical(); 
        }

        private void DrawEvents()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (animator.isExpanded = MalbersEditor.Foldout(animator.isExpanded, "Player Events Mount|Dismount"))
                {
                    EditorGUILayout.PropertyField(PlayerOnStartMounting);
                    EditorGUILayout.PropertyField(PlayerOnEndMounting);
                    EditorGUILayout.PropertyField(PlayerOnStartDismounting);
                    EditorGUILayout.PropertyField(PlayerOnEndDismounting);
                }

                if (m_root.isExpanded = MalbersEditor.Foldout(m_root.isExpanded, "Player Events Other"))
                {
                    EditorGUILayout.PropertyField(PlayerOnInputCheck);
                    EditorGUILayout.PropertyField(PlayerOnCanMount);
                    EditorGUILayout.PropertyField(PlayerOnCanDismount);
                    EditorGUILayout.PropertyField(PlayerOnFindMount);
                    EditorGUILayout.PropertyField(PlayerCanCallMount);
                }
            }
        }

       }
#endif
#endregion
}