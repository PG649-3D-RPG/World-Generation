using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SPTC_PerlinNoise : MaskEffect
{
    [Header("Settings")]
    public float scale = .1f;
    public float maxAddedHeight = 1f;
    [Header("Fractal Perlin Noise")]
    public int numberOfRuns = 1;


    protected override void Apply(Heightmap h, Mask mask){
        h.PerlinNoise(maxAddedHeight : maxAddedHeight, scale: scale, mask : mask, fractalRuns : numberOfRuns);
    }
}