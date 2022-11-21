using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPTC_WG : MonoBehaviour
{
    public WorldGeneratorSettings settings;
    void Start(){
        WorldGenerator.Generate(settings);
    }
}