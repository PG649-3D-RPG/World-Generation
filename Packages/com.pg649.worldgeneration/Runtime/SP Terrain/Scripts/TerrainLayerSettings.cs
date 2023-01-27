using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "TerrainLayerSettings", menuName = "PG649-WorldGeneration/Terrain Layer Settings")]
public class TerrainLayerSettings : ScriptableObject {

    [Serializable]
    public struct LayerTuple{
        public TerrainMasksE mask;
        public TerrainLayer terrainLayer;
    }
    [SerializeField]
    public LayerTuple[] layerTuples;
    

}
