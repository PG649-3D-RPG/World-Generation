using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
// using Unity.AI.Navigation;

public class EnvironmentGenerator : MonoBehaviour
{
    private Terrain Terrain { get; set; }
    private BorderGenerator BorderGenerator { get; set; }
    private int OffsetX { get; set; } = 0;
    private int OffsetY { get; set; } = 0;

    public bool[,] BorderZone;
    public bool[,] ObstacleZone;

    private bool[,] UsedSpace;
    private bool[,] FreeSpace;

    private readonly Dictionary<string, int> CustomTerrainLayerIndices = new();

    [Header("General settings")]
    [Space(10)]
    public bool UseRandomSeed = false;
    public int RandomSeed = 42;


    [Header("Terrain settings")]
    [Space(10)]
    public bool GenerateHeights = true;
    public float Depth = 10;
    public float Scale = 2.5f;
    private readonly int TerrainSize = 256; // must be 2^n


    [Header("Border settings")]
    [Space(10)]
    public bool GenerateBorders = true;
    public int MaxBorderSize = 12;
    public int MinBorderSize = 6;
    public bool UseSmoothing = true;
    public bool StrongerSmoothing = false;
    public int SmoothRadius = 4;
    public int SmoothPasses = 1;
    private int BorderPadding = 10;

    [Header("Obstacle settings")]
    [Space(10)]
    public bool GenerateObstacles = true;
    public int ObstacleSize = 10;
    public int NumberOfObstacles = 5;
    public int ObstaclePadding = 10;

    [Header("Plant settings")]
    [Space(10)]
    public bool GeneratePlants = false;

    public void Build()
    {
        // OffsetX = Random.Range(0f, 9999f);
        // OffsetY = Random.Range(0f, 9999f);
        if (UseRandomSeed) Random.InitState(RandomSeed);

        Terrain = GetComponent<Terrain>();
        RegenerateTerrain();
    }

    public void ShowZone(ZONES zone)
    {
        switch (zone)
        {
            case ZONES.BORDERS:
                var borderLayer = ZoneManager.ShowZone(TerrainSize, BorderZone, Color.yellow);
                AddTerrainLayer(Terrain.terrainData, borderLayer, "border");
                break;
            case ZONES.OBSTACLES:
                var obstacleLayer = ZoneManager.ShowZone(TerrainSize, ObstacleZone, Color.green);
                AddTerrainLayer(Terrain.terrainData, obstacleLayer, "obstacles");
                break;
            case ZONES.FREE:
                var freeLayer = ZoneManager.ShowZone(TerrainSize, FreeSpace, Color.white);
                AddTerrainLayer(Terrain.terrainData, freeLayer, "free");
                break;
            case ZONES.USED:
                var usedLayer = ZoneManager.ShowZone(TerrainSize, UsedSpace, Color.red);
                AddTerrainLayer(Terrain.terrainData, usedLayer, "used");
                break;
            default:
                break;
        }
    }
    public void RemoveZone(ZONES zone)
    {
        switch (zone)
        {
            case ZONES.BORDERS:
                RemoveTerrainLayer(Terrain.terrainData, "border");
                break;
            case ZONES.OBSTACLES:
                RemoveTerrainLayer(Terrain.terrainData, "obstacles");
                break;
            case ZONES.FREE:
                RemoveTerrainLayer(Terrain.terrainData, "free");
                break;
            case ZONES.USED:
                RemoveTerrainLayer(Terrain.terrainData, "used");
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Gets height (y) of terrain at Vector x.
    /// </summary>
    /// <param name="position">Position vector of object on terrain</param>
    /// <returns></returns>
    public float GetTerrainHeight(Vector3 position)
    {
        return Terrain.SampleHeight(position);
    }

    private void RegenerateTerrain()
    {
        // clear all runtime containers
        foreach (var layerName in CustomTerrainLayerIndices.Keys)
        {
            RemoveTerrainLayer(Terrain.terrainData, layerName);
        }
        CustomTerrainLayerIndices.Clear();

        BorderZone = new bool[TerrainSize, TerrainSize];
        ObstacleZone = new bool[TerrainSize, TerrainSize];

        FreeSpace = new bool[TerrainSize, TerrainSize];
        UsedSpace = new bool[TerrainSize, TerrainSize];

        // generate new basic terrain
        Terrain.terrainData.heightmapResolution = TerrainSize + 1;
        Terrain.terrainData.size = new Vector3(TerrainSize, Depth, TerrainSize);
        var heights = new float[TerrainSize, TerrainSize];
        Terrain.terrainData.SetHeights(0, 0, heights);

        // create necessary new Generators
        BorderGenerator = new BorderGenerator(TerrainSize, Scale, OffsetX, OffsetY, MinBorderSize, MaxBorderSize, BorderPadding, UseSmoothing, SmoothPasses, SmoothRadius, StrongerSmoothing);

        // Generate Environment
        if (GenerateHeights)
        {
            PerlinGenerator perlinGenerator = new PerlinGenerator(TerrainSize, Depth, Scale, OffsetX, OffsetY);
            Terrain.terrainData = perlinGenerator.GenerateTerrain(Terrain.terrainData);
        }
        if (GenerateBorders)
        {
            Terrain.terrainData = BorderGenerator.GenerateBorders(Terrain.terrainData);
            BorderZone = BorderGenerator.GetBorderZone();
        }
        if (GenerateObstacles)
        {
            ObstacleGenerator obstacleGenerator = new ObstacleGenerator(TerrainSize, Scale, NumberOfObstacles, ObstacleSize, ObstacleSize, ObstaclePadding, BorderGenerator);
            Terrain.terrainData = obstacleGenerator.GenerateObstacles(Terrain.terrainData);
            ObstacleZone = obstacleGenerator.GetObstacleZone();
        }
        Terrain.Flush();

        UpdateFreeAndUsedSpace();
    }

    public void UpdateFreeAndUsedSpace()
    {
        // naively calculate used and unused spaces
        for (int x = 0; x < TerrainSize; x++)
        {
            for (int y = 0; y < TerrainSize; y++)
            {
                FreeSpace[x, y] = !(BorderZone[x, y] || ObstacleZone[x, y]);
                UsedSpace[x, y] = BorderZone[x, y] || ObstacleZone[x, y];
            }
        }
    }

    /// <summary>
    /// Adds new <see cref="TerrainLayer"/> to the given <see cref="TerrainData"/> object.
    /// </summary>
    /// <param name="terrainData"><see cref="TerrainData"/> to add layer to.</param>
    /// <param name="inputLayer"><see cref="TerrainLayer"/> to add.</param>
    private void AddTerrainLayer(TerrainData terrainData, TerrainLayer inputLayer, string layerName)
    {
        if (inputLayer == null)
            return;

        var layers = terrainData.terrainLayers;
        for (var idx = 0; idx < layers.Length; ++idx)
        {
            if (layers[idx] == inputLayer)
                return;
        }

        int newIndex = layers.Length;
        var newarray = new TerrainLayer[newIndex + 1];
        System.Array.Copy(layers, 0, newarray, 0, newIndex);
        newarray[newIndex] = inputLayer;
        CustomTerrainLayerIndices[layerName] = newIndex;

        terrainData.terrainLayers = newarray;
    }

    private void RemoveTerrainLayer(TerrainData terrainData, string layerName)
    {
        if (CustomTerrainLayerIndices.TryGetValue(layerName, out int layernum))
        {
            var oldLayers = terrainData.terrainLayers;
            var newLayers = new TerrainLayer[oldLayers.Length - 1];
            for (int i = 0; i < oldLayers.Length; i++)
            {
                if (i == layernum) continue;
                newLayers[i] = oldLayers[i];
            }
            terrainData.terrainLayers = newLayers;
            CustomTerrainLayerIndices.Remove(layerName);
        }
    }
}
