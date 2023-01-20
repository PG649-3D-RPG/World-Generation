using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BiomeObjectSettings : ScriptableObject
{
    public abstract Placeable GetPlaceable(int seed = 42);
}
