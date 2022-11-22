using UnityEditor;
using UnityEngine;

namespace LSystem
{
    [CustomEditor(typeof(LSystemPropertyViewer))]
    public class LSystemPropertyViewerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            LSystemPropertyViewer prop = (LSystemPropertyViewer)target;

            GUI.enabled = false;

            EditorGUILayout.LabelField("Default Settings", EditorStyles.boldLabel);
            EditorGUILayout.FloatField("Distance", prop.m_Distance);

            EditorGUILayout.IntField("Angle", prop.m_Angle);
            EditorGUILayout.EnumFlagsField("Initial Direction", prop.m_InitialDirection);
            EditorGUILayout.FloatField("Thickness", prop.m_Thickness);
            EditorGUILayout.LongField("Cross Sections", prop.m_CrossSections);
            EditorGUILayout.LongField("Cross Section Divisions", prop.m_CrossSectionDivisions);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("L-System", EditorStyles.boldLabel);
            EditorGUILayout.Toggle("Translate Points?", prop.m_TranslatePoints);
            EditorGUILayout.TextField("Start String", prop.m_StartString);
            EditorGUILayout.LongField("Number of Generations", prop.m_Iterations);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rules", EditorStyles.boldLabel);
            foreach (var r in prop.m_Rules)
            {
                EditorGUILayout.TextField(r);
            }

            GUI.enabled = true;
        }
    }
}