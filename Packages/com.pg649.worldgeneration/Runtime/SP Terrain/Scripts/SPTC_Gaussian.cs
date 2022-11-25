using UnityEngine;

public class SPTC_Gaussian : MaskEffect {
    [Header("Settings")]
    public int numberOfRuns;
    public Gauss_SD weight;


    protected override void Apply(Heightmap h, Mask mask) {
        h.GaussianBlur(mask: mask, numberOfRuns: numberOfRuns, std: weight);
    }
}
