using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.VargrMultiplayer
{
	[AddComponentMenu("Malbers/Multiplayer/AIInstance")]
	public class AIInstance : AnimalInstance
    {
		[SerializeField]
		private GameObject m_AIBrain;
        protected override void Awake()
        {
			m_AIBrain.SetActive(false);
			base.Awake();
        }

		protected override void UpdateOwner(bool asServer = false)
		{
			base.UpdateOwner(asServer);
			m_AIBrain.SetActive(isController);
		}

		public override void setName()
		{
			gameObject.name = "AI";
			base.NameAddID();
			if(isController) gameObject.name += "-LOCAL";
		}
    }
#region Inspector
#if UNITY_EDITOR
    [CustomEditor(typeof(AIInstance))]
    public class AIIntEditor : AnimalIntEditor
    {
        private SerializedProperty
            AIBrain
        ;

		protected override void OnEnable()
        {
			base.OnEnable();

			AIBrain = serializedObject.FindProperty("m_AIBrain");
		}

		protected override void DrawAnimalHeader()
		{
			MalbersEditor.DrawDescription("AI Instance");
		}

		protected override void DrawGeneral()
		{
			base.DrawGeneral();
			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(AIBrain, new GUIContent("AI Brain Component", "Game Object that holds AI"));
            }
		}
    }
#endif
#endregion
}