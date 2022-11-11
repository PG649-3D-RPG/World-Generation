using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.AI.Navigation;

public class SPTC_Base : MonoBehaviour
{
    [Header("Random")]
    public int seed = 42;
    [Header("Space Partitioning")]
    public int size = 1024;
    public SPTreeT.PartitionMode partitionMode;
    public int minPartitionWidth = 64;
    public int minPartitionDepth = 64;
    public int skipChildren = 0;
    [Header("Level Placement")]
    public int leftRightMinMargin = 11;
    public int leftRightMaxMargin = 16;
    public int frontBackMinMargin = 11;
    public int frontBackMaxMargin = 16;
    public int levelPlacementProbability = 60;
    public int numberOfTypes = 1;
    [Header("Corridors")]
    public int minCorridorWidth = 6;
    public int maxCorridorWidth = 10;
    public float minCorridorHeight = 2;
    public float maxCorridorHeight = 3;
    public float maxDistance = 32;

    void Start()
    {
        int[] spsize = new int[] {size,size};
        int[] minSize = new int[] {minPartitionWidth,minPartitionDepth};
        Tuple<int,int>[] minMaxMargin = new Tuple<int,int>[] {new Tuple<int,int> (leftRightMinMargin,leftRightMaxMargin), new Tuple<int,int> (frontBackMinMargin,frontBackMaxMargin)};
        SPTreeT spTree;
        System.Random rand = new System.Random(seed);
        switch(partitionMode){
            case SPTreeT.PartitionMode.KDTreeRandom:
                spTree = new SPTreeT(spsize, SPTreeT.KDTreeRandom(minSize), rand : rand, skipChildren : skipChildren);
                break;
            case SPTreeT.PartitionMode.QuadTreeUniform:
                spTree = new SPTreeT(spsize, SPTreeT.QuadTreeUniform(), SPTreeT.StopMinSize(minSize), rand : rand, skipChildren : skipChildren);
                break;
            default:
                spTree = new SPTreeT(spsize, SPTreeT.KDTreeRandom(minSize), rand : rand, skipChildren : skipChildren);
                break;
        }
        DungeonTreeT dTree = new DungeonTreeT(spTree);
        dTree.Root.Node.FHeight = DungeonTreeNode.fHeight2DMinMax(3,4);
        dTree.Root.Node.MinMaxMargin = minMaxMargin;
        dTree.PlaceRooms(levelPlacementProbability);
        dTree.PlaceCorridors(minCorridorWidth, maxCorridorWidth, minCorridorHeight, maxCorridorHeight, maxDistance : maxDistance);
        dTree.AssignTypes(numberOfTypes);

        Heightmap heightmap = new Heightmap(size);

        TerrainMasks tm = dTree.ToTerrainMasks();

        foreach(MaskEffect me in GetComponents<MaskEffect>()){
            me.Apply(heightmap, tm);
        }
        
        GameObject tgo = new GameObject("Terrain");
        tgo.transform.parent = gameObject.transform;
        heightmap.AddTerrainToGameObject(tgo);
        NavMeshSurface nms = tgo.GetComponent<NavMeshSurface>();
        if (nms == null) nms = tgo.AddComponent<NavMeshSurface>();
        nms.BuildNavMesh();
        
    }
}