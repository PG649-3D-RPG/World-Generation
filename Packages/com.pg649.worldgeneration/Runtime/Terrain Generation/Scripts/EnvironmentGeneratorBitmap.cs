using System;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentGeneratorBitmap {
    private readonly bool[,] heightmapMask;
    private readonly int size;
    private const float DEPTH = 100;
    private const float AMPLITUDE = .8f;
    private const float FREQUENCY = 256f;
    private Terrain terrain;

    public EnvironmentGeneratorBitmap(bool[,] mask) {
        heightmapMask = mask;
        size = mask.GetLength(0);
        if (!IsPowerOfTwo(size)) throw new System.ArgumentException("TerrainSize must be a power of 2");
    }

    private bool IsPowerOfTwo(int x) => (x != 0) && ((x & (x - 1)) == 0); // from https://stackoverflow.com/a/600306

    public void Build() {
        GameObject go = GameObject.Find("Terrain Space");
        if (go == null) go = new GameObject("Terrain Space");

        terrain = go.GetComponent<Terrain>();
        if (terrain == null) terrain = go.AddComponent<Terrain>();
        terrain.terrainData = new TerrainData();
        terrain.materialTemplate = new Material(Shader.Find("Nature/Terrain/Diffuse"));

        terrain.terrainData.heightmapResolution = size + 1;
        var heights = new float[size, size];

        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                heights[y, x] = heightmapMask[y, x] ? 0f : 1f;
                // heights[y, x] = heightmapMask[y, x] ? Mathf.Clamp01(Mathf.PerlinNoise(y / FREQUENCY, x / FREQUENCY) * AMPLITUDE) : 1f;
                // heights[y, x] += Mathf.Clamp01(Mathf.PerlinNoise(y / FREQUENCY, x / FREQUENCY) * AMPLITUDE);
            }
        }

        // AddNoise(heights);
        // heights = TerrainBlur.BlurCPU(heights, passes: 1, radius: 3);
        // for (int i = 0; i < 1; i++) {
        //     heights = TerrainBlur.ApplyBlur(heights, 5);
        // }
        // SmoothAlongMask(heights);
        // AddNoise(heights);
        // heights = TerrainBlur.ApplyBlur(heights, 5);
        // for (int i = 0; i < 3; i++) {
        //     heights = TerrainBlur.ApplyBlur(heights, 10);
        // }
        terrain.terrainData.SetHeights(0, 0, heights);
        terrain.terrainData.size = new Vector3(size, DEPTH, size);
        // Terrain.Flush();
        // Terrain.terrainData.size = new Vector3(settings.TerrainSize, settings.Depth, settings.TerrainSize);

        terrain.Flush();
    }

    public void SmoothAlongMask(float[,] heights) {
        // TODO increase speed by blurring into a new array on GPU via compute shaders -> control via passes -> no need for lookahead/radius(maybe implementradius as lookahead later)

        //determine borderpixels of heightmap mask -> pixels of inner border are calculated
        List<Tuple<int, int>> borderCoords = new();
        // padded by 1 as border coordinates of the terrain do not qualify as borderpixels
        for (int y = 1; y < size - 1; y++) {
            for (int x = 1; x < size - 1; x++) {
                if (heightmapMask[y, x] != heightmapMask[y - 1, x] // bottom
                || heightmapMask[y, x] != heightmapMask[y + 1, x] // top
                || heightmapMask[y, x] != heightmapMask[y, x - 1] // left
                || heightmapMask[y, x] != heightmapMask[y, x + 1] //right
                ) {
                    borderCoords.Add(new(x, y));
                }
            }
        }
        // Debug.Log(borderCoords.Count);
        // var smoothRadius = 3;
        // var smoothPasses = 20;
        // for (int n = 0; n < smoothPasses; n++) {
        //     foreach (var (x, y) in borderCoords) {
        //         for (int i = 0; i < smoothRadius; i++) {
        //             for (int j = 0; j < smoothRadius; j++) {
        //                 if (x - j < 1 || y - i < 1 || x + j > size - 2 || y + j > size - 2) continue; // dont smooth too close to edge of terrain
        //                 heights[y + i, x + j] = Mathf.Clamp01(GetSmoothedValue(x + j, y + i, heights, true));
        //                 heights[y - i, x - j] = Mathf.Clamp01(GetSmoothedValue(x - j, y - i, heights, true));
        //             }
        //         }

        //     }
        // }
        // for (int y = smoothRadius; y < size - smoothRadius; y++) {
        //     for (int x = smoothRadius; x < size - smoothRadius; x++) {
        //         // smooth any position
        //         //TODO only consider edges of rooms
        //         for (int i = 0; i < smoothRadius; i++) {
        //             for (int j = 0; j < smoothRadius; j++) {
        //                 heights[y + i, x + j] = Mathf.Clamp01(GetSmoothedValue(x + j, y + i, heights, true));
        //                 heights[y - i, x - j] = Mathf.Clamp01(GetSmoothedValue(x - j, y - i, heights, true));
        //             }
        //         }
        //     }
        // // }
        // for (int n = 0; n < smoothPasses; n++) {
        //     for (int y = 1; y < size - 1; y++) {
        //         for (int x = 1; x < size - 1; x++) {
        //             // smooth any position
        //             //TODO only consider edges of rooms
        //             heights[y, x] = Mathf.Clamp01(GetSmoothedValue(x, y, heights, true));
        //             heights[y, x] = Mathf.Clamp01(GetSmoothedValue(x, y, heights, true));

        //         }
        //     }
        // }
    }

    public void AddNoise(float[,] heights) {
        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                heights[y, x] += Mathf.Clamp01(Mathf.PerlinNoise(y / FREQUENCY, x / FREQUENCY) * AMPLITUDE);
            }
        }
    }

    private float GetSmoothedValue(int x, int y, float[,] heights, bool strongSmoothing) {
        // calculate neighbours -> pos > 0 this works
        var heightTopLeft = heights[y - 1, x - 1];
        var heightTop = heights[y - 1, x];
        var heightTopRight = heights[y - 1, x + 1];

        var heightLeft = heights[y, x - 1];
        var height = heights[y, x];
        var heightRight = heights[y, x + 1];

        var heightBottomLeft = heights[y + 1, x - 1];
        var heightBottom = heights[y + 1, x];
        var heightBottomRight = heights[y + 1, x + 1];

        float mean = (heightTopLeft + heightTop + heightTopRight + heightLeft + heightRight + heightBottomLeft + heightBottom + heightBottomRight) / 8 - height;

        return strongSmoothing ? height + mean : height + mean / 2;
    }

}
