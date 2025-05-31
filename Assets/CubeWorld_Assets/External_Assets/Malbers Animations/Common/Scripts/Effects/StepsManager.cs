using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations
{
    /// <summary> This will manage the steps sounds and tracks for each animal, on each feet there's a Script StepTriger (Basic)  </summary>
    [AddComponentMenu("Malbers/Utilities/Effects - Audio/Step Manager")]
    public class StepsManager : MonoBehaviour, IAnimatorListener
    {
        [Tooltip("Enable Disable the Steps Manager")]
        public bool Active = true;
        [Tooltip("Time to wait to create a new track")]
        public float WaitNextStep = 0.2f;
        [Tooltip("Layer Mask used to find the ground")]
        public LayerReference GroundLayer = new(1);
        [Tooltip("Global Particle System for the Tracks, to have more individual tracks ")]
        public ParticleSystem Tracks;
        private ParticleSystem Instance;
        [Tooltip("Particle System for the Dust")]
        public ParticleSystem Dust;
        [Tooltip("This will instantiate a gameObject instead of using the Particle system")]
        public bool instantiateTracks = false;
        [Tooltip("Create Foot Tracks Particles only on Static GameObjects")]
        public bool GroundIsStatic = false;
        public float StepsVolume = 0.2f;
        public int DustParticles = 30;

        [Tooltip("Scale of the dust and track particles")]
        public Vector3 Scale = Vector3.one;

        [Tooltip("Sounds to play when the animal creates a track")]
        public AudioClipReference sounds;
        [Tooltip("Distance to Instantiate the tracks on a terrain")]
        public float trackOffset = 0.0085f;

        [Tooltip("Tracks will be on only when the character is on any of these tats")]
        public List<StateID> TracksOnlyOnState;
        public bool InTrackState { get; set; }

        public List<StepTrigger> Feet { get; set; }

        protected ICharacterAction character;

        protected virtual void Awake()
        {
            if (Tracks != null)
            {
                if (Tracks.gameObject.IsPrefab())
                {
                    Instance = Instantiate(Tracks, transform, false); //Instantiate in case the Track is a refab
                }
                else
                {
                    Instance = Tracks;
                }
                Instance.transform.localScale = Scale;
            }


            InTrackState = true;

            character = GetComponentInParent<ICharacterAction>();
        }

        protected virtual void OnEnable()
        {
            if (character != null && TracksOnlyOnState != null) character.OnState += StateChange;

        }

        protected virtual void OnDisable()
        {
            if (character != null && TracksOnlyOnState != null) character.OnState -= StateChange;
        }

        protected virtual void StateChange(int obj)
        {
            if (Feet == null) return;

            InTrackState = (TracksOnlyOnState.Find(x => x.ID == obj));

            foreach (var track in Feet)
            {
                track.gameObject.SetActive(InTrackState);
            }
        }


        //Is Called by any of the "StepTrigger" Script on a feet when they collide with the ground.
        public virtual void EnterStep(StepTrigger foot, Collider surface)
        {
            if (!Active) return;
            if (!InTrackState) return; //The character is not on a locomotion or idle

            if (Dust && Dust.gameObject.IsPrefab())
            {
                Dust = Instantiate(Dust, transform, false);             //If is a prefab clone it!
                Dust.transform.localScale = Scale;
            }

            if (foot.StepAudio && foot.StepAudio.enabled && !sounds.NullOrEmpty())    //If the track has an AudioSource Component and whe have some audio to play
            {
                sounds.Play(foot.StepAudio);
            }


            var Ray = new Ray(foot.transform.position, -transform.up);

            if (surface.Raycast(Ray, out RaycastHit hit, 1))
            {
                var TrackPosition = foot.transform.position; TrackPosition.y += trackOffset;
                var TrackRotation = (Quaternion.FromToRotation(-foot.transform.forward, hit.normal) * foot.transform.rotation);

                if (Dust)
                {
                    Dust.transform.SetPositionAndRotation(TrackPosition, TrackRotation);
                    Dust.transform.Rotate(-90, 0, 0);
                    Dust.Emit(DustParticles);
                }

                if (Instance)
                {
                    ParticleSystem.EmitParams ptrack = new ParticleSystem.EmitParams
                    {
                        rotation3D = TrackRotation.eulerAngles, //Set The Rotation
                        position = TrackPosition, //Set The Position
                    };

                    if (instantiateTracks)
                    {
                        if ((GroundIsStatic && surface.gameObject.isStatic))
                        {
                            Instance.Emit(ptrack, 1);
                        }
                        else
                        {
                            var newtrack = Instantiate(Instance); //Instantiate in case the Track is a refab

                            var ParentFixer = newtrack.transform.SetParentScaleFixer(hit.transform, TrackPosition);


                            ParticleSystem.EmitParams tr = new()
                            {
                                rotation3D = TrackRotation.eulerAngles, //Set The Rotation
                                position = Vector3.zero, //Set The Position
                            };

                            var main = newtrack.main;
                            main.simulationSpace = ParticleSystemSimulationSpace.Local;
                            newtrack.Emit(tr, 1);
                            this.Delay_Action(() => newtrack.isPlaying, () =>
                            {
                                if (ParentFixer != null) Destroy(ParentFixer.gameObject);
                            });
                        }
                    }
                    else
                    {
                        Instance.Emit(ptrack, 1);
                    }
                }
            }
        }

        /// <summary>Disable this sfcript, e.g.. deactivate when is sleeping or death </summary>
        public virtual void EnableSteps(bool value) => Active = value;

        public virtual bool OnAnimatorBehaviourMessage(string message, object value) => this.InvokeWithParams(message, value);
    }
}
