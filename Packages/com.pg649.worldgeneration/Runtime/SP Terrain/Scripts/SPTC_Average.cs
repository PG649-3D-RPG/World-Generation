using UnityEngine;

public class SPTC_Average : MaskEffect {
    [Header("Settings")]
    public int numberOfRuns;


    protected override void Apply(Heightmap h, Mask mask) {
        h.AverageFilter(mask: mask, numberOfRuns: numberOfRuns);
    }
}
