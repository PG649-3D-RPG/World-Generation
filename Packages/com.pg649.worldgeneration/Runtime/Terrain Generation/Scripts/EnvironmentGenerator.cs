using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
// using Unity.AI.Navigation;

public class EnvironmentGenerator
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

    private EnvironmentGeneratorSettings settings;

    bool IsPowerOfTwo(int x) => (x != 0) && ((x & (x - 1)) == 0); // from https://stackoverflow.com/a/600306
    public EnvironmentGenerator(ref Terrain terrain, EnvironmentGeneratorSettings generatorSettings)
    {
        settings = generatorSettings;
        if (!IsPowerOfTwo(settings.TerrainSize)) throw new System.ArgumentException("TerrainSize must be a power of 2");
        Terrain = terrain;
    }

    public void Build()
    {
        OffsetX = Random.Range(0, 9999);
        OffsetY = Random.Range(0, 9999);
        if (settings.UseRandomSeed) Random.InitState(settings.RandomSeed);

        RegenerateTerrain();
    }

    public void ShowZone(ZONES zone)
    {
        switch (zone)
        {
            case ZONES.BORDERS:
                var borderLayer = ZoneManager.ShowZone(settings.TerrainSize, BorderZone, Color.yellow);
                AddTerrainLayer(Terrain.terrainData, borderLayer, "border");
                break;
            case ZONES.OBSTACLES:
                var obstacleLayer = ZoneManager.ShowZone(settings.TerrainSize, ObstacleZone, Color.green);
                AddTerrainLayer(Terrain.terrainData, obstacleLayer, "obstacles");
                break;
            case ZONES.FREE:
                var freeLayer = ZoneManager.ShowZone(settings.TerrainSize, FreeSpace, Color.white);
                AddTerrainLayer(Terrain.terrainData, freeLayer, "free");
                break;
            case ZONES.USED:
                var usedLayer = ZoneManager.ShowZone(settings.TerrainSize, UsedSpace, Color.red);
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

        BorderZone = new bool[settings.TerrainSize, settings.TerrainSize];
        ObstacleZone = new bool[settings.TerrainSize, settings.TerrainSize];

        FreeSpace = new bool[settings.TerrainSize, settings.TerrainSize];
        UsedSpace = new bool[settings.TerrainSize, settings.TerrainSize];

        // generate new basic terrain
        Terrain.terrainData.heightmapResolution = settings.TerrainSize + 1;
        Terrain.terrainData.size = new Vector3(settings.TerrainSize, settings.Depth, settings.TerrainSize);
        var heights = new float[settings.TerrainSize, settings.TerrainSize];
        Terrain.terrainData.SetHeights(0, 0, heights);

        // create necessary new Generators
        BorderGenerator = new BorderGenerator(settings.TerrainSize, settings.Scale, OffsetX, OffsetY, settings.MinBorderSize, settings.MaxBorderSize, settings.UseSmoothing, settings.SmoothPasses, settings.SmoothRadius, settings.StrongerSmoothing);

        // Generate Environment
        if (settings.GenerateHeights)
        {
            PerlinGenerator perlinGenerator = new PerlinGenerator(settings.TerrainSize, settings.Scale, OffsetX, OffsetY);
            Terrain.terrainData = perlinGenerator.GenerateTerrain(Terrain.terrainData);
        }
        if (settings.GenerateBorders)
        {
            //bool[,] safeZone = new bool[settings.TerrainSize, settings.TerrainSize];
            //for (int i = 0; i < settings.MaxBorderSize; i++)
            //{
            //    for (int j = 0; j < settings.MaxBorderSize; j++)
            //    {
            //        safeZone[100 + i, j] = true;
            //    }
            //}
            //BorderGenerator.SetBorderSafeZone(safeZone);
            Terrain.terrainData = BorderGenerator.GenerateBorders(Terrain.terrainData);
            BorderZone = BorderGenerator.GetBorderZone();
        }
        if (settings.GenerateObstacles)
        {
            ObstacleGenerator obstacleGenerator = new ObstacleGenerator(settings.TerrainSize, settings.Scale, settings.NumberOfObstacles, settings.ObstacleSize, settings.ObstacleSize, settings.ObstaclePadding, BorderGenerator);
            Terrain.terrainData = obstacleGenerator.GenerateObstacles(Terrain.terrainData);
            ObstacleZone = obstacleGenerator.GetObstacleZone();
        }
        Terrain.Flush();

        UpdateFreeAndUsedSpace();
    }

    public void UpdateFreeAndUsedSpace()
    {
        // naively calculate used and unused spaces
        for (int x = 0; x < settings.TerrainSize; x++)
        {
            for (int y = 0; y < settings.TerrainSize; y++)
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
