using UnityEngine;
using MalbersAnimations.Controller;
using MalbersAnimations.Events;
using MalbersAnimations.IK;
using System.Collections.Generic;

#if AC_FISHNET
using FishNet.Object;
using FishNet.Component.Animating;
#elif AC_PURRNET
using PurrNet;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.VargrMultiplayer
{
	[DisallowMultipleComponent]
	[AddComponentMenu("Malbers/Multiplayer/AnimalInstance")]
	[RequireComponent(typeof(Tags))]
	public partial class AnimalInstance : NetworkBehaviour
    {
		public int EditorTab1 = 0;
		private const float MAX_PASSED_TIME = 0.3f; // Lagcomp for Projectile need to be fixed
		public Stats Vitals;
		public MAnimal Animal;
		public NetMWeaponManager WeaponManager;
		protected IKManager m_IKManager;
		protected NetworkAnimator m_netAnimator;
		protected (uint Key, int OrginalPrefabID) m_ShiftData = (0,-1);
		[SerializeField] protected GameObject local_component;
		[SerializeField] protected bool m_Debug = true;
		public GameObjectEvent PreShapeShift = new();
		public BoolEvent OwnerEvent = new();
		public BoolEvent SpawnedEvent = new();
		public Tags AnimalTags;
		protected List<Tag> m_defaultTags;
		
		protected virtual void Awake()
		{
			AnimalTags = GetComponent<Tags>();
			m_defaultTags = new(AnimalTags.tags);
			if(Animal == null) Animal = GetComponent<MAnimal>();
			if(Vitals == null) Vitals = GetComponent<Stats>();
			if(WeaponManager == null) WeaponManager = GetComponent<NetMWeaponManager>();
			SetupHAP();
			SetupAnimator();
			SetupIKSync();
			SetupWeaponManager();
		}

		partial void SetupHAP();
		partial void HAPSpawnSync();

		protected void CheckWeapons()
		{
			WeaponUtility[] weapons = GetComponentsInChildren<WeaponUtility>();
			foreach(WeaponUtility weapon in weapons) WeaponManager.Holster_SetWeapon(weapon.Weapon);
		}

		protected virtual void UpdateOwner(bool asServer = false)
		{	
			/*Disabled Shapeshift for now.
			if(isServer){ //Look at this as we may want to change how this is working.
				if(m_ShiftData.Key != 0){
					if(isController){
						if(SpawnManager.StoredShiftState.ContainsKey(m_ShiftData.Key)){
							SpawnManager.StoredShiftState[m_ShiftData.Key].ApplyState(Animal);
							SpawnManager.StoredShiftState.Remove(m_ShiftData.Key);
						}
					}else{
						TargetAnimalStateRpc(m_ShiftData.Key);
					}
				}
			}
			*/

			//Debug.Log("ANIMAL OWNER UPDATE - asServer: "+asServer, this);
			if(isController){
				int state = Animal.Anim.GetInteger("State");
				int mode = Animal.Anim.GetInteger("Mode");
				Animal.SetEnable(state != 10);
				if(state != 0 && state != 10){
					Animal.State_Activate(state);
					Animal.Mode_Activate(mode);
				}
			}else{
				Animal.SetEnable(false);
			}

			local_component?.SetActive(isController);
			setName();
			//gameObject.GetComponent<CCSync>().InitCharacter();
			OwnerEvent?.Invoke(isController);
		}

		public virtual void setName()
		{
			gameObject.name = "Animal";
			NameAddID();
			if(isController) gameObject.name += "-LOCAL";
		}
/*
DISABLED SHAPE SHIFT FOR NOW
		#region ShapeShift

		public virtual void ShapeShift(GameObject prefab = null)
		{
			if(!isServer) return;

			if(prefab == null && m_ShiftData.OrginalPrefabID != -1) prefab = NetworkManager.GetPrefab(m_ShiftData.OrginalPrefabID, true).gameObject;

			if(prefab == null)	return;

			uint shiftKey = SpawnManager.NextShiftKey();

			if(isController) SpawnManager.StoredShiftState.Add(shiftKey, new NetworkStoredState(Animal));

			RpcShapeShift(shiftKey);

			SpawnShape(prefab, shiftKey);
		}


		[ObserversRpc]
		protected virtual void RpcShapeShift(uint key)
		{
			// Anything needed for Everyone PreShapeShift info Here
			if(isServer) return;
			if(isController) SpawnManager.StoredShiftState.Add(key, new NetworkStoredState(Animal));
		}
		protected virtual void SpawnShape(GameObject prefab, uint shiftKey, bool statCopy = true)
		{
			if(!isServer) return;
			if(prefab == null) return;
			// Need to save date but as Animal is Root of this, we can to that Above.
			SpawnPoint spawnPoint = SpawnManager.GetBackStage(true);
			GameObject go = Instantiate(prefab);
			spawnPoint.OrientCharactor(go.transform);
			PreShapeShift.Invoke(go);
			AnimalInstance s_animal = go.GetComponent<AnimalInstance>();
			s_animal.SetShiftData(shiftKey, m_ShiftData.OrginalPrefabID == -1 ? prefabId : m_ShiftData.OrginalPrefabID);
			// Currently Copy the Existing ones.
			if(statCopy) ShapeShiftStats(s_animal);
			base.RemoveOwnership();
			base.Spawn(go, base.Owner);

			PostShapeShift();
		}

		protected virtual void ShapeShiftStats(AnimalInstance target)
		{
			for (int i = 0; i < Vitals.stats.Count; i++)
			{
				if(target.Vitals.Stat_Get(Vitals.stats[i].ID) == null) continue;
				target.Vitals.Stat_SetValue(Vitals.stats[i].ID, Vitals.stats[i].Value);
			}
		}

		protected virtual void PostShapeShift()
		{
			if(!IsServerStarted) return;
			base.Despawn();
		}

		[ObserversRpc]
		protected void TargetAnimalStateRpc(uint key)
		{
			if(!isController) return;

			if(SpawnManager.StoredShiftState.ContainsKey(key)){
				SpawnManager.StoredShiftState[key].ApplyState(Animal);
				SpawnManager.StoredShiftState.Remove(key);
			}
		}
		public virtual void SetShiftData(uint key, int ogfab)
		{
			if(m_ShiftData.OrginalPrefabID != -1) return;

			m_ShiftData.Key = key;
			m_ShiftData.OrginalPrefabID = ogfab;
		}
		#endregion
*/
		protected void SetupIKSync()
		{
			m_IKManager = GetComponent<IKManager>();
			if(m_IKManager == null) return;

			foreach (IKSet item in m_IKManager.sets)
			{
				item.OnWeightChanged.AddListener((data) => SyncIK(item.name, item.active, data));
				item.OnSetEnable.AddListener(() => SyncIK(item.name, true, item.Weight));
				item.OnSetDisable.AddListener(() => SyncIK(item.name, false, item.Weight));
			}
		}

		public void TransferOwnership()
		{
			if(!isController) return;

		}
    }




#region Inspector
#if UNITY_EDITOR
    [CustomEditor(typeof(AnimalInstance))]
    public class AnimalIntEditor : Editor
    {
        AnimalInstance M;

        private SerializedProperty
            EditorTab1, OnStateParam, OnModeParam, LocalComp, PreShiftEvent, Animal, Vitals, WeaponManager
        ;

		protected virtual void OnEnable()
        {
			M = (AnimalInstance)target;
			EditorTab1 = serializedObject.FindProperty("EditorTab1");
#if FISHNET
			OnStateParam = serializedObject.FindProperty("m_OnState_Parameters");
			OnModeParam = serializedObject.FindProperty("m_OnMode_Parameters");
#endif
			LocalComp = serializedObject.FindProperty("local_component");
			PreShiftEvent = serializedObject.FindProperty("PreShapeShift");
			Animal = serializedObject.FindProperty("Animal");
			Vitals = serializedObject.FindProperty("Vitals");
			WeaponManager = serializedObject.FindProperty("WeaponManager");

		}

		protected virtual void DrawAnimalHeader()
		{
			MalbersEditor.DrawDescription("Animal Instance");
		}

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
			DrawAnimalHeader();
            EditorTab1.intValue = DrawTabs(EditorTab1.intValue);

			if(EditorTab1.intValue == 0){
				DrawGeneral();
			}else if(EditorTab1.intValue == 1){
				DrawAnimation();
			}else if(EditorTab1.intValue == 2){
				DrawEvents();
			}

            serializedObject.ApplyModifiedProperties();            
        }

		protected virtual int DrawTabs(int targeValue)
		{
			return GUILayout.Toolbar(targeValue, new string[] { "General", "Animation Link", "Events" });
		}
		protected virtual void DrawGeneral()
		{
			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(Animal, new GUIContent("MAnimal Component"));
				EditorGUILayout.PropertyField(Vitals, new GUIContent("Stats Component"));
				EditorGUILayout.PropertyField(WeaponManager, new GUIContent("Weapon Manager", "This must be Net Weapon Manager"));
				EditorGUILayout.PropertyField(LocalComp, new GUIContent("Local Component", "Game Object that holds all Local Componets that should only be active on the Owner System"));
            }
		}
		protected virtual void DrawEvents()
		{
			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(PreShiftEvent, new GUIContent("Pre Shape Shift Event"));
            }
		}
		protected virtual void DrawAnimation()
		{
#if FISHNET
			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
				EditorGUILayout.PropertyField(OnStateParam, new GUIContent("On State Trigger Sync", "List of Animation Parameters that should be passed again when the On State Trigger is set"));
				EditorGUILayout.PropertyField(OnModeParam, new GUIContent("On Mode Trigger Sync", "List of Animation Parameters that should be passed again when the On Mode Trigger is set"));
            }
#endif
		}
    }
#endif
#endregion

}