using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Biome Settings", menuName = "PG649-WorldGeneration/Biome Settings")]
public class BiomeSettings : ScriptableObject
{   
    public float objectsSquareMeter = .1f;

    [System.Serializable]
    public struct BiomeTuple{
        public float p;
        public BiomeObjectSettings settings;
    }
    public BiomeTuple[] objects;

    public Placeable[] GetPlaceables(int seed = 42){
        Placeable[] pa = new Placeable[objects.Length];
        for(int i = 0; i < pa.Length; i++){
            pa[i] = objects[i].settings.GetPlaceable(seed : seed);
        }
        return pa;
    }
}
