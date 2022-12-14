using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
// using Unity.AI.Navigation;

public class EnvironmentGenerator {
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
    private List<Tuple<DungeonRoom.Face, int, int>> corridorPoints;

    bool IsPowerOfTwo(int x) => (x != 0) && ((x & (x - 1)) == 0); // from https://stackoverflow.com/a/600306
    public EnvironmentGenerator(ref Terrain terrain, EnvironmentGeneratorSettings generatorSettings, List<Tuple<DungeonRoom.Face, int, int>> cp = null) {
        settings = generatorSettings;
        corridorPoints = cp;
        //if (!IsPowerOfTwo(settings.TerrainSize)) throw new System.ArgumentException("TerrainSize must be a power of 2");
        Terrain = terrain;
    }

    public void Build() {
        OffsetX = Random.Range(0, 9999);
        OffsetY = Random.Range(0, 9999);
        if (settings.UseRandomSeed) Random.InitState(settings.RandomSeed);

        RegenerateTerrain();
    }

    public void ShowZone(ZONES zone) {
        // switch (zone)
        // {
        //     case ZONES.BORDERS:
        //         var borderLayer = ZoneManager.ShowZone(settings.TerrainSize, BorderZone, Color.yellow);
        //         AddTerrainLayer(Terrain.terrainData, borderLayer, "border");
        //         break;
        //     case ZONES.OBSTACLES:
        //         var obstacleLayer = ZoneManager.ShowZone(settings.TerrainSize, ObstacleZone, Color.green);
        //         AddTerrainLayer(Terrain.terrainData, obstacleLayer, "obstacles");
        //         break;
        //     case ZONES.FREE:
        //         var freeLayer = ZoneManager.ShowZone(settings.TerrainSize, FreeSpace, Color.white);
        //         AddTerrainLayer(Terrain.terrainData, freeLayer, "free");
        //         break;
        //     case ZONES.USED:
        //         var usedLayer = ZoneManager.ShowZone(settings.TerrainSize, UsedSpace, Color.red);
        //         AddTerrainLayer(Terrain.terrainData, usedLayer, "used");
        //         break;
        //     default:
        //         break;
        // }
    }
    public void RemoveZone(ZONES zone) {
        // switch (zone)
        // {
        //     case ZONES.BORDERS:
        //         RemoveTerrainLayer(Terrain.terrainData, "border");
        //         break;
        //     case ZONES.OBSTACLES:
        //         RemoveTerrainLayer(Terrain.terrainData, "obstacles");
        //         break;
        //     case ZONES.FREE:
        //         RemoveTerrainLayer(Terrain.terrainData, "free");
        //         break;
        //     case ZONES.USED:
        //         RemoveTerrainLayer(Terrain.terrainData, "used");
        //         break;
        //     default:
        //         break;
        // }
    }

    /// <summary>
    /// Gets height (y) of terrain at Vector x.
    /// </summary>
    /// <param name="position">Position vector of object on terrain</param>
    /// <returns></returns>
    public float GetTerrainHeight(Vector3 position) {
        return Terrain.SampleHeight(position);
    }
    private int ToNextPowerOfTwo(int i) {
        // if i == 0 or i==zweierpotenz return i
        if (IsPowerOfTwo(i)) return i;
        // shift bits until value==1
        int exp = 1;
        int n = Mathf.Abs(i);
        while (n > 1) {
            n >>= 1;
            exp++;
        }
        return 0b1 << exp;
    }

    private void RegenerateTerrain() {
        // clear all runtime containers
        foreach (var layerName in CustomTerrainLayerIndices.Keys) {
            RemoveTerrainLayer(Terrain.terrainData, layerName);
        }
        CustomTerrainLayerIndices.Clear();

        BorderZone = new bool[settings.TerrainSizeY, settings.TerrainSizeX];
        ObstacleZone = new bool[settings.TerrainSizeY, settings.TerrainSizeX];

        FreeSpace = new bool[settings.TerrainSizeY, settings.TerrainSizeX];
        UsedSpace = new bool[settings.TerrainSizeY, settings.TerrainSizeX];

        // generate new basic terrain
        Terrain.terrainData = new TerrainData();

        Terrain.terrainData.heightmapResolution = ToNextPowerOfTwo(Mathf.Max(settings.TerrainSizeX, settings.TerrainSizeY)) + 1;
        var heights = new float[Terrain.terrainData.heightmapResolution, Terrain.terrainData.heightmapResolution];

        // create necessary new Generators
        BorderGenerator = new BorderGenerator(heights.GetLength(1), heights.GetLength(0), settings.Scale, OffsetX, OffsetY, settings.MinBorderSize, settings.MaxBorderSize, settings.UseSmoothing, settings.SmoothPasses, settings.SmoothRadius, settings.StrongerSmoothing);

        // Generate Environment
        if (settings.GenerateHeights) {
            PerlinGenerator perlinGenerator = new PerlinGenerator(settings.Scale, OffsetX, OffsetY);
            perlinGenerator.GenerateTerrain(ref heights);
        }
        if (settings.GenerateBorders) {
            bool[,] safeZone = new bool[heights.GetLength(1), heights.GetLength(0)];
            // for (int i = 0; i < settings.MaxBorderSize; i++)
            // {
            //     for (int j = 0; j < settings.MaxBorderSize; j++)
            //     {
            //         safeZone[0 + i, j] = true;
            //     }
            // }
            if (corridorPoints != null) {
                foreach (var (face, offset, width) in corridorPoints) {
                    // face,offset,width
                    Debug.Log("TerrainSize: " + settings.TerrainSizeX + ", " + settings.TerrainSizeY);
                    Debug.Log("HeightmapRes: " + (ToNextPowerOfTwo(Mathf.Max(settings.TerrainSizeX, settings.TerrainSizeY)) + 1) + ", " + Terrain.terrainData.heightmapResolution);
                    Debug.Log("Room size: (" + heights.GetLength(1) + ", " + heights.GetLength(0) + ")");
                    Debug.Log("Corridor: (" + face + ", " + offset + ", " + width + ")");
                    switch (face) {
                        case DungeonRoom.Face.Left:
                            for (int y = safeZone.GetLength(0) - offset - width; y < safeZone.GetLength(0) - offset; y++) {
                                for (int x = 0; x < settings.MaxBorderSize; x++) {
                                    safeZone[y, x] = true;
                                }
                            }
                            break;
                        case DungeonRoom.Face.Right:
                            for (int y = safeZone.GetLength(0) - offset - width; y < safeZone.GetLength(0) - offset; y++) {
                                for (int x = heights.GetLength(1) - settings.MaxBorderSize; x < settings.MaxBorderSize; x++) {
                                    safeZone[y, x] = true;
                                }
                            }
                            break;
                        case DungeonRoom.Face.Front:
                            for (int x = safeZone.GetLength(1) - offset - width; x < safeZone.GetLength(1) - offset; x++) {
                                for (int y = 0; y < settings.MaxBorderSize; y++) {
                                    safeZone[y, x] = true;
                                }
                            }
                            break;
                        case DungeonRoom.Face.Back:
                            for (int x = safeZone.GetLength(1) - offset - width; x < safeZone.GetLength(1) - offset; x++) {
                                for (int y = heights.GetLength(0) - settings.MaxBorderSize; y < settings.MaxBorderSize; y++) {
                                    safeZone[y, x] = true;
                                }
                            }
                            break;
                        case DungeonRoom.Face.Top:
                            //useless
                            break;
                        case DungeonRoom.Face.Bottom:
                            //useless
                            break;
                    }
                }
            }
            BorderGenerator.SetBorderSafeZone(safeZone);
            BorderGenerator.GenerateBorders(ref heights);
            BorderZone = BorderGenerator.GetBorderZone();
        }
        // if (settings.GenerateObstacles)
        // {
        //     ObstacleGenerator obstacleGenerator = new ObstacleGenerator(settings.TerrainSize, settings.Scale, settings.NumberOfObstacles, settings.ObstacleSize, settings.ObstacleSize, settings.ObstaclePadding, BorderGenerator);
        //     obstacleGenerator.GenerateObstacles(ref heights);
        //     ObstacleZone = obstacleGenerator.GetObstacleZone();
        // }

        //Terrain.terrainData.size = new Vector3(settings.TerrainSize, settings.Depth, settings.TerrainSize);
        Terrain.terrainData.SetHeights(0, 0, heights);

        Terrain.terrainData.size = new Vector3(settings.TerrainSizeX, settings.Depth, settings.TerrainSizeY);
        // Terrain.Flush();
        // Terrain.terrainData.size = new Vector3(settings.TerrainSize, settings.Depth, settings.TerrainSize);

        Terrain.Flush();

        // UpdateFreeAndUsedSpace();
    }

    public void UpdateFreeAndUsedSpace() {
        // naively calculate used and unused spaces
        for (int x = 0; x < settings.TerrainSizeX; x++) {
            for (int y = 0; y < settings.TerrainSizeY; y++) {
                FreeSpace[y, x] = !(BorderZone[y, x] || ObstacleZone[y, x]);
                UsedSpace[y, x] = BorderZone[y, x] || ObstacleZone[y, x];
            }
        }
    }

    /// <summary>
    /// Adds new <see cref="TerrainLayer"/> to the given <see cref="TerrainData"/> object.
    /// </summary>
    /// <param name="terrainData"><see cref="TerrainData"/> to add layer to.</param>
    /// <param name="inputLayer"><see cref="TerrainLayer"/> to add.</param>
    private void AddTerrainLayer(TerrainData terrainData, TerrainLayer inputLayer, string layerName) {
        if (inputLayer == null)
            return;

        var layers = terrainData.terrainLayers;
        for (var idx = 0; idx < layers.Length; ++idx) {
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

    private void RemoveTerrainLayer(TerrainData terrainData, string layerName) {
        if (CustomTerrainLayerIndices.TryGetValue(layerName, out int layernum)) {
            var oldLayers = terrainData.terrainLayers;
            var newLayers = new TerrainLayer[oldLayers.Length - 1];
            for (int i = 0; i < oldLayers.Length; i++) {
                if (i == layernum) continue;
                newLayers[i] = oldLayers[i];
            }
            terrainData.terrainLayers = newLayers;
            CustomTerrainLayerIndices.Remove(layerName);
        }
    }
}
