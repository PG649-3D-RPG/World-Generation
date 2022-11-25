using System;
using UnityEngine;
public class Heightmap {
    private int heightScale;
    private int size;
    private float[,] heights;

    public Heightmap(int size, int heightScale = 100) {
        this.size = size;
        this.heightScale = heightScale;
        this.heights = new float[size, size];
    }

    public void SetByMask(Mask mask, float trueValue, float falseValue = -1) {
        HeightmapTransforms.SetByMask(heights, mask, trueValue / heightScale, falseValue: falseValue == -1 ? -1 : falseValue / heightScale);
    }
    public void PerlinNoise(float maxAddedHeight = 1, float scale = .1f, Mask mask = null, int fractalRuns = 1) {
        HeightmapTransforms.ApplyPerlinNoise(heights, maxAddedHeight: maxAddedHeight / heightScale, scale: scale, mask: mask, fractalRuns: fractalRuns);
    }
    public void AverageFilter(Mask mask = null, int numberOfRuns = 1) {
        // System.Diagnostics.Stopwatch sw = new();
        // sw.Restart();
        for (int i = 0; i < numberOfRuns; i++) {
            HeightmapTransforms.ApplyFilter(heights, HeightmapTransforms.AverageFilter(3, 3), mask: mask);
        }
        // sw.Stop();
        // Debug.Log("Runtime AverageFilter CPU:\t " + sw.Elapsed);
        // sw.Reset();
    }
    public void AverageFilterGPU(Mask mask = null, int numberOfRuns = 1) {
        // System.Diagnostics.Stopwatch sw = new();
        // sw.Restart();
        TerrainShader.AverageFilterGPU3x3(input: heights, mask: mask, passes: numberOfRuns);
        // sw.Stop();
        // Debug.Log("Runtime AverageFilter GPU:\t " + sw.Elapsed);
        // sw.Reset();
    }
    public void GaussianBlur(Mask mask = null, int numberOfRuns = 1, Gauss_SD std = Gauss_SD.SD1) {
        TerrainShader.GaussianBlurGPU3x3(input: heights, mask: mask, passes: numberOfRuns, std: std);
    }
    public void Power(float power, Mask mask = null) {
        MapI(f => Mathf.Pow(f, power), mask);
    }

    public void MapI(Func<float, float> f, Mask mask = null) {
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                if (mask == null || mask[i, j]) heights[i, j] = f(heights[i, j]);
            }
        }
    }

    public void AddTerrainToGameObject(GameObject go) {
        Terrain terrain = go.AddComponent<Terrain>();
        terrain.terrainData = new TerrainData();
        terrain.materialTemplate = new Material(Shader.Find("Nature/Terrain/Diffuse"));
        terrain.terrainData.heightmapResolution = size + 1;
        terrain.terrainData.SetHeights(0, 0, heights.Transpose());
        terrain.terrainData.size = new Vector3(size, 100, size);
        terrain.Flush();
    }
}
