using MalbersAnimations.Controller;
using System.Collections.Generic;
using UnityEngine;
using MalbersAnimations.Scriptables;
using UnityEngine.Events;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations
{
    [DefaultExecutionOrder(1000)]
    /// <summary> Explosion Logic</summary>
    [AddComponentMenu("Malbers/Damage/Explosion Force")]

    public class MExplosion : MDamager
    {
        [Tooltip("The Explosion will happen on Start ")]
        public bool ExplodeOnStart;
        [Tooltip("Value needed for the AddExplosionForce method default = 0 ")]
        public float upwardsModifier = 0;
        [Tooltip("Radius of the Explosion")]
        public float radius = 10;
        [Tooltip("Life of the explosion, after this time has elapsed the Explosion gameobject will be destroyed ")]
        public float life = 10f;
        [Tooltip("Layer of objects that block the explosion. Make sure this layer does not include the hit layer.")]
        public LayerReference blockingLayer;

        public int ColliderSize = 50;

        public AnimationCurve DamageCurve = new(MTools.DefaultCurveLinearInverse);

        public UnityEvent OnExplode;

        [HideInInspector] public int Editor_Tabs1;


        private Collider[] colliders;

        void Start()
        {
            colliders = new Collider[ColliderSize];

            if (ExplodeOnStart)
                Explode();
        }

        public virtual void Explode()
        {
            Physics.OverlapSphereNonAlloc(transform.position, radius, colliders, Layer, triggerInteraction);             //Ignore Colliders

            List<GameObject> Real_Roots = new();

            for (int i = 0; i < colliders.Length; i++)
            {
                var col = colliders[i];

                if (col == null) continue; //no other collider were found    

                if (dontHitOwner && Owner && col.transform.IsChildOf(Owner.transform)) continue;   //Don't hit yourself

                // Perform a raycast to check for sight-blocking objects
                Vector3 directionToCollider = col.bounds.center - transform.position;
                float distanceToCollider = directionToCollider.magnitude;

                if (Physics.Raycast(transform.position, directionToCollider.normalized, out RaycastHit hit, distanceToCollider, blockingLayer))
                {
                    // If the raycast hits something in the blocking Layer, skip this collider
                    continue;
                }

                var rb = col.attachedRigidbody;

                GameObject realRoot = col.transform.FindObjectCore().gameObject;       //Get the gameObject on the entering collider

                //Means the Root is not on the real root since its not on the search layer
                if (realRoot.layer != col.gameObject.layer)
                    realRoot = MTools.FindRealParentByLayer(col.transform);

                if (!Real_Roots.Contains(realRoot))
                {
                    //Debug.Log("realRoot = " + realRoot);

                    //Distance of the collider and the Explosion
                    var Distance = Vector3.Distance(transform.position, col.bounds.center);

                    var ExplotionRange = DamageCurve.Evaluate(Distance / radius); //Calculate the explostion range 

                    if (rb != null && rb.useGravity)
                    {
                        col.attachedRigidbody.AddExplosionForce(Force * ExplotionRange, transform.position, radius, upwardsModifier, forceMode);
                    }


                    Debugging("Apply Explosion", col);

                    Real_Roots.Add(realRoot); //Affect Only One 


                    if (statModifier.ID != null)
                    {
                        var modif = new StatModifier(statModifier)
                        {
                            Value = statModifier.Value * ExplotionRange    //Do Damage depending the distance from the explosion
                        };

                        TryDamage(col.gameObject, modif);
                        TryInteract(col.gameObject);

                        ////Use the Damageable comonent instead!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        //modif.ModifyStat(other.GetComponentInParent<Stats>());
                    }
                }

                col = null; //Clear the Collider
            }
            //Debug.Log("-----------------------");

            OnExplode.Invoke();

            Destroy(gameObject, life);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = (Color.red);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MExplosion))]
    [CanEditMultipleObjects]
    public class MExposionEd : MDamagerEd
    {
        SerializedProperty ExplodeOnStart, upwardsModifier, radius, life, Editor_Tabs1, DamageCurve, blockingLayer, OnExplode;
        protected string[] Tabs1 = new string[] { "General", "Damage", "Extras", "Events" };

        private void OnEnable()
        {
            FindBaseProperties();

            ExplodeOnStart = serializedObject.FindProperty("ExplodeOnStart");

            upwardsModifier = serializedObject.FindProperty("upwardsModifier");
            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");
            DamageCurve = serializedObject.FindProperty("DamageCurve");

            radius = serializedObject.FindProperty("radius");
            life = serializedObject.FindProperty("life");
            blockingLayer  = serializedObject.FindProperty("blockingLayer");
            OnExplode = serializedObject.FindProperty("OnExplode");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDescription("Explosion Damager. Damage is reduced if the target is far from the center of the explosion");

            Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, Tabs1);

            int Selection = Editor_Tabs1.intValue;

            if (Selection == 0) DrawGeneral();
            else if (Selection == 1) DrawDamage();
            else if (Selection == 2) DrawExtras();
            else if (Selection == 3) DrawEvents();

            serializedObject.ApplyModifiedProperties();
        }

        protected override void DrawGeneral(bool drawbox = true)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Explosion", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(ExplodeOnStart, new GUIContent("On Start"));
                EditorGUILayout.PropertyField(DamageCurve);
                EditorGUILayout.PropertyField(radius);
                EditorGUILayout.PropertyField(blockingLayer);

                EditorGUILayout.PropertyField(life);
            }
            base.DrawGeneral(drawbox);
        }


        private void DrawDamage()
        {
            DrawStatModifier();
            DrawCriticalDamage();
        }

        private void DrawExtras()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                DrawPhysics(false);
                EditorGUILayout.PropertyField(upwardsModifier);
            }
            EditorGUILayout.EndVertical();

            DrawMisc();
        }

        protected override void DrawCustomEvents()
        {
            EditorGUILayout.PropertyField(OnExplode);
            base.DrawCustomEvents();          
        }

    }
#endif

}
