using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PerlinGenerator))]
public class TerrainGenerator : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PerlinGenerator gen = (PerlinGenerator)target;
        if (GUILayout.Button("Build Terrain"))
        {
            gen.BuildTerrain();
        }
    }
}