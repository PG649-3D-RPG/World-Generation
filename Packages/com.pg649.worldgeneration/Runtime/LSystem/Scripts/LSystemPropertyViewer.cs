using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LSystem {
    public class LSystemPropertyViewer : MonoBehaviour
    {
        public float m_Distance;
        public short m_Angle;
        public INITIAL_DIRECTION m_InitialDirection;
        public float m_Thickness;
        public uint m_CrossSections;
        public uint m_CrossSectionDivisions;

        public bool m_TranslatePoints;
        public string m_StartString;
        public uint m_Iterations;


        public string[] m_Rules;

        public void Populate(LSystemProperties properties)
        {
            m_Distance = properties.distance;
            m_Angle = properties.angle;
            m_InitialDirection = properties.initialDirection;
            m_Thickness = properties.thickness;
            m_CrossSections = properties.crossSections;
            m_CrossSectionDivisions = properties.crossSectionDivisions;
            m_TranslatePoints = properties.translatePoints;
            m_StartString = properties.startString;
            m_Iterations = properties.iterations;
            m_Rules = properties.rules;

        }
    }
}