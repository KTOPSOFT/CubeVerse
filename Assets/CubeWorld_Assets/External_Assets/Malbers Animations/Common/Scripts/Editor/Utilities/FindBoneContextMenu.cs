#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace MalbersAnimations
{
#if UNITY_EDITOR
    public static class FindBoneContextMenu
    {
        [InitializeOnLoadMethod]
        public static void Init()
        {
            EditorApplication.contextualPropertyMenu += ContextualPropertyMenu;
        }

        private static void ContextualPropertyMenu(GenericMenu menu, SerializedProperty property)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                //Find if the property is a Transform or a GameObject
                if (MSerializedTools.GetPropertyType(property) != typeof(Transform)
                    && MSerializedTools.GetPropertyType(property) != typeof(GameObject))
                    return;

                menu.AddItem(new GUIContent("Set Human Bone/Body/Head"), false, () => { FinBone(property, HumanBodyBones.Head); });
                menu.AddItem(new GUIContent("Set Human Bone/Body/Eyes"), false, () => { FinBone(property, "Eyes"); });
                menu.AddItem(new GUIContent("Set Human Bone/Body/Right Eye"), false, () => { FinBone(property, HumanBodyBones.RightEye); });
                menu.AddItem(new GUIContent("Set Human Bone/Body/Right Eye"), false, () => { FinBone(property, HumanBodyBones.RightEye); });

                menu.AddItem(new GUIContent("Set Human Bone/Body/Neck"), false, () => { FinBone(property, HumanBodyBones.Neck); });
                menu.AddItem(new GUIContent("Set Human Bone/Body/Hips"), false, () => { FinBone(property, HumanBodyBones.Hips); });
                menu.AddItem(new GUIContent("Set Human Bone/Body/Spine"), false, () => { FinBone(property, HumanBodyBones.Spine); });
                menu.AddItem(new GUIContent("Set Human Bone/Body/Chest"), false, () => { FinBone(property, HumanBodyBones.Chest); });
                menu.AddItem(new GUIContent("Set Human Bone/Body/Upper Chest"), false, () => { FinBone(property, HumanBodyBones.UpperChest); });

                menu.AddItem(new GUIContent("Set Human Bone/Left/Left Shoulder"), false, () => { FinBone(property, HumanBodyBones.LeftShoulder); });
                menu.AddItem(new GUIContent("Set Human Bone/Left/Left Upper Arm"), false, () => { FinBone(property, HumanBodyBones.LeftUpperArm); });
                menu.AddItem(new GUIContent("Set Human Bone/Left/Left Lower Arm"), false, () => { FinBone(property, HumanBodyBones.LeftLowerArm); });
                menu.AddItem(new GUIContent("Set Human Bone/Left/Left Hand"), false, () => { FinBone(property, HumanBodyBones.LeftHand); });

                menu.AddItem(new GUIContent("Set Human Bone/Left/Left Upper Leg"), false, () => { FinBone(property, HumanBodyBones.LeftUpperLeg); });
                menu.AddItem(new GUIContent("Set Human Bone/Left/Left Lower Leg"), false, () => { FinBone(property, HumanBodyBones.LeftLowerLeg); });
                menu.AddItem(new GUIContent("Set Human Bone/Left/Left Foot"), false, () => { FinBone(property, HumanBodyBones.LeftFoot); });
                menu.AddItem(new GUIContent("Set Human Bone/Left/Left Toes"), false, () => { FinBone(property, HumanBodyBones.LeftToes); });


                menu.AddItem(new GUIContent("Set Human Bone/Right/Right Hand"), false, () => { FinBone(property, HumanBodyBones.RightHand); });
                menu.AddItem(new GUIContent("Set Human Bone/Right/Right Upper Arm"), false, () => { FinBone(property, HumanBodyBones.RightUpperArm); });
                menu.AddItem(new GUIContent("Set Human Bone/Right/Right Lower Arm"), false, () => { FinBone(property, HumanBodyBones.RightLowerArm); });


                menu.AddItem(new GUIContent("Set Human Bone/Right/Right Shoulder"), false, () => { FinBone(property, HumanBodyBones.RightShoulder); });
                menu.AddItem(new GUIContent("Set Human Bone/Right/Right Upper Leg"), false, () => { FinBone(property, HumanBodyBones.RightUpperLeg); });
                menu.AddItem(new GUIContent("Set Human Bone/Right/Right Lower Leg"), false, () => { FinBone(property, HumanBodyBones.RightLowerLeg); });
                menu.AddItem(new GUIContent("Set Human Bone/Right/Right Foot"), false, () => { FinBone(property, HumanBodyBones.RightFoot); });
                menu.AddItem(new GUIContent("Set Human Bone/Right/Right Toes"), false, () => { FinBone(property, HumanBodyBones.RightToes); });

            }
        }

        private static void FinBone(SerializedProperty property, HumanBodyBones bone)
        {
            Object Bone = GetBone(property, bone);

            if (Bone != null)
            {
                property.objectReferenceValue = Bone;
                property.serializedObject.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning("No Human Bone Found");
            }
        }

        private static Object GetBone(SerializedProperty property, HumanBodyBones bone)
        {
            var Holder = property.serializedObject.targetObject;
            var animator = (Holder as Component).FindComponent<Animator>();

            if (animator != null)
            {
                Transform target;

                if (animator.isHuman)
                {
                    target = animator.GetBoneTransform(bone);
                }
                else
                {
                    target = animator.transform.FindGrandChild(bone.ToString());
                }

                if (target == null) return null;

                if (MSerializedTools.GetPropertyType(property) == typeof(Transform))
                    return target;
                else return target.gameObject;
            }
            return null;
        }


        private static void FinBone(SerializedProperty property, string bone)
        {
            var Holder = property.serializedObject.targetObject;
            var animator = (Holder as Component).FindComponent<Animator>();

            if (animator != null)
            {
                Transform target;

                target = animator.transform.FindGrandChild(bone.ToString());

                if (target == null) return;

                if (MSerializedTools.GetPropertyType(property) == typeof(Transform))
                    property.objectReferenceValue = target;
                else property.objectReferenceValue = target.gameObject;

                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
#endif
}