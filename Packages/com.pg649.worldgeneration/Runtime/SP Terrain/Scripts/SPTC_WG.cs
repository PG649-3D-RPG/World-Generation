using UnityEngine;

public class SPTC_WG : MonoBehaviour {
    public WorldGeneratorSettings settings;
    void Awake() {
        var terrain = WorldGenerator.Generate(settings);
    }
}
