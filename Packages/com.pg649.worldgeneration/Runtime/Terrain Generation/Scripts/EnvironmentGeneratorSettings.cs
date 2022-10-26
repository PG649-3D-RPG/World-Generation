using UnityEngine;

[CreateAssetMenu(fileName = "EnvironmentGeneratorSettings", menuName = "PG649-WorldGeneration/Environment Generator Settings")]
public class EnvironmentGeneratorSettings : ScriptableObject
{
    [Header("General settings")]
    [Space(10)]
    public bool UseRandomSeed = false;
    public int RandomSeed = 42;


    [Header("Terrain settings")]
    [Space(10)]
    // [Tooltip("Must be 2^n, creates always a square terrain")]
    public int TerrainSizeX = 256;
    public int TerrainSizeY = 256;
    public bool GenerateHeights = true;
    public float Depth = 10;
    public float Scale = 1;//2.5f;


    [Header("Border settings")]
    [Space(10)]
    public bool GenerateBorders = true;
    public int MaxBorderSize => Mathf.Min(12, Mathf.Max(TerrainSizeX, TerrainSizeY) / 8);
    public int MinBorderSize => Mathf.Max(Mathf.Min(6, Mathf.Max(TerrainSizeX, TerrainSizeY) / 10), 2);
    public bool UseSmoothing = true;
    public bool StrongerSmoothing = false;
    public int SmoothRadius = 4;
    public int SmoothPasses = 1;

    [Header("Obstacle settings")]
    [Space(10)]
    public bool GenerateObstacles = true;
    public int ObstacleSize = 10;
    public int NumberOfObstacles = 5;
    public int ObstaclePadding = 10;

    [Header("Plant settings")]
    [Space(10)]
    public bool GeneratePlants = false;

    // public EnvironmentGeneratorSettings() { }

    // public EnvironmentGeneratorSettings(bool useRandomSeed, int randomSeed, int terrainSize, bool generateHeights, float depth, float scale, bool generateBorders, int maxBorderSize, int minBorderSize, bool useSmoothing, bool strongerSmoothing, int smoothRadius, int smoothPasses, bool generateObstacles, int obstacleSize, int numberOfObstacles, int obstaclePadding, bool generatePlants)
    // {
    //     UseRandomSeed = useRandomSeed;
    //     RandomSeed = randomSeed;
    //     TerrainSizeX = terrainSizeX;
    //     TerrainSizeY = terrainSizeY;
    //     GenerateHeights = generateHeights;
    //     Depth = depth;
    //     Scale = scale;
    //     GenerateBorders = generateBorders;
    //     UseSmoothing = useSmoothing;
    //     StrongerSmoothing = strongerSmoothing;
    //     SmoothRadius = smoothRadius;
    //     SmoothPasses = smoothPasses;
    //     GenerateObstacles = generateObstacles;
    //     ObstacleSize = obstacleSize;
    //     NumberOfObstacles = numberOfObstacles;
    //     ObstaclePadding = obstaclePadding;
    //     GeneratePlants = generatePlants;
    // }
}