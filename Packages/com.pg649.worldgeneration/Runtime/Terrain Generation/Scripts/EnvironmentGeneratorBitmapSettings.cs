using UnityEngine;

[CreateAssetMenu(fileName = "EnvironmentGeneratorBitmapSettings", menuName = "PG649-WorldGeneration/Environment Generator Bitmap Settings")]
public class EnvironmentGeneratorBitmapSettings : ScriptableObject {
    [Header("General settings")]
    [Space(10)]
    public float Depth = 100f;

    [Header("Perlin settings")]
    [Space(10)]
    public float AMPLITUDE = .8f;
    public float FREQUENCY = 192f;

    [Header("Blur settings")]
    [Space(10)]
    [SerializeField]
    public ComputeShader gaussBlurShader;
    public int BlurringPasses = 10;
    public int BlurRadius = 10;
}
