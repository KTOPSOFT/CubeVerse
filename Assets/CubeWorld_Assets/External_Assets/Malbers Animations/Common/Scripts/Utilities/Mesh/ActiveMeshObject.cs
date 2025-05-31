using MalbersAnimations.Reactions;
using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations.Utilities
{
    [CreateAssetMenu(menuName = "Malbers Animations/Tools/ActiveMesh Object")]

    public class ActiveMeshObject : ScriptableObject
    {
        [Tooltip("When this ActiveMesh is Added. Set it as the Current Active Mesh")]
        public bool Activate = true;
        [Tooltip("Actual Static or Skinned Mesh to add to the Animal Controller")]
        public Renderer Mesh;
        [Tooltip("New Set of Materials to add")]
        public Material[] materials;


        [Header("Data")]
        [Tooltip("Name of the Item that will be used to locate it on the set (Optional)")]
        public StringReference ItemName = new();
        [Tooltip("Set on which the new Mesh Item will be added")]
        public StringReference SetName = new();
        [Tooltip("Parent of the new Item Mesh")]
        public StringReference Parent = new();


        [Header("Reactions")]
        [SerializeReference, SubclassSelector]
        public Reaction OnActive;
        [SerializeReference, SubclassSelector]
        public Reaction OnDeactive;

        public void AddMesh(ActiveMeshes ACM)
        {
            //Find if we have a set with the same name
            ActiveSMesh set = ACM.Meshes.Find(item => item.Name == SetName);

            if (set == null) //Create a new one if we do not have it
            {
                set = new ActiveSMesh
                {
                    Name = SetName,
                    name = SetName,
                    Onwer = ACM,
                    MeshItems = new() //Create a new list of MeshItems

                };
                ACM.Meshes.Add(set);

                if (ACM.debug) Debug.Log($"<B>Active Mesh</B> Set <B>{SetName.Value}</B> Added to the list", this);
            }

            var NewParent = (ACM.Owner.FindGrandChild(Parent.Value) ?? null) ?? ACM.RootBone.FindGrandChild(Parent.Value) ?? ACM.transform;
            var meshItem = set.MeshItems.Find(item => item.Mesh != null && item.Mesh.name == name);

            if (meshItem == null)
            {
                MeshItem FoundSame = null;
                Renderer meshRenderer = null;
                Transform MeshTransform = null;

                //Search every mesh on the set and on the MeshItems to find if we have the same renderer somewhere
                foreach (var item in set.MeshItems)
                {
                    if (item.MainRenderer is SkinnedMeshRenderer SMR1 && Mesh is SkinnedMeshRenderer SMR2)
                    {
                        if (SMR1.sharedMesh == SMR2.sharedMesh)
                        {
                            FoundSame = item;
                            MeshTransform = item.Mesh;
                            meshRenderer = item.MainRenderer;
                            if (ACM.debug) Debug.Log($"Found Equal Skinned Mesh Renderer (Using same Object)... {MeshTransform.name}");
                            break;
                        }
                    }
                    else if (item.MainRenderer is MeshRenderer MR1 && Mesh is MeshRenderer MR2) //Ugly but it works
                    {
                        if (MR1.GetComponent<MeshFilter>().sharedMesh == MR2.GetComponent<MeshFilter>().sharedMesh)
                        {
                            FoundSame = item;
                            MeshTransform = item.Mesh;
                            meshRenderer = item.MainRenderer;
                            if (ACM.debug) Debug.Log($"Found Equal  Mesh Renderer (Using same Object)...{MeshTransform.name}");
                            break;
                        }
                    }
                }

                //Instantiate the new mesh
                if (FoundSame == null)
                {
                    meshRenderer = Instantiate(Mesh);
                    meshRenderer.gameObject.name = name;        //Change the name of the new mesh
                    meshRenderer.transform.SetParent(NewParent);          //Set the parent of the new mesh
                    meshRenderer.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity); //Reset the position and rotation    
                    MeshTransform = meshRenderer.transform;


                    //Rebone the Skinned Mesh to the Root Bone
                    if (meshRenderer is SkinnedMeshRenderer skinned)
                    {
                        if (MTools.ReboneSkinnedMesh(ACM.RootBone, skinned))
                        {
                            if (ACM.debug) Debug.Log($"<B>[Active Mesh]</B> - Bone Transfer Completed: [{skinned.gameObject.name}]");
                        }
                    }
                }

                if (materials != null && materials.Length > 0)
                    meshRenderer.materials = materials; //Set the new materials to the new mesh

                var NewMeshItem = new MeshItem()
                {
                    Mesh = MeshTransform,
                    Parent = NewParent,
                    MeshOn = OnActive,
                    MeshOff = OnDeactive,
                    ItemName = ItemName,
                    materials = materials,
                    MainRenderer = meshRenderer
                };


                set.MeshItems.Add(NewMeshItem); //Add the new mesh to the list

                if (Activate)
                {
                    set.ChangeMesh(set.MeshItems.Count - 1); //Set the new mesh as the current active mesh the last one added
                }
            }
        }



        public void AddMesh(GameObject MESH)
        {
            var act = MESH.FindComponent<ActiveMeshes>();
            if (act) AddMesh(act);
        }

        public void AddMesh(Component MESH)
        {
            var act = MESH.FindComponent<ActiveMeshes>();
            if (act) AddMesh(act);
        }
    }
}