using UnityEngine;

public class EnvironmentGeneratorEditor : MonoBehaviour
{
    private EnvironmentGenerator generator;
    public EnvironmentGeneratorSettings GeneratorSettings;

    public void Build()
    {
        Terrain terrain = GetComponent<Terrain>();
        generator = new EnvironmentGenerator(ref terrain, GeneratorSettings);
        generator.Build();
    }
    public void ShowZone(ZONES zone)
    {
        generator.ShowZone(zone);
    }
    public void RemoveZone(ZONES zone)
    {
        generator.RemoveZone(zone);
    }
}
