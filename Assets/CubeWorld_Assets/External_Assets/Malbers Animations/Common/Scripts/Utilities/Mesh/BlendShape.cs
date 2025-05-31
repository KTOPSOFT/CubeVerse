using MalbersAnimations.Events;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Utilities
{
    /// <summary>Manage the Blend Shapes of a Mesh</summary>
    [AddComponentMenu("Malbers/Utilities/Mesh/Blend Shapes")]
    public class BlendShape : MonoBehaviour
    {
        [CreateScriptableAsset]
        public BlendShapePreset preset;
        [RequiredField]
        public SkinnedMeshRenderer mesh;
        public SkinnedMeshRenderer[] LODs;

        public float[] blendShapes;                    //Value of the Blend Shape

        [Tooltip("Min Value to use on the blendshapes")]
        public float Min = -100;
        [Tooltip("Max Value to use on the blendshapes")]
        public float Max = 100;

        [Tooltip("Start with a random shape on Start")]
        public bool random;
        public int PinnedShape;

        /// <summary>Does the mesh has Blend Shapes? </summary>
        internal bool HasBlendShapes => mesh && mesh.sharedMesh.blendShapeCount > 0;


        private void Start()
        {
            if (preset)
                LoadPreset();
            else if (random)
                Randomize();
        }


        private void Reset()
        {
            mesh = GetComponentInChildren<SkinnedMeshRenderer>();
            if (mesh)
            {
                blendShapes = new float[mesh.sharedMesh.blendShapeCount];

                for (int i = 0; i < blendShapes.Length; i++)
                    blendShapes[i] = mesh.GetBlendShapeWeight(i);
            }
        }


        /// <summary>Returns the current Blend Shapes Values</summary>
        public virtual float[] GetBlendShapeValues()
        {
            if (HasBlendShapes)
            {
                float[] BS = new float[mesh.sharedMesh.blendShapeCount];

                for (int i = 0; i < BS.Length; i++)
                {
                    BS[i] = mesh.GetBlendShapeWeight(i);
                }
                return BS;
            }
            return null;
        }


        public void SmoothBlendShape(BlendShapePreset preset) => LoadSmoothPreset(preset);

        public void LoadSmoothPreset(BlendShapePreset preset)
        {
            StopAllCoroutines();
            preset.SmoothBlend(mesh);

            foreach (var item in LODs)
            {
                preset.SmoothBlend(item);
            }
        }

        public void SavePreset()
        {
            if (preset)
            {
                preset.blendShapes = new float[blendShapes.Length];

                for (int i = 0; i < preset.blendShapes.Length; i++)
                {
                    preset.blendShapes[i] = blendShapes[i];
                }
                Debug.Log("Preset: " + preset.name + " Saved");

                MTools.SetDirty(preset);
            }
        }

        public void LoadPreset() => LoadPreset(preset);

        public void LoadPreset(BlendShapePreset preset)
        {
            if (preset && preset.blendShapes != null && preset.blendShapes.Length > 0)
            {
                blendShapes = new float[preset.blendShapes.Length];

                for (int i = 0; i < preset.blendShapes.Length; i++)
                {
                    blendShapes[i] = preset.blendShapes[i];
                }

                Debug.Log("Preset: " + preset.name + " Loaded", this);
                UpdateBlendShapes();

                if (!Application.isPlaying) MTools.SetDirty(preset);
            }
        }

        public virtual void SetShapesCount()
        {
            if (mesh)
            {
                blendShapes = new float[mesh.sharedMesh.blendShapeCount];

                for (int i = 0; i < blendShapes.Length; i++)
                {
                    blendShapes[i] = mesh.GetBlendShapeWeight(i);
                }
            }
        }


        /// <summary>Set Random Values to the Mesh Blend Shapes</summary>
        public virtual void Randomize()
        {
            if (HasBlendShapes)
            {
                for (int i = 0; i < blendShapes.Length; i++)
                {
                    blendShapes[i] = Random.Range(Min, Max);
                    mesh.SetBlendShapeWeight(i, blendShapes[i]);
                }
                UpdateLODs();
            }
        }

        /// <summary>Set Random Values to the Mesh Blend Shapes</summary>
        public virtual void ResetToZero()
        {
            if (HasBlendShapes)
            {
                for (int i = 0; i < blendShapes.Length; i++)
                {
                    blendShapes[i] = 0;
                    mesh.SetBlendShapeWeight(i, blendShapes[i]);
                }
                UpdateLODs();
            }
        }

        /// <summary>Set a weight of a Blend Shape by its name</summary>
        public virtual void SetWeight(string name, float value)
        {
            if (HasBlendShapes)
            {
                PinnedShape = mesh.sharedMesh.GetBlendShapeIndex(name);
                if (PinnedShape != -1)
                {
                    mesh.SetBlendShapeWeight(PinnedShape, value);
                }
            }
        }

        /// <summary>Set a weight of a Blend Shape by its index</summary>
        public virtual void SetWeight(int index, float value)
        {
            if (HasBlendShapes)
                mesh.SetBlendShapeWeight(PinnedShape = index, value);
        }

        public virtual void _PinShape(string name) => PinnedShape = mesh.sharedMesh.GetBlendShapeIndex(name);

        public virtual void _PinShape(int index) => PinnedShape = index;

        public virtual void _PinnedShapeSetValue(float value)
        {
            if (PinnedShape != -1)
            {
                value = Mathf.Clamp(value, 0, 100);
                blendShapes[PinnedShape] = value;
                mesh.SetBlendShapeWeight(PinnedShape, value);
                UpdateLODs(PinnedShape);
            }
        }


        public virtual void UpdateBlendShapes()
        {
            if (mesh && blendShapes != null)
            {
                int Length = Mathf.Min(mesh.sharedMesh.blendShapeCount, blendShapes.Length);

                for (int i = 0; i < Length; i++)
                {
                    mesh.SetBlendShapeWeight(i, blendShapes[i]);
                }

                UpdateLODs();
            }
        }

        /// <summary>Update the LODs Values</summary>
        protected virtual void UpdateLODs()
        {
            for (int i = 0; i < blendShapes.Length; i++)
            {
                UpdateLODs(i);
            }
        }

        /// <summary>Updates Only a Shape in all LODS
        protected virtual void UpdateLODs(int index)
        {
            if (LODs != null)
            {
                foreach (var lods in LODs)
                {
                    if (lods != null && lods.sharedMesh.blendShapeCount > index)
                        lods.SetBlendShapeWeight(index, blendShapes[index]);
                }
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Create Event Listeners")]
        void CreateListeners()
        {

            MEventListener listener =
                (this.FindComponent<MEventListener>()
                ?? transform.root.GetComponentInChildren<MEventListener>())
                ?? transform.root.gameObject.AddComponent<MEventListener>();
            listener.Events ??= new List<MEventItemListener>();

            MEvent BlendS = MTools.GetInstance<MEvent>("Blend Shapes");


            if (listener.Events.Find(item => item.Event == BlendS) == null)
            {
                var item = new MEventItemListener()
                {
                    Event = BlendS,
                    useVoid = false,
                    useString = true,
                    useInt = true,
                    useFloat = true
                };

                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseInt, _PinShape);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseString, _PinShape);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(item.ResponseFloat, _PinnedShapeSetValue);
                listener.Events.Add(item);

                Debug.Log("<B>Blend Shapes</B> Added to the Event Listeners");

                MTools.SetDirty(listener);
            }
        }
#endif
    }
}