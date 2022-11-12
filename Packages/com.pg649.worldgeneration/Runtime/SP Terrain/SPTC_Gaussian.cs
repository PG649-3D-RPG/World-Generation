using UnityEngine;

public class SPTC_Gaussian : MaskEffect {
    [Header("Settings")]
    public int numberOfRuns;
    [Tooltip("GaussianBlur Compute Shader has to be selected.")]
    public ComputeShader gaussian;
    public Gauss_SD weight;


    protected override void Apply(Heightmap h, Mask mask) {
        h.GaussianBlur(gaussian, mask: mask, numberOfRuns: numberOfRuns, std: weight);
    }
}
