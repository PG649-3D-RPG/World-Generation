using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Biome Settings", menuName = "PG649-WorldGeneration/Biome Settings")]
public class BiomeSettings : ScriptableObject
{   
    public float objectsSquareMeter = .1f;
    public bool seedIncrement = false;

    [System.Serializable]
    public struct BiomeTuple{
        public float p;
        public BiomeObjectSettings settings;
    }
    public List<BiomeTuple> objects;

    public List<Placeable> GetPlaceables(int width, int height, int seed = 42){
        float nObjects = objectsSquareMeter * width * height;
        List<Placeable> pl = new List<Placeable>();
        foreach(BiomeTuple bt in objects){
            for(int i = 0; i < bt.p * nObjects; i++){
                int s = seed;
                pl.Add(bt.settings.GetPlaceable(seed : s));
                if(seedIncrement) s += 1;
            }
        }
        return pl;
    }
}
