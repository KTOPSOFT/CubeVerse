using MalbersAnimations.Events;
using MalbersAnimations.Reactions;
using MalbersAnimations.Scriptables;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Utilities
{
    [AddComponentMenu("Malbers/Utilities/Mesh/Active Meshes")]
    public class ActiveMeshes : MonoBehaviour
    {
        [Tooltip("Root Bone for any skinned mesh. See Mesh Avatar")]
        public Transform RootBone;


        [Tooltip("Main Object owner of the Active Mesh Component")]
        public Transform Owner;

        public List<ActiveSMesh> Meshes = new();
        public bool showMeshesList = true;

        public bool debug;

        public ActiveSMesh this[int index] { get => Meshes[index]; set => Meshes[index] = value; }

        public int Count => Meshes.Count;

        public ActiveSMesh Pinned;

        public Int2Event OnMeshChanged = new();

        /// <summary> All Active Meshes Index stored on a string separated by a space ' '  </summary>
        public string AllIndex
        {
            set
            {
                string[] getIndex = value.Split(' ');

                for (int i = 0; i < Count; i++)
                {
                    if (getIndex.Length > i)
                    {

                        if (int.TryParse(getIndex[i], out int index))
                        {
                            if (index == -1) continue;

                            Meshes[i].ChangeMesh(index);
                        }
                    }
                }
            }
            get
            {
                string AllIndex = "";

                for (int i = 0; i < Count; i++)
                {
                    AllIndex += Meshes[i].Current.ToString() + " ";
                }

                AllIndex.Remove(AllIndex.Length - 1);   //Remove the last space }
                return AllIndex;
            }
        }

        /// <summary> Upgrade from old versions </summary>
        [SerializeField] private bool MeshItemUpdated = false;


        [SerializeField] private int selectedMeshIndex;


        #region EDITOR CALL FUNCTIONS
        public bool SyncMeshItem()
        {
            if (MeshItemUpdated) return false; //Do this only when the MeshItemUpdated is false


            var needsUpdate = false;

            foreach (var item in Meshes)
            {
                needsUpdate = item.SyncMesh() || needsUpdate;

                Debug.Log($"<B>[Active Mesh]</B> Syncing Mesh Items on <B>{item.Name}</B>", this);
            }

            //update the MeshItemUpdated to false
            if (!Application.isPlaying)
            {
                MeshItemUpdated = true;
                Debug.Log($"[{name}] <b>[Active Mesh]</b> Updated. Save the Prefab", this);
                MTools.SetDirty(this);
            }
            else
            {
                if (needsUpdate)
                {
                    // Debug.Log($"[{name}] [Active Mesh] New Mesh Item needs to be updated. Please select the [Active Mesh Component] and let the Editor Update to v2", this);
                }
            }

            return true;
        }

        public bool ReviewNames()
        {
            var NeedsUpdate = false;

            foreach (var Set in Meshes)
            {
                if (Set.MeshItems != null)
                    foreach (var item in Set.MeshItems)
                    {
                        NeedsUpdate = item.SetParameters() || NeedsUpdate;
                    }
            }
            if (NeedsUpdate)
            {
                //Debug.Log("[Active Mesh] - Names Updated", this);
                MTools.SetDirty(this);
            }
            return NeedsUpdate;
        }




        [ContextMenu("Reset Sync")]
        void ResetSync()
        {
            MeshItemUpdated = false;
            SyncMeshItem();

            MTools.SetDirty(this);
        }



        [ContextMenu("Fill Extra Parameters")]
        public void SetExtraParameters()
        {
            foreach (var mesh in Meshes)
            {
                foreach (var item in mesh.MeshItems)
                {
                    if (string.IsNullOrEmpty(item.ItemName))
                    {
                        item.ItemName = item.Mesh != null ? item.Mesh.name : $"No {mesh.Name}";
                    }

                    if (item.Mesh && (item.Renderers == null || item.Renderers.Length == 0))
                        item.Renderers = item.Mesh.GetComponentsInChildren<Renderer>();
                }
            }

            Debug.Log("Extra Parameters Completed. Names and LODs", this);

            if (RootBone == null)
            {
                var animator = GetComponentInParent<Animator>();

                if (animator)
                {
                    RootBone = animator.avatarRoot;
                }
            }

            if (Owner == null) Owner = GetComponentInParent<IObjectCore>().transform;

            MTools.SetDirty(this);
        }
        #endregion


        public bool random;
        private void Start()
        {
            //Initialize the new List of Active Meshes
            SyncMeshItem();

            Initialize();

            if (random) Randomize();
        }

        private void Initialize()
        {
            if (Owner == null) Owner = transform; //Set the Owner to the Active Mesh Transform (if is null)

            foreach (var set in Meshes)
            {
                set.Initialize(this); //Initialize the Active Mesh Set
            }
        }


        /// <summary>  Randomize all the Active Meshes on the list </summary>
        public void Randomize()
        {
            for (int i = 0; i < Meshes.Count; i++)
            {
                var mat = Meshes[i];

                if (mat.MeshItems == null || mat.MeshItems.Count == 0) continue;

                var newIndex = UnityEngine.Random.Range(0, mat.MeshItems.Count);

                mat.ChangeMesh(newIndex);

                OnMeshChanged.Invoke(i, mat.Current); ; //Invoke the Event
            }
        }


        /// <summary> Set All ActiveMeshes Index from the Meshes list. </summary>
        public void SetActiveMeshesIndex(int[] MeshesIndex)
        {
            if (MeshesIndex.Length != Count)
            {
                Debug.LogError("Meshes Index array Lenghts don't match");
                return;
            }

            for (int i = 0; i < MeshesIndex.Length; i++)
            {
                Meshes[i].ChangeMesh(MeshesIndex[i]);

                OnMeshChanged.Invoke(i, Meshes[i].Current); ; //Invoke the Event
            }
        }


        /// <summary>Select an Element from the List by its index, and change to the next variation.  </summary>
        public virtual void ChangeMesh(int index)
        {
            var mesh = Meshes[index % Count];
            mesh.ChangeMesh();

            OnMeshChanged.Invoke(index % Count, mesh.Current); //Invoke the Event
        }

        /// <summary>Select an Element from the List by the index, and change to a variation. using its index</summary>
        public virtual void ChangeMesh(int index, int IndexMesh)
        {
            var mesh = Meshes[index % Count];

            mesh.ChangeMesh(IndexMesh - 1);

            OnMeshChanged.Invoke(index % Count, mesh.Current); //Invoke the Event
        }


        /// <summary> Change to next mesh using the name</summary>
        public virtual void ChangeMesh(string name, bool next)
        {
            int index = Meshes.FindIndex(item => item.Name == name);

            if (index != -1)
            {
                Meshes[index].ChangeMesh(next);
                OnMeshChanged.Invoke(index, Meshes[index].Current); //Invoke the Event
            }
        }

        public virtual void ChangeMesh(string name) => ChangeMesh(name, true);

        public virtual void ChangeMesh(string name, int CurrentIndex)
        {
            int index = Meshes.FindIndex(item => item.Name == name);

            if (index != -1)
            {
                Meshes[index].ChangeMesh(CurrentIndex);
                OnMeshChanged.Invoke(index, Meshes[index].Current); //Invoke the Event
            }
        }

        public virtual void ChangeMesh(int index, bool next)
        {
            Meshes[index].ChangeMesh(next);
            OnMeshChanged.Invoke(index, Meshes[index].Current); //Invoke the Event
        }

        /// <summary>Toggle all meshes on the list</summary>
        public virtual void ChangeMesh(bool next = true)
        {
            for (int i = 0; i < Meshes.Count; i++)
            {
                var mesh = Meshes[i];
                mesh.ChangeMesh(next);

                OnMeshChanged.Invoke(i, Meshes[i].Current); //Invoke the Event
            }
        }

        /// <summary>  Get the Active mesh by their name  </summary>
        public virtual ActiveSMesh GetActiveMesh(string name)
        {
            if (Count == 0) return null;

            return Meshes.Find(item => item.Name == name);
        }

        /// <summary>  Get the Active Mesh by their Index </summary>
        public virtual ActiveSMesh GetActiveMesh(int index)
        {
            //clamp the index with the Count of the list
            index = Mathf.Clamp(index, 0, Count - 1);
            return Meshes[index];
        }

        public virtual void Pin_Mesh(int index) => Pinned = GetActiveMesh(index);
        public virtual void Pin_Mesh(string name) => Pinned = GetActiveMesh(name);
        public virtual void Pin_SetMesh(int index) => Pinned.ChangeMesh(index - 1);



#if UNITY_EDITOR
        [ContextMenu("Create Event Listeners")]
        void CreateListeners()
        {
            MEventListener listener =
                (this.FindComponent<MEventListener>()
                ?? transform.root.GetComponentInChildren<MEventListener>())
                ?? transform.root.gameObject.AddComponent<MEventListener>();
            listener.Events ??= new List<MEventItemListener>();

            MEvent BlendS = MTools.GetInstance<MEvent>("Change Mesh");


            if (listener.Events.Find(item => item.Event == BlendS) == null)
            {
                var item = new MEventItemListener()
                {
                    Event = BlendS,
                    useVoid = false,
                    useString = true,
                    useInt = true
                };

                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseInt, ChangeMesh);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseString, ChangeMesh);
                listener.Events.Add(item);

                Debug.Log("<B>Change Mesh</B> Added to the Event Listeners");
                UnityEditor.EditorUtility.SetDirty(listener);

                MTools.SetDirty(listener);
            }
        }

        private void Reset()
        {
            if (Owner == null) Owner = transform; //Set the Owner to the Active Mesh Transform (if is null)
            if (RootBone == null)
            {
                var animator = GetComponentInParent<Animator>();

                if (animator)
                {
                    RootBone = animator.avatarRoot;
                }
                else
                {
                    RootBone = transform;
                }
            }

            MeshItemUpdated = true;
        }


#endif
    }

    [Serializable]
    public class ActiveSMesh
    {
        [HideInInspector] public string Name = "NameHere";

        public StringReference name = new();

        [HideInInspector] public bool Active = true;

        [HideInInspector]
        [SerializeField] public Transform[] meshes; //Old Meshes

        public List<MeshItem> MeshItems;

        [HideInInspector, SerializeField] public int Current;

        /// <summary>  Current Active Mesh Item  </summary>
        public MeshItem CurrentItem { get; set; }

        public ActiveMeshes Onwer { get; set; }

        [SerializeField, HideInInspector] //Editor Only 
        private int CurrentMeshItemIndex;

        public TransformEvent OnSetMeshChange = new();

        public bool SyncMesh()
        {
            var needsUpdate = false;

            //sync the MeshItems with the meshes
            if (MeshItems == null || MeshItems.Count != meshes.Length)
            {
                MeshItems = new();
                foreach (var transf in meshes)
                    MeshItems.Add(new() { Mesh = transf });

                needsUpdate = true;
            }
            return needsUpdate;
        }


        /// <summary>Change mesh to the Next/Before</summary>
        public virtual void ChangeMesh(bool next = true)
        {
            if (!Active) return;

            if (next)
                Current++;
            else
                Current--;

            if (Current >= MeshItems.Count) Current = 0;
            if (Current < 0) Current = MeshItems.Count - 1;

            //  Current %= MeshItems.Count;

            //for all meshes on the list
            for (int i = 0; i < MeshItems.Count; i++)
            {
                var item = MeshItems[i];

                if (i == Current) continue; //Skip the current one (it will be activated later)

                if (item.Mesh)
                {
                    if (item.Mesh.gameObject.IsPrefab())
                    {
                        Debug.LogWarning($"<B>[Active Mesh]</B> Mesh <B>{item.ItemName}</B> is a Prefab. It will not be deactivated", Onwer);
                        continue;
                    }
                    else if (item.Mesh.gameObject.activeSelf)
                    {
                        item.Mesh.gameObject.SetActive(false);

                        if (Application.isPlaying)
                        {
                            item.MeshOff?.React(Onwer); //Do this only when the game is playing
                            var HideSet = GetHideSet(item);
                            Unhide_Set(HideSet);
                        }
                    }
                }

                var NewItem = MeshItems[Current];


                if (NewItem.Mesh)
                {
                    if (NewItem.Mesh.gameObject.IsPrefab())
                    {
                        Debug.LogWarning($"<B>[Active Mesh]</B> Mesh <B>{NewItem.ItemName}</B> is a Prefab. It will not be activated", Onwer);
                        continue;
                    }


                    NewItem.Mesh.gameObject.SetActive(true);
                    NewItem.UpdateMaterials();

                    if (Application.isPlaying)
                    {
                        var HideSet = GetHideSet(NewItem);
                        Hide_Set(HideSet);
                    }
                }

                if (Application.isPlaying)
                {
                    NewItem.MeshOn?.React(Onwer);
                    OnSetMeshChange.Invoke(NewItem.Mesh); //Invoke that a new mesh was changed
                }

                CurrentItem = NewItem; //Store the current Item

                //  Debug.Log($"<B>[Active Mesh]</B> Set <B>{Name}</B> Current {Current} :  MeshItems.Count {MeshItems.Count}", Onwer);
            }
        }

        internal ActiveSMesh GetHideSet(MeshItem NewItem)
        {
            if (!string.IsNullOrEmpty(NewItem.HideSet)) //Hide All the Meshes on the HideSet
            {
                return Onwer.Meshes.Find(item => item.Name == NewItem.HideSet);   //Find the HideSet
            }
            return null;
        }

        internal void Hide_Set(ActiveSMesh HideSetMesh)
        {
            if (HideSetMesh != null) //Hide All the Meshes on the HideSet
            {
                foreach (var item in HideSetMesh.MeshItems)
                {
                    item.Mesh.gameObject.SetActive(false);
                }
            }
        }

        internal void Unhide_Set(ActiveSMesh HideSetMesh)
        {
            HideSetMesh?.ChangeMesh(HideSetMesh.Current); //Enable the current Mesh
        }

        /// <summary> Set a mesh by Index </summary>
        public virtual void ChangeMesh(int Index)
        {
            if (!Active) return;
            Current = Index - 1;
            ChangeMesh();
        }

        public void Set_by_BinaryIndex(int binaryCurrent)
        {
            int current = 0;

            for (int i = 0; i < MeshItems.Count; i++)
            {
                if (MTools.IsBitActive(binaryCurrent, i))
                {
                    current = i;   //Find the first active bit and use it as current
                    break;
                }
            }
            ChangeMesh(current);
        }

        internal void Initialize(ActiveMeshes owner)
        {
            foreach (var item in MeshItems)
            {
                //Verify if is a Prefab
                if (item.Mesh != null && item.Mesh.gameObject.IsPrefab())
                {
                    item.Mesh = GameObject.Instantiate(item.Mesh); //Instantiate the Prefab
                    item.Renderers = item.Mesh.GetComponentsInChildren<Renderer>(); //Get all the renderers on the new Mesh //Runtime Version

                    foreach (var renderer in item.Renderers)
                    {
                        //Rebone the Skinned Mesh to the Root Bone
                        if (renderer is SkinnedMeshRenderer skinned)
                        {
                            if (MTools.ReboneSkinnedMesh(owner.RootBone, skinned))
                            {
                                if (owner.debug) Debug.Log($"<B>[Active Mesh]</B> - Bone Transfer Completed: [{skinned.gameObject.name}]", skinned);
                            }
                        }
                    }

                }

                item.SetParameters(); //Find if some Parameters are not set properly

                if (item.Mesh)
                {
                    //Cache the firt Renderer Found (I need to cache all the Renderers, it may be LODs Included)
                    if (item.MainRenderer == null) item.MainRenderer = item.Mesh.GetComponentInChildren<Renderer>();


                    //If the materials are null or empty Store the Materials on Start       
                    if (item.MainRenderer && (item.materials == null || item.materials.Length == 0))
                        item.materials = item.MainRenderer.sharedMaterials;


                    item.UpdateMaterials(); //Update the materials on the Mesh
                }

                item.SetParent();
            }

            // Debug.Log($"<B>[Active Mesh]</B> Set <B>{Name}</B> Current {Current} :  MeshItems.Count {MeshItems.Count}", owner);

            Onwer = owner; //Set the Owner of the Active Mesh

            if (MeshItems != null && MeshItems.Count > 0)
            {
                Current = Mathf.Clamp(Current, 0, MeshItems.Count - 1); //Clamp the Current Index
                CurrentItem = MeshItems[Current]; //Set the Current Item
            }
        }
    }

    [Serializable]
    public class MeshItem
    {
        public string ItemName = "";

        [Tooltip("Main Transform mesh")]
        public Transform Mesh;

        [Tooltip("Name of the transform to parent the mesh. (Optional)")]
        public Transform Parent;

        [Tooltip("Hides  another ActiveMesh Set when this Item is Active. Works only in Play Mode")]
        public string HideSet;

        [Tooltip("New Set of Materials to change the mesh")]
        public Material[] materials;


        [Tooltip("LODs Included in the Mesh Item")]
        public Renderer[] Renderers;

        [SerializeReference, SubclassSelector]
        public Reaction MeshOn;

        [SerializeReference, SubclassSelector]
        public Reaction MeshOff;


        [SerializeField, HideInInspector] private int EditorTab;

        /// <summary> Cache the renderer from the Mesh Transform  </summary>
        [HideInInspector] public Renderer MainRenderer;

        //Uptade all materials on the LODs
        internal void UpdateMaterials()
        {
            if (materials != null && materials.Length > 0)
            {
                if (MainRenderer == null) MainRenderer = Mesh.GetComponentInChildren<Renderer>(); //Find the first one
                if (MainRenderer) MainRenderer.sharedMaterials = materials;


                //Update LODs
                if (Renderers != null && Renderers.Length > 0)
                {
                    for (int i = 0; i < Renderers.Length; i++)
                    {
                        var Re = Renderers[i];

                        if (Re != null && Re.transform != Mesh) //Skip the main Mesh
                        {
                            Re.sharedMaterials = materials;
                        }
                    }
                }
            }
        }

        internal bool SetParameters()
        {
            var needsUpdate = false;
            if (string.IsNullOrEmpty(ItemName))
            {
                ItemName = Mesh != null ? Mesh.name : $"Empty";
                needsUpdate = true;
            }

            if (Mesh && (Renderers == null || Renderers.Length == 0))
            {
                Renderers = Mesh.GetComponentsInChildren<Renderer>();
                needsUpdate = true;
            }

            return needsUpdate;
        }

        internal void SetParent()
        {
            //Parent the item to its parent
            if (Mesh != null && Parent != null && Mesh.parent != Parent)
            {
                Mesh.SetParent(Parent);
                Mesh.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity); //Reset the position and rotation
            }
        }
    }
}