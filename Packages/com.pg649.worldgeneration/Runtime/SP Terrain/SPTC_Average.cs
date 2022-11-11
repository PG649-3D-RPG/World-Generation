using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPTC_Average : MaskEffect {
    [Header("Settings")]
    public int numberOfRuns;
    public bool useGPU;
    public ComputeShader averageShader;


    protected override void Apply(Heightmap h, Mask mask) {
        if (useGPU)
            h.AverageFilterGPU(averageShader, mask: mask, numberOfRuns: numberOfRuns);
        else
            h.AverageFilter(mask: mask, numberOfRuns: numberOfRuns);
    }
}
