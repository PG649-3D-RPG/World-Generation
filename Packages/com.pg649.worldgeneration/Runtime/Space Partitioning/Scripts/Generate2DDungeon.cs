using UnityEngine;
using System;
using System.Linq;

public class SPTest : MonoBehaviour
{
    public int width = 256;
    public int depth = 256;
    public SPTreeT.PartitionMode partitionMode;
    public int minWidth = 2;
    public int minDepth = 2;
    public int roomPlacementProbability = 100;

    void Start()
    {
        int[] size = new int[] {width,depth};
        int[] minSize = new int[] {minWidth,minDepth};
        Tuple<int,int>[] minMaxMargin = new Tuple<int,int>[] {new Tuple<int,int> (1,5), new Tuple<int,int> (1,5) };
        SPTreeT spTree;
        switch(partitionMode){
            case SPTreeT.PartitionMode.KDTreeRandom:
                spTree = new SPTreeT(size, SPTreeT.KDTreeRandom(minSize));
                break;
            case SPTreeT.PartitionMode.QuadTreeUniform:
                spTree = new SPTreeT(size, SPTreeT.QuadTreeUniform(), SPTreeT.StopMinSize(minSize));
                break;
            default:
                spTree = new SPTreeT(size, SPTreeT.KDTreeRandom(minSize));
                break;
        }
        
        DungeonTreeT dTree = new DungeonTreeT(spTree);
        dTree.Root.Node.FHeight = DungeonTreeNode.fHeight2DMinMax(3,4);
        dTree.Root.Node.MinMaxMargin = minMaxMargin;
        dTree.PlaceRooms(roomPlacementProbability);
        dTree.ToGameObject();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
