using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnvironmentGenerator))]
public class TerrainGenerator : Editor
{
    private ZONES? activeZone = null;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EnvironmentGenerator gen = (EnvironmentGenerator)target;
        if (GUILayout.Button("Build Terrain"))
        {
            if (activeZone.HasValue)
            {
                gen.RemoveZone(activeZone.Value);
                activeZone = null;
            }
            gen.Build();
        }

        if (GUILayout.Button("Show/Hide Border Zone"))
        {
            if (activeZone.HasValue && activeZone.Value == ZONES.BORDERS)
            {
                gen.RemoveZone(ZONES.BORDERS);
                activeZone = null;
            }
            else if (activeZone.HasValue)
            {
                // remove current zone
                gen.RemoveZone(activeZone.Value);
                // show border zone
                gen.ShowZone(ZONES.BORDERS);
                activeZone = ZONES.BORDERS;
            }
            else
            {
                gen.ShowZone(ZONES.BORDERS);
                activeZone = ZONES.BORDERS;
            }
        }

        if (GUILayout.Button("Show/Hide Obstacle Zone"))
        {
            if (activeZone.HasValue && activeZone.Value == ZONES.OBSTACLES)
            {
                gen.RemoveZone(ZONES.OBSTACLES);
                activeZone = null;
            }
            else if (activeZone.HasValue)
            {
                // remove current zone
                gen.RemoveZone(activeZone.Value);
                // show obstacle zone
                gen.ShowZone(ZONES.OBSTACLES);
                activeZone = ZONES.OBSTACLES;
            }
            else
            {
                gen.ShowZone(ZONES.OBSTACLES);
                activeZone = ZONES.OBSTACLES;
            }
        }
    }
}