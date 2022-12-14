using System;
using System.IO;
using UnityEngine;

public class Generate2DDungeon : MonoBehaviour
{
    [Header("Random")]
    public int seed = 42;
    [Header("Space Partitioning")]
    public int width = 256;
    public int depth = 256;
    public SPTreeT.PartitionMode partitionMode;
    public int minPartitionWidth = 16;
    public int minPartitionDepth = 16;
    public int skipChildren = 0;
    [Header("Room Placement")]
    public int leftRightMinMargin = 1;
    public int leftRightMaxMargin = 5;
    public int frontBackMinMargin = 1;
    public int frontBackMaxMargin = 5;
    public int roomPlacementProbability = 100;
    // public bool useTerrains = false;
    // [Header("QuadTree quadratic terrain options")]
    // public bool quadraticTerrain = false;
    // public int divideMin = 1;
    // public int divideMax = 2;
    [Header("Corridors")]
    public int minCorridorWidth = 1;
    public int maxCorridorWidth = 1;
    public float minCorridorHeight = 2;
    public float maxCorridorHeight = 3;
    public float maxDistance = 32;
    [Header("Other")]
    public bool createTexture = false;
    public EnvironmentGeneratorBitmapSettings environmentGeneratorSettings;
    void Start()
    {
        int[] size = new int[] {width,depth};
        int[] minSize = new int[] {minPartitionWidth,minPartitionDepth};
        Tuple<int,int>[] minMaxMargin = new Tuple<int,int>[] {new Tuple<int,int> (leftRightMinMargin,leftRightMaxMargin), new Tuple<int,int> (frontBackMinMargin,frontBackMaxMargin)};
        SPTreeT spTree;
        System.Random rand = new System.Random(seed);
        switch(partitionMode){
            case SPTreeT.PartitionMode.KDTreeRandom:
                spTree = new SPTreeT(size, SPTreeT.KDTreeRandom(minSize), rand : rand, skipChildren : skipChildren);
                break;
            case SPTreeT.PartitionMode.QuadTreeUniform:
                spTree = new SPTreeT(size, SPTreeT.QuadTreeUniform(), SPTreeT.StopMinSize(minSize), rand : rand, skipChildren : skipChildren);
                break;
            default:
                spTree = new SPTreeT(size, SPTreeT.KDTreeRandom(minSize), rand : rand, skipChildren : skipChildren);
                break;
        }
        DungeonTreeT dTree = new DungeonTreeT(spTree);
        dTree.Root.Node.FHeight = DungeonTreeNode.fHeight2DMinMax(3,4);
        dTree.Root.Node.MinMaxMargin = minMaxMargin;
        dTree.PlaceRooms(roomPlacementProbability);//, quadraticTerrain : quadraticTerrain, quadraticTerrainMin : divideMin, quadraticTerrainMax : divideMax);
        dTree.PlaceCorridors(minCorridorWidth, maxCorridorWidth, minCorridorHeight, maxCorridorHeight, maxDistance : maxDistance);
        //dTree.ToGameObject();
        bool[,] a = (dTree.RoomsMask() + dTree.CorridorsMask()).Array ;

        // Texture2D tt = dTree.ToTexture(dTree.RoomsFreeMask());
        // byte[] bytess = ImageConversion.EncodeArrayToPNG(tt.GetRawTextureData(), tt.graphicsFormat, (uint)tt.width, (uint)tt.height);
        // File.WriteAllBytes(Application.dataPath + "/test.png", bytess);
        
        EnvironmentGeneratorBitmap generator = new EnvironmentGeneratorBitmap(a, environmentGeneratorSettings);
        generator.Build();
        // if(createTexture){
        //     Texture2D t = dTree.ToTexture(a);
        //     byte[] bytes = ImageConversion.EncodeArrayToPNG(t.GetRawTextureData(), t.graphicsFormat, (uint)t.width, (uint)t.height);
        //     File.WriteAllBytes(Application.dataPath + "/dungeon.png", bytes);
        // }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}