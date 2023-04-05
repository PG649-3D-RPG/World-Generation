using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TerrainMod {
    private Terrain terrain;
    private int terrainWidth;
    private int terrainHeight;
    private TerrainMasks terrainMasks;
    private TerrainData td;
    private int resolutionScale;

    public TerrainMod(Terrain terrain, int terrainWidth, int terrainHeight, TerrainMasks tm, int resolutionScale = 4) {
        this.terrain = terrain;
        td = terrain.terrainData;
        this.terrainWidth = terrainWidth;
        this.terrainHeight = terrainHeight;
        this.terrainMasks = tm;
        this.resolutionScale = resolutionScale;
        td.alphamapResolution = terrainWidth * resolutionScale;
    }

    public void MarkSpawnPoints(List<Tuple<Vector3Int, int>> spawnPoints, Texture2D texture) {
        int tlc = td.terrainLayers.Length;
        TerrainLayer tlSpawnPoints = new TerrainLayer();
        tlSpawnPoints.diffuseTexture = texture;
        TerrainLayer[] tla = new TerrainLayer[tlc + 1];
        for (int i = 0; i < tlc; i++) {
            tla[i] = td.terrainLayers[i];
        }
        tla[tlc] = tlSpawnPoints;
        td.terrainLayers = tla;

        //td.alphamapResolution = terrainWidth * 4;
        float[,,] alphaData = td.GetAlphamaps(0, 0, td.alphamapWidth, td.alphamapHeight);
        int alphamapWidth = td.alphamapWidth;
        int alphamapHeight = td.alphamapHeight;
        Parallel.For(0, alphamapWidth * alphamapHeight, id => {
            int i = id / alphamapWidth;
            int j = id % alphamapHeight;
            alphaData[i, j, tlc] = 0f;
        });

        //float stepSizeWidth = (float)td.alphamapWidth / terrainWidth;
        //float stepSizeHeight = (float)td.alphamapHeight / terrainHeight;

        foreach (Tuple<Vector3Int, int> t in spawnPoints) {
            Vector3Int p = t.Item1;
            for (int ix = 0; ix < resolutionScale; ix++) {
                for (int iy = 0; iy < resolutionScale; iy++) {
                    alphaData[(p.z * resolutionScale) + ix, (p.x * resolutionScale) + iy, tlc] = 1f;
                }
            }
            //alphaData[Mathf.RoundToInt((float)p.z * stepSizeWidth), Mathf.RoundToInt((float)p.x*stepSizeHeight), tlc] = 0f;
        }
        td.SetAlphamaps(0, 0, alphaData);
    }

    public void ApplyTerrainLayer(Mask mask, TerrainLayer tl, float[,] values = null) {
        TerrainData td = terrain.terrainData;
        float[,,] alphaData = td.GetAlphamaps(0, 0, td.alphamapWidth, td.alphamapHeight);
        float[,,] newAlphaData = new float[alphaData.GetLength(0), alphaData.GetLength(1), alphaData.GetLength(2) + 1];

        int dim0 = alphaData.GetLength(0);
        int dim1 = alphaData.GetLength(1);
        int dim2 = alphaData.GetLength(2);

        Parallel.For(0, dim0, i => {
            for (int j = 0; j < dim1; j++) {
                for (int k = 0; k < dim2; k++) {
                    newAlphaData[i, j, k] = alphaData[i, j, k];
                }
            }
        });

        int tlc = td.terrainLayers.Length;
        TerrainLayer[] tla = new TerrainLayer[tlc + 1];
        for (int i = 0; i < tlc; i++) {
            tla[i] = td.terrainLayers[i];
        }
        tla[tlc] = tl;
        td.terrainLayers = tla;

        //td.alphamapResolution = terrainWidth * 4;
        Parallel.For(0, terrainWidth, x => {
            for (int y = 0; y < terrainHeight; y++) {
                for (int ix = 0; ix < resolutionScale; ix++) {
                    for (int iy = 0; iy < resolutionScale; iy++) {
                        newAlphaData[(y * resolutionScale) + iy, (x * resolutionScale) + ix, tlc] = mask[x, y] ? 1f : 0f;
                    }
                }
            }
        });

        td.SetAlphamaps(0, 0, newAlphaData);
    }

    public void ApplyTerrainLayer(Mask mask, Texture2D t) {
        TerrainLayer tl = new TerrainLayer();
        tl.diffuseTexture = t;
        ApplyTerrainLayer(mask, tl);
    }

    public void ApplyTerrainLayers(TerrainLayerSettings s) {
        foreach (TerrainLayerSettings.LayerTuple t in s.layerTuples) {
            ApplyTerrainLayer(terrainMasks.MaskByEnum(t.mask), t.terrainLayer);
        }
    }

    public void ApplyPerlinNoise(Mask mask) {
        TerrainLayer tl = new TerrainLayer();
        tl.diffuseTexture = Texture2D.whiteTexture;
        TerrainData td = terrain.terrainData;
        float[,,] alphaData = td.GetAlphamaps(0, 0, td.alphamapWidth, td.alphamapHeight);
        float[,,] newAlphaData = new float[alphaData.GetLength(0), alphaData.GetLength(1), alphaData.GetLength(2) + 1];

        int dim0 = alphaData.GetLength(0);
        int dim1 = alphaData.GetLength(1);
        int dim2 = alphaData.GetLength(2);

        Parallel.For(0, dim0, i => {
            for (int j = 0; j < dim1; j++) {
                for (int k = 0; k < dim2; k++) {
                    newAlphaData[i, j, k] = alphaData[i, j, k];
                }
            }
        });

        int tlc = td.terrainLayers.Length;
        TerrainLayer[] tla = new TerrainLayer[tlc + 1];
        for (int i = 0; i < tlc; i++) {
            tla[i] = td.terrainLayers[i];
        }
        tla[tlc] = tl;
        td.terrainLayers = tla;

        //td.alphamapResolution = terrainWidth * 4;
        Parallel.For(0, terrainWidth, x => {
            for (int y = 0; y < terrainHeight; y++) {
                for (int ix = 0; ix < resolutionScale; ix++) {
                    for (int iy = 0; iy < resolutionScale; iy++) {
                        newAlphaData[(y * resolutionScale) + iy, (x * resolutionScale) + ix, tlc] = mask[x, y] ? Mathf.PerlinNoise((float)x / 50, (float)y / 50) : 0f;
                    }
                }
            }
        });
        td.SetAlphamaps(0, 0, newAlphaData);
    }

}
