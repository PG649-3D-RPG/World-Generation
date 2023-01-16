using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainMod{
    private Terrain terrain;
    private int terrainWidth;
    private int terrainHeight;

    public TerrainMod(Terrain terrain, int terrainWidth, int terrainHeight){
        this.terrain = terrain;
        this.terrainWidth = terrainWidth;
        this.terrainHeight = terrainHeight;
    }   

    public void MarkSpawnPoints(List<Tuple<Vector3Int, int>> spawnPoints, Texture2D texture){
        TerrainData td = terrain.terrainData;
        int tlc = td.terrainLayers.Length;
        TerrainLayer tlSpawnPoints = new TerrainLayer();
        tlSpawnPoints.diffuseTexture = texture;
        TerrainLayer[] tla = new TerrainLayer[tlc + 1];
        for(int i = 0; i < tlc; i++){
            tla[i] = td.terrainLayers[i];
        }
        tla[tlc] = tlSpawnPoints;
        td.SetTerrainLayersRegisterUndo(tla, "Add SpawnPoints layer");

        //Assign 0f when other parts have textures
        float[,,] alphaData = td.GetAlphamaps(0, 0, td.alphamapWidth, td.alphamapHeight);
        for(int i = 0; i < td.alphamapWidth; i++){
            for(int j = 0; j < td.alphamapHeight; j++){
                alphaData[i,j,tlc] = 1f; 
            }
        }

        float stepSizeWidth = (float)td.alphamapWidth / terrainWidth;
        float stepSizeHeight = (float)td.alphamapHeight / terrainHeight;
        foreach(Tuple<Vector3Int, int> t in spawnPoints){
            Vector3Int p = t.Item1;
            alphaData[Mathf.RoundToInt((float)p.z * stepSizeWidth), Mathf.RoundToInt((float)p.x*stepSizeHeight), tlc] = 0f;
        }
        td.SetAlphamaps(0, 0, alphaData);
    }

}
