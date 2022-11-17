using UnityEngine;

public class SPTC_Average : MaskEffect {
    [Header("Settings")]
    public int numberOfRuns;
    public bool useGPU = true;


    protected override void Apply(Heightmap h, Mask mask) {
        if (useGPU)
            h.AverageFilterGPU(mask: mask, numberOfRuns: numberOfRuns);
        else
            h.AverageFilter(mask: mask, numberOfRuns: numberOfRuns);
    }
}
