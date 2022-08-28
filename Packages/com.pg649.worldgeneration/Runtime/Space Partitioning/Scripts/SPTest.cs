using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class SPTest : MonoBehaviour
{
    public int width = 10;
    public int depth = 10;
    public int minWidth = 2;
    public int minDepth = 2;

    void Start()
    {
        int[] size = new int[] {width,depth};
        int[] minSize = new int[] {minWidth,minDepth};
        Tuple<int,int>[] minMaxMargin = new Tuple<int,int>[] {new Tuple<int,int> (1,2), new Tuple<int,int> (1,2) };
        SPTree spTree = new SPTree(size, SPTree.KDTreeRandom(minSize));
        DungeonTree dTree = new DungeonTree(spTree);
        Debug.Log(dTree.GetType());
        dTree.MinMaxMargin = minMaxMargin;
        Debug.Log("minmaxmargin:" + dTree.MinMaxMargin[0].Item1 + dTree.MinMaxMargin[0].Item2);
        dTree.PlaceRooms();
        dTree.ToGameObject();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
