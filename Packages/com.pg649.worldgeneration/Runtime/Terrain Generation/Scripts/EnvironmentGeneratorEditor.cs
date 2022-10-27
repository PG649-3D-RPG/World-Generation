using UnityEngine;

public class EnvironmentGeneratorEditor : MonoBehaviour {
    private EnvironmentGeneratorBitmap generator;
    public EnvironmentGeneratorSettings GeneratorSettings;

    public void Build() {
        int size = 2048;
        bool[,] bitmap = new bool[size, size];

        for (int i = 500; i < 1000; i++) {
            for (int j = 1000; j < 2000; j++) {
                bitmap[i, j] = true;
            }
        }
        for (int i = 1000; i < 1200; i++) {
            for (int j = 1400; j < 1600; j++) {
                bitmap[i, j] = true;
            }
        }
        for (int i = 1200; i < 1700; i++) {
            for (int j = 1000; j < 2000; j++) {
                bitmap[i, j] = true;
            }
        }

        generator = new EnvironmentGeneratorBitmap(bitmap);
        generator.Build();
    }
    public void ShowZone(ZONES zone) {
        // generator.ShowZone(zone);
    }
    public void RemoveZone(ZONES zone) {
        // generator.RemoveZone(zone);
    }
}
