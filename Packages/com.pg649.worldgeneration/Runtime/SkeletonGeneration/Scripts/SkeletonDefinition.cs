using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonDefinition
{
    public BoneDefinition RootBone;

    public LimitTable JointLimits;
    public SkeletonDefinition(BoneDefinition root, LimitTable limits) {
        this.RootBone = root;
        this.JointLimits = limits;
    }
}

public struct JointLimits {
    public float XAxisMax;
    public float XAxisMin;
    public float YAxisSymmetric;
    public float ZAxisSymmetric;
}
