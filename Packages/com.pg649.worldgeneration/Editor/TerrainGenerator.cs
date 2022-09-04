using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PerlinGenerator))]
public class TerrainGenerator : Editor
{
    private bool showBorderZone = false;
    private bool showObstacleZone = false;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PerlinGenerator gen = (PerlinGenerator)target;
        if (GUILayout.Button("Build Terrain"))
        {
            gen.BuildTerrain();
        }
        if (GUILayout.Button("Show/Hide Border Zone"))
        {
            if (!showBorderZone)
            {
                gen.ShowBorderZone();
                showBorderZone = true;
            }
            else
            {
                gen.RemoveBorderZoneLayer();
                showBorderZone = false;
            }
        }
        if (GUILayout.Button("Show/Hide Obstacle Zone"))
        {
            if (!showObstacleZone)
            {
                gen.ShowObstacleZone();
                showObstacleZone = true;
            }
            else
            {
                gen.RemoveObstacleZone();
                showObstacleZone = false;
            }
        }
    }
}