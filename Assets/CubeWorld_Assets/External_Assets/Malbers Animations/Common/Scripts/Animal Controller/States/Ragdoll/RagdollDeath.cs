using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [AddTypeMenu("Death/Ragdoll Replace")]
    public class RagdollDeath : State
    {
        [Header("Ragdoll")]
        [Tooltip("Ragdoll prefab that will replace the current animal controller")]
        public GameObject ragdollPrefab;

        public float Drag = 0.1f;
        public float AngularDrag = 0.1f;


        public bool EnablePreProcessing = true;
        public CollisionDetectionMode collision = CollisionDetectionMode.ContinuousSpeculative;

        //public override string StateName => "Death/Ragdoll Replace";
        public override string StateIDName => "Death";

        [Tooltip("Destroy the Animal after the Ragdoll is created. If is set to false then it will only Hide the GameObject")]
        public bool DestroyAnimal = true;

        public override void Activate()
        {
            animal.Mode_Stop();
            animal.Mode_Interrupt();
            base.Activate();
            Replace();
        }

        public void Replace()
        {
            //Instantiate the new Ragdoll model
            GameObject ragdollInstance = Instantiate(ragdollPrefab, transform.position, transform.rotation);

            //Prepare the ragdoll
            var AllJoints = ragdollInstance.GetComponentsInChildren<CharacterJoint>();
            foreach (var joint in AllJoints) { joint.enablePreprocessing = true; }


            //need to disable it, otherwise when we copy over the hierarchy objects position/rotation, the ragdoll will try each time to 
            //"correct" the attached joint, leading to a deformed/glitched instance
            ragdollInstance.SetActive(false);

            //Match the Root Bones 
            ragdollInstance.transform.SetPositionAndRotation(transform.position, transform.rotation);

            //Map all the Animal Bones in the Dictionary
            var animalBones = animal.RootBone.GetComponentsInChildren<Transform>();
            var AnimalBoneMap = new Dictionary<string, Transform>();
            foreach (Transform bone in animalBones) AnimalBoneMap[bone.name] = bone;


            //Map all the Bones in the Ragdoll in a Dictionary
            var ragdollBones = ragdollInstance.GetComponentsInChildren<Transform>();
            //var RagdollBoneMap = new Dictionary<string, Transform>();
            //foreach (Transform bone in ragdollBones) RagdollBoneMap[bone.name] = bone;


            foreach (var bn in ragdollBones)
            {
                //Match the Position and Rotation of Animal Bones to the Ragdoll Bone
                if (AnimalBoneMap.TryGetValue(bn.name, out Transform root))
                {
                    bn.SetPositionAndRotation(root.position, root.rotation);
                }
            }

            animal.Anim.enabled = false; //Disable Animator (?)


            //Disable/Remove all mesh renderers in the ragdoll
            var allSkinnedMeshRendererRagdoll = ragdollInstance.GetComponentsInChildren<SkinnedMeshRenderer>();
            var allMeshRendererRagdoll = ragdollInstance.GetComponentsInChildren<MeshRenderer>();

            foreach (var rdoll in allSkinnedMeshRendererRagdoll)
            {
                Destroy(rdoll.gameObject);
                // rdoll.gameObject.SetActive(false);
            }

            foreach (var rdoll in allMeshRendererRagdoll)
            {
                Destroy(rdoll.gameObject);
                // rdoll.gameObject.SetActive(false);
            }



            var allSkinnedMeshRendererAnimal = animal.GetComponentsInChildren<SkinnedMeshRenderer>(false);
            var allMeshRendererAnimal = animal.GetComponentsInChildren<MeshRenderer>(false);

            var allLODs = animal.GetComponentsInChildren<LODGroup>();

            //change the LODs of the cameras
            foreach (var lod in allLODs)
            {
                lod.transform.parent = ragdollInstance.transform;
            }

            //Move all Skinned mesh renderers to the Ragdoll 
            foreach (var rdoll in allSkinnedMeshRendererAnimal)
            {

                if (rdoll.gameObject.activeInHierarchy)
                {
                    if (rdoll.GetComponentInParent<LODGroup>() == null)
                    {
                        rdoll.transform.parent = ragdollInstance.transform;
                    }
                }
                RemapSkinToNewBones(rdoll, ragdollInstance.transform);
            }

            //Move all Skinned mesh renderers to the Ragdoll 
            foreach (var rdoll in allMeshRendererAnimal)
            {
                if (rdoll.gameObject.activeInHierarchy)
                {
                    if (rdoll.GetComponentInParent<LODGroup>() == null)
                    {
                        var Parent = ragdollInstance.transform.FindGrandChild(rdoll.transform.parent.name) ??
                            ragdollInstance.transform.FindGrandChild(rdoll.transform.parent.parent.name);

                        rdoll.transform.parent = Parent;
                    }
                }
            }
            //  return; 

            Vector3 HitDirection = Vector3.zero;
            Vector3 HitPoint = Vector3.zero;
            Collider HitCollider = null;
            ForceMode ForceMod = ForceMode.VelocityChange;

            if (animal.TryGetComponent<IMDamage>(out var IMDamage))
            {
                HitDirection = IMDamage.HitDirection;
                HitPoint = IMDamage.HitPosition;
                HitCollider = IMDamage.HitCollider;
                ForceMod = IMDamage.LastForceMode;
            }

            MDebug.Draw_Arrow(HitPoint, HitDirection.normalized * 3, Color.yellow, 5);


            var ragdollRB = ragdollInstance.GetComponentsInChildren<Rigidbody>();
            foreach (var rb in ragdollRB)
            {
                rb.collisionDetectionMode = collision;
                rb.isKinematic = false;
                rb.linearVelocity = animal.RB.linearVelocity;  //Match the velocity that the animal had onto the ragdoll

                rb.linearDamping = Drag;
                rb.angularDamping = AngularDrag;

                if (HitCollider != null && HitCollider.name.Contains(rb.name)) //Find the collider and the rigidbody
                {
                    rb.AddForce(HitDirection, ForceMod);
                }
            }

            animal.OnStateChange.Invoke(ID);//Invoke the Event!!


            ragdollInstance.SetActive(true);

            animal.Delay_Action(() =>
            {
                if (DestroyAnimal)
                    Destroy(animal.gameObject);
                else
                    animal.gameObject.SetActive(false);
            });
        }


        private void RemapSkinToNewBones(SkinnedMeshRenderer thisRenderer, Transform RootBone)
        {
            if (thisRenderer == null) return;

            var OldRootBone = thisRenderer.rootBone;

            var NewBones = RootBone.GetComponentsInChildren<Transform>();

            Dictionary<string, Transform> boneMap = new();

            foreach (Transform t in NewBones)
            {
                boneMap[t.name] = t;
            }

            Transform[] boneArray = thisRenderer.bones;


            for (int idx = 0; idx < boneArray.Length; ++idx)
            {
                string boneName = boneArray[idx].name;

                if (false == boneMap.TryGetValue(boneName, out boneArray[idx]))
                {
                    Debug.LogError("failed to get bone: " + boneName);
                }
            }
            thisRenderer.bones = boneArray;

            if (boneMap.TryGetValue(OldRootBone.name, out Transform ro))
            {
                thisRenderer.rootBone = ro; //Remap the rootbone
            }
        }
    }
}