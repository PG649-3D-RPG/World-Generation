using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnvironmentGeneratorEditor))]
public class TerrainGenerator : Editor {
    // private ZONES? activeZone = null;
    private EnvironmentGeneratorEditor gen;
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        gen = (EnvironmentGeneratorEditor) target;

        if (GUILayout.Button("Build Terrain")) {
            // if (activeZone.HasValue) {
            //     gen.RemoveZone(activeZone.Value);
            //     activeZone = null;
            // }
            gen.Build();
        }

        // if (GUILayout.Button("Show/Hide Free space")) {
        //     Show(ZONES.FREE);
        // }

        // if (GUILayout.Button("Show/Hide Used space")) {
        //     Show(ZONES.USED);
        // }

        // if (GUILayout.Button("Show/Hide Border Zone")) {
        //     Show(ZONES.BORDERS);
        // }

        // if (GUILayout.Button("Show/Hide Obstacle Zone")) {
        //     Show(ZONES.OBSTACLES);
        // }

    }

    // private void Show(ZONES zone) {
    //     if (activeZone.HasValue && activeZone.Value == zone) {
    //         gen.RemoveZone(zone);
    //         activeZone = null;
    //     }
    //     else if (activeZone.HasValue) {
    //         // remove current zone
    //         gen.RemoveZone(activeZone.Value);
    //         // show selected zone
    //         gen.ShowZone(zone);
    //         activeZone = zone;
    //     }
    //     else {
    //         gen.ShowZone(zone);
    //         activeZone = zone;
    //     }
    // }
}
