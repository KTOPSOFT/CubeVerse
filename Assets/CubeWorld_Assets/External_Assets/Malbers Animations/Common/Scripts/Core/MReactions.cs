
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MalbersAnimations.Reactions
{
    [AddComponentMenu("Malbers/Animal Controller/Reactions")]
    public class MReactions : MonoBehaviour
    {
        [Tooltip("Try to find a target on Enable. (Search first in the hierarchy then in the parents)")]
        [ContextMenuItem("Find Target", "GetTarget on Enable")]
        public bool FindTarget = false;

        [SerializeField] private Component Target;

        [Tooltip("React when the Component is Enabled")]
        public bool ReactOnEnable = false;
        [Tooltip("React when the Component is Disabled")]
        public bool ReactOnDisable = false;



        [SerializeReference, SubclassSelector]
        public Reaction reaction;

        private void OnEnable()
        {
            if (FindTarget)
                Target = GetComponent(reaction.ReactionType) ?? GetComponentInParent(reaction.ReactionType);

            if (ReactOnEnable) React();
        }

        private void OnDisable()
        {
            if (ReactOnDisable) React();
        }

        [ContextMenu("Find Target")]
        public void GetTarget()
        {
            Target = GetComponent(reaction.ReactionType) ?? GetComponentInParent(reaction.ReactionType);
            MTools.SetDirty(this);
        }

        public void React()
        {
            if (reaction != null)
            {
                reaction.React(Target);
            }
            else
            {
                Debug.LogError("Reaction is Empty. Please use any reaction", this);
            }
        }


        public void React(int index)
        {
            if (reaction != null && reaction is ListReaction list)
            {
                index = Mathf.Clamp(index, 0, list.reactions.Count - 1);
                list.reactions[index]?.React(Target);
            }
            else
            {
                Debug.LogError("Reaction is Empty. Please use any reaction", this);
            }
        }


        public void React(Component newAnimal)
        {
            if (reaction != null)
            {
                Target = reaction.VerifyComponent(newAnimal);

                reaction.TryReact(Target);
            }
            else
            {
                Debug.LogError("Reaction is Empty. Please use any reaction", this);
            }
        }

        public void React(GameObject newAnimal) => React(newAnimal.transform);
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(MReactions))]
    public class MReactionEditor : Editor
    {
        SerializedProperty FindTarget, Target, reaction, ReactOnEnable, ReactOnDisable;

        private GUIContent _SearchIcon;
        private GUIContent _OnEnable, _OnDisable, _FindTarget;
        private GUIContent _ReactIcon;
        MReactions M;


        private void OnEnable()
        {
            M = (MReactions)target;
            FindTarget = serializedObject.FindProperty("FindTarget");
            Target = serializedObject.FindProperty("Target");
            reaction = serializedObject.FindProperty("reaction");
            ReactOnDisable = serializedObject.FindProperty("ReactOnDisable");
            ReactOnEnable = serializedObject.FindProperty("ReactOnEnable");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (M.reaction != null)
            {
                using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    var width = 28f;

                    if (Application.isPlaying)
                    {
                        if (_ReactIcon == null)
                        {
                            _ReactIcon = EditorGUIUtility.IconContent("d_PlayButton@2x");
                            _ReactIcon.tooltip = "React at Runtime";
                        }

                        if (GUILayout.Button(_ReactIcon, EditorStyles.miniButton, GUILayout.Width(width), GUILayout.Height(20)))
                        {
                            (target as MReactions).React();
                        }
                    }

                    if (_SearchIcon == null)
                    {
                        _SearchIcon = EditorGUIUtility.IconContent("Search Icon");
                        _SearchIcon.tooltip = "Find Target in hierarchy";
                    }

                    if (GUILayout.Button(_SearchIcon, EditorStyles.miniButton, GUILayout.Width(width), GUILayout.Height(20)))
                    {
                        (target as MReactions).GetTarget();
                    }

                    EditorGUIUtility.labelWidth = 60;
                    EditorGUILayout.PropertyField(Target);
                    EditorGUIUtility.labelWidth = 0;

                    #region ICONS

                    if (_FindTarget == null)
                    {
                        _FindTarget = EditorGUIUtility.IconContent("d_ol_plus");
                        _FindTarget.tooltip = "GetTarget on Enable";
                    }

                    if (_OnEnable == null)
                    {
                        _OnEnable = EditorGUIUtility.IconContent("d_toggle_on_focus");
                        _OnEnable.tooltip = "React On Enable";
                    }

                    if (_OnDisable == null)
                    {
                        _OnDisable = EditorGUIUtility.IconContent("d_toggle_bg_focus");
                        _OnDisable.tooltip = "React On Disable";
                    }
                    #endregion

                    FindTarget.boolValue = GUILayout.Toggle(FindTarget.boolValue, _FindTarget,
                        EditorStyles.miniButton, GUILayout.Width(width), GUILayout.Height(20));

                    var dC = GUI.color;
                    if (ReactOnEnable.boolValue) GUI.color = Color.green;
                    ReactOnEnable.boolValue = GUILayout.Toggle(ReactOnEnable.boolValue, _OnEnable,
                    EditorStyles.miniButton, GUILayout.Width(width), GUILayout.Height(20));
                    GUI.color = dC;

                    if (ReactOnDisable.boolValue) GUI.color = Color.green;
                    ReactOnDisable.boolValue = GUILayout.Toggle(ReactOnDisable.boolValue, _OnDisable,
                    EditorStyles.miniButton, GUILayout.Width(width), GUILayout.Height(20));
                    GUI.color = dC;
                }
            }

            // using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(reaction);
                EditorGUI.indentLevel--;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}