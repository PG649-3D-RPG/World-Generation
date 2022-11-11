using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class EnvironmentGeneratorBitmap {
    private readonly ComputeShader computeShader;
    private readonly EnvironmentGeneratorBitmapSettings gen_settings;

    private readonly bool[,] heightmapMask;
    private readonly int size;

    private Terrain terrain;

    public EnvironmentGeneratorBitmap(bool[,] mask, EnvironmentGeneratorBitmapSettings settings) {
        computeShader = settings.gaussBlurShader;
        gen_settings = settings;
        heightmapMask = mask;
        size = mask.GetLength(0);
        if (!IsPowerOfTwo(size)) throw new System.ArgumentException("TerrainSize must be a power of 2");
    }

    private bool IsPowerOfTwo(int x) => (x != 0) && ((x & (x - 1)) == 0); // from https://stackoverflow.com/a/600306

    public void Build() {
        System.Diagnostics.Stopwatch sw = new();

        GameObject go = GameObject.Find("Terrain Space");
        if (go == null) go = new GameObject("Terrain Space");

        terrain = go.GetComponent<Terrain>();
        if (terrain == null) terrain = go.AddComponent<Terrain>();
        terrain.terrainData = new TerrainData();
        terrain.materialTemplate = new Material(Shader.Find("Nature/Terrain/Diffuse"));

        terrain.terrainData.heightmapResolution = size + 1;
        var heights = new float[size, size];

        bool[,] ivbm = HeightmapTransforms.MaskInvertedBorder(heightmapMask, 6);

        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                heights[y, x] = heightmapMask[y, x] ? 0f : .8f;
                if (ivbm[y, x]) heights[y, x] = 0.5f;
                // heights[y, x] = heightmapMask[y, x] ? Mathf.Clamp01(Mathf.PerlinNoise(y / FREQUENCY, x / FREQUENCY) * AMPLITUDE) : 1f;
                // heights[y, x] += Mathf.Clamp01(Mathf.PerlinNoise(y / FREQUENCY, x / FREQUENCY) * AMPLITUDE);
            }
        }


        //AddNoise(heights);

        for (int i = 0; i < 5; i++) HeightmapTransforms.ApplyFilter(heights, HeightmapTransforms.AverageFilter(3, 3), heightmapMask, invertMask: true);

        HeightmapTransforms.ApplyPerlinNoise(heights, maxAddedHeight: .4f, mask: heightmapMask);
        HeightmapTransforms.ApplyPerlinNoise(heights, maxAddedHeight: .4f, scale: .6f, mask: ivbm);
        HeightmapTransforms.ApplyPerlinNoise(heights, maxAddedHeight: .1f, scale: .4f, mask: heightmapMask.ZipMap(ivbm, (x, y) => !x && !y));


        // sw.Reset();
        // sw.Restart();
        // for (int i = 0; i < 100; i++) {
        //     heights = TerrainBlur.GaussianBlurParCPU(heights, 1, weight: 1f);
        // }
        // sw.Stop();
        // Debug.Log("Runtime Gauss CPU Naive:\t " + sw.Elapsed);
        // sw.Reset();

        sw.Restart();
        TerrainShader.GaussianBlurGPU3x3SD1(computeShader, input: heights, 1000);
        sw.Stop();
        Debug.Log("Runtime Gauss Compute Shader:\t " + sw.Elapsed);
        sw.Reset();

        //AddNoise(heights);
        //heights = TerrainBlur.ApplyBlur(heights, 5);
        // for (int i = 0; i < 3; i++) {
        //     heights = TerrainBlur.ApplyBlur(heights, 10);
        // }
        terrain.terrainData.SetHeights(0, 0, heights);
        terrain.terrainData.size = new Vector3(size, gen_settings.Depth, size);

        terrain.Flush();
        AddNavMesh(go);
    }
    private void AddNavMesh(GameObject go) {
        NavMeshSurface nms = go.GetComponent<NavMeshSurface>();
        if (nms == null) nms = go.AddComponent<NavMeshSurface>();
        nms.BuildNavMesh();
    }

    public void AddNoise(float[,] heights) {
        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                heights[y, x] += Mathf.Clamp01(Mathf.PerlinNoise(y / gen_settings.FREQUENCY, x / gen_settings.FREQUENCY) * gen_settings.AMPLITUDE);
            }
        }
    }

}
