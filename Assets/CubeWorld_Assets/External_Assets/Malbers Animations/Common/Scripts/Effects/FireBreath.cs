using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Utilities/Effects - Audio/FireBreath")]
    public class FireBreath : MonoBehaviour
    {
        public bool onStart = false;
        public float rateOverTime = 500f;
        public ParticleSystem[] m_Particles;

        private bool currentState;

        void Awake()
        {
            if (m_Particles == null)
                m_Particles = GetComponentsInChildren<ParticleSystem>();

            if (m_Particles != null && m_Particles.Length > 0)
            {
                foreach (var p in m_Particles)
                {
                    var emission = p.emission;
                    emission.rateOverTime = new ParticleSystem.MinMaxCurve(0);
                }
            }
            else
            {
                Destroy(this);
            }
        }

        private void OnEnable()
        {
            if (onStart) Activate(true);
        }

        public void Activate(bool value)
        {
            if (currentState != value)
            {
                currentState = value;

                foreach (var p in m_Particles)
                {
                    var emission = p.emission;
                    emission.rateOverTime = new ParticleSystem.MinMaxCurve(value ? rateOverTime : 0);
                }
            }
        }


        public void FireBreathColor(Color newcolor)
        {
            foreach (var p in m_Particles)
            {
                var particle = p.main;
                particle.startColor = new ParticleSystem.MinMaxGradient(newcolor);
            }
        }
        public void FireBreathColor(ColorVar newcolor) => FireBreathColor(newcolor.Value);
    }
}
