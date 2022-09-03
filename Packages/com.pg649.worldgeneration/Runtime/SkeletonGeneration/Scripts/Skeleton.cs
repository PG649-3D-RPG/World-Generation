using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Skeleton : MonoBehaviour
{
    public Dictionary<BoneCategory, List<GameObject>> bonesByCategory;

    public int nBones;
    public int nAngXMotLimited;    
    public int nAngYMotLimited;
    public int nAngZMotLimited;

    public Skeleton() {
        bonesByCategory = new();

        foreach (BoneCategory cat in Enum.GetValues(typeof(BoneCategory))) {
            bonesByCategory[cat] = new List<GameObject>();
        }

        nBones = 0;
        nAngXMotLimited = 0;
        nAngYMotLimited = 0;
        nAngZMotLimited = 0;
    }
}
