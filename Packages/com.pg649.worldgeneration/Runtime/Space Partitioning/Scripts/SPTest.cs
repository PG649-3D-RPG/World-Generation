using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SPTest : MonoBehaviour
{
    void Start()
    {
        int[] size = new int[] {10,10};
        int[] minSize = new int[] {3,3};
        SPTree spTree = new SPTree(size, SPTree.KDTreeRandom(minSize));
        DungeonTree dTree = new DungeonTree(spTree);
        Debug.Log(dTree.Rand);
        Debug.Log(dTree.MinMaxMargin[0]);
        dTree.PlaceRooms();
      
        foreach(DungeonTree l in dTree.Leaves()){
            Debug.Log("point:" + l.Point[0] + "," + l.Point[1]);
            Debug.Log("size:" + l.label[0] + "," + l.label[1] + "\n");
            Debug.Log(l.Room);
        }
        dTree.ToGameObject();
       // Debug.Log(tree.Depth); 
        //Debug.Log(tree.Height);
        //Debug.Log(tree.Leaves().Count);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
