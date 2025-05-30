using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SoftKitty.PCW
{
    [DefaultExecutionOrder(-100)]
    public class InterpolationController : MonoBehaviour
    {

        private float[] m_lastFixedUpdateTimes;
        private int m_newTimeIndex;

        private static float m_interpolationFactor;
        public static float InterpolationFactor
        {
            get { return m_interpolationFactor; }
        }

        public void Start()
        {
            m_lastFixedUpdateTimes = new float[2];
            m_newTimeIndex = 0;
        }

        public void FixedUpdate()
        {
            m_newTimeIndex = OldTimeIndex();
            m_lastFixedUpdateTimes[m_newTimeIndex] = Time.fixedTime;
        }

        public void Update()
        {
            float newerTime = m_lastFixedUpdateTimes[m_newTimeIndex];
            float olderTime = m_lastFixedUpdateTimes[OldTimeIndex()];

            if (newerTime != olderTime)
            {
                m_interpolationFactor = (Time.time - newerTime) / (newerTime - olderTime);
            }
            else
            {
                m_interpolationFactor = 1;
            }
        }

        private int OldTimeIndex()
        {
            return (m_newTimeIndex == 0 ? 1 : 0);
        }
    }
}