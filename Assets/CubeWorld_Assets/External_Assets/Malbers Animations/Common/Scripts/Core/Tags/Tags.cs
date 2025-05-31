using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations
{
    [HelpURL("https://malbersanimations.gitbook.io/animal-controller/secondary-components/scriptables/tags")]
    [AddComponentMenu("Malbers/Utilities/Tools/Tags")]
    public class Tags : MonoBehaviour//, ITag
    {
        /// <summary>Keep a Track of the game objects that has this component</summary>
        public static List<Tags> TagsHolders;

        /// <summary>List of tags for this component</summary>
        public List<Tag> tags = new();

        /// <summary>Convert the list to a Hash Set  (So the element does not repeat and is faster to find)</summary>
        private readonly HashSet<int> HashTag = new();


        void OnEnable()
        {
            TagsHolders ??= new List<Tags>();

            //Save the GameObject who has this Tags on the global TagsHolders list //Better for saving performance
            TagsHolders.Add(this);
        }
        void OnDisable()
        {
            //Remove the GameObject who has this Tags on the global TagsHolders list //Better for saving performance
            TagsHolders.Remove(this);
        }

        public void Awake()
        {
            //Clean Repeated elements
            var x = new HashSet<Tag>(tags);

            x.Remove(null); //Remove if there's a null tag

            foreach (var item in x)
                HashTag.Add(item.ID);

        }

        /// <summary>Return all the Gameobjects that use a tag</summary>
        public static List<GameObject> GambeObjectbyTag(Tag tag) => GambeObjectbyTag(tag.ID);

        /// <summary>Return all the Gameobjects that use a tag ID</summary>
        public static List<GameObject> GambeObjectbyTag(int tag)
        {
            var go = new List<GameObject>();

            if (TagsHolders == null || TagsHolders.Count == 0) return null;

            foreach (var item in TagsHolders)
            {
                if (item.HasTag(tag))
                {
                    go.Add(item.gameObject);
                }
            }

            if (go.Count == 0) return null;
            return go;
        }

        public static List<GameObject> GambeObjectbyTag(Tag[] tags)
        {
            var go = new List<GameObject>();

            if (Tags.TagsHolders == null || TagsHolders.Count == 0) return null;

            foreach (var item in TagsHolders)
            {
                if (item.HasTag(tags))
                {
                    go.Add(item.gameObject);
                }
            }

            if (go.Count == 0) return null;
            return go;
        }




        /// <summary>Return all the Gameobjects that use a tag ID</summary>
        public bool HasTag(Tag tag) => HasTag(tag.ID);

        /// <summary>Return all the Gameobjects that use a tag ID</summary>
        public bool HasTag(int key) => HashTag.Contains(key);

        /// <summary>Check if this component has a more than one tag</summary>
        public bool HasTag(params Tag[] enteringTags)
        {
            foreach (var tag in enteringTags)
            {
                if (HashTag.Contains(tag)) return true;
            }
            return false;
        }


        /// <summary>Check if this component has any of the Tags of an array</summary>
        public bool HasTag(params int[] enteringTags)
        {
            foreach (var tag in enteringTags)
            {
                if (HashTag.Contains(tag)) return true;
            }
            return false;
        }


        /// <summary>Check if this component has all tags given</summary>
        public bool HasAllTags(params Tag[] enteringTags)
        {
            foreach (var tag in enteringTags)
            {
                if (!HashTag.Contains(tag)) return false;
            }
            return true;
        }


        /// <summary>Check if this component has all tags given</summary>
        public bool HasAllTags(params int[] enteringTags)
        {
            foreach (var tag in enteringTags)
            {
                if (!HashTag.Contains(tag)) return false;
            }
            return true;
        }

        /// <summary>Add a new Tag</summary>
        public void AddTag(Tag t)
        {
            if (!HashTag.Contains(t.ID))
            {
                tags.Add(t);
                HashTag.Add(t.ID);
            }
        }

        /// <summary>Remove an existing Tag</summary>
        public void RemoveTag(Tag t)
        {
            if (HashTag.Contains(t))
            {
                tags.Remove(t);
                HashTag.Remove(t.ID);
            }
        }
    }

    public static class Tag_Transform_Extension
    {
        ///// <summary> Returns if the Transform has a malbers Tag</summary>
        //public static bool HasMalbersTag(this Transform t, Tag tag)
        //{
        //    var tagC = t.GetComponent<Tags>();
        //    return tagC != null ? tagC.HasTag(tag) : false;
        //}

        /// <summary> Returns if the Transform has a malbers Tag</summary>
        public static bool HasMalbersTag(this Transform t, Tag tag)
        {
            return Tags.TagsHolders.Exists(x => x.transform == t && x.HasTag(tag));
        }


        /// <summary> Returns if the Transform has malbers Tag from a list</summary>
        public static bool HasMalbersTag(this Transform t, params Tag[] tags)
        {
            return Tags.TagsHolders.Exists(x => x.transform == t && x.HasTag(tags));
        }

        /// <summary> Returns if the Gameobject has a Malbers Tag</summary>
        public static bool HasMalbersTag(this GameObject t, Tag tag) => HasMalbersTag(t.transform, tag);
        public static bool HasMalbersTag(this Component t, Tag tag) => HasMalbersTag(t.transform, tag);

        public static bool HasMalbersTag(this GameObject t, params Tag[] tags) => HasMalbersTag(t.transform, tags);



        /// <summary>  Use GetComponentInParent to find a Tag in their Parents</summary>
        private static Tags GetTagInParent(GameObject t)
        {
            return t.GetComponentInParent<Tags>(false);
        }

        public static GameObject FindWithMalbersTag(this GameObject t, Tag tag)
        {
            var allTags = t.GetComponentsInChildren<Tags>(false);

            if (allTags != null)
            {
                foreach (var item in allTags)
                {
                    if (item.HasTag(tag))
                        return item.gameObject;
                }
            }
            return null;
        }

        public static Transform FindWithMalbersTag(this Transform t, Tag tag)
        {
            if (t == null) return null;

            var allTags = t.GetComponentsInChildren<Tags>(false);

            if (allTags != null)
            {
                foreach (var item in allTags)
                {
                    if (item.HasTag(tag))
                        return item.transform;
                }
            }
            return null;
        }


        ///// <summary>Search in the List of Static Tags if a gameObject is child of one of the Tag Holders </summary>
        //private static Tags GetTag(GameObject childOB)
        //{
        //    return Tags.TagsHolders.Find(t => childOB.transform.SameHierarchy(t.transform));
        //}

        /// <summary> Returns if the Transform has a malbers Tag in one of its parents</summary>
        public static bool HasMalbersTagInParent(this Transform t, Tag tag)
        {
            Tags tagC = GetTagInParent(t.gameObject);
            return tagC != null && tagC.HasTag(tag);
        }


        public static bool HasMalbersTagInParent(this Transform t, params Tag[] tags)
        {
            //if (tags == null || tags.Length == 0) return true; //Meaning no need to search tags

            var tagC = GetTagInParent(t.gameObject);
            return tagC != null && tagC.HasTag(tags);
        }

        /// <summary> Returns if the Gameobject has a Malbers Tag in one of its parents</summary>
        public static bool HasMalbersTagInParent(this GameObject t, Tag tag)
        {
            var tagC = GetTagInParent(t);
            return tagC != null && tagC.HasTag(tag);
        }

        public static bool HasMalbersTagInParent(this GameObject t, params Tag[] tags)
        {
            var tagC = GetTagInParent(t);
            return tagC != null && tagC.HasTag(tags);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Tags)), CanEditMultipleObjects]
    public class TagsEd : Editor
    {
        SerializedProperty tags;
        private void OnEnable()
        {
            tags = serializedObject.FindProperty("tags");
        }

        public override void OnInspectorGUI()
        {
            //MalbersEditor.DrawDescription("Dupicated Tags will cause errors. Keep unique tags in the list");
            serializedObject.Update();
            EditorGUILayout.PropertyField(tags, true);
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}