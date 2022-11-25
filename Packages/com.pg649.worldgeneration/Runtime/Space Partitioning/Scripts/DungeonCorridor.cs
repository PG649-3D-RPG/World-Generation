using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonCorridor : IGameObjectable {
    private List<Vector3Int> path;
    private int width;
    private float height;
    public DungeonRoom startRoom, endRoom;
    public DungeonRoom.Face startFace, endFace;
    private int startIndex, endIndex;
    public Vector3Int partitionPoint;

    public DungeonCorridor(){
        path = new List<Vector3Int>();
    }

    public GameObject ToGameObject(){
        GameObject go = new GameObject("DungeonCorridor");
        DungeonCorridorMeta dcm = go.AddComponent<DungeonCorridorMeta>();
        dcm.SetValues(this);
        List<Vector3> l = new List<Vector3>();
        foreach(Vector3Int v in path) l.Add(new Vector3(v.x,v.y,v.z));
        MeshGeneration.CorridorGround(l,width).transform.parent = go.transform;
        return go;
    }

    public float Length(){
        float l = 0;
        for(int i = 0; i < path.Count-1; i++){
            l += Vector3.Distance(path[i],path[i+1]);
        }
        return l;
    }

    public void ApplyToBoolArray(bool[,] m){
        Vector3Int p = path[0];
        int i = 0;
        while(i != path.Count-1){
            Vector3Int dir = (path[i+1]-path[i]);
            if(dir.magnitude > 0) dir = dir/(int)dir.magnitude;
            for(int j = 0; j < width; j++){
                if(dir == Vector3.left || dir == Vector3.right){
                    m[partitionPoint.x + p.x, partitionPoint.z +p.z + j] = true;
                }
                else{
                    m[partitionPoint.x + p.x + j, partitionPoint.z +p.z] = true;
                }
            }
            p += dir;
            if(p == path[i+1]){
                Vector3Int nextDir = i < path.Count-2 ? (path[i+2]-path[i+1]) : Vector3Int.zero;
                if(nextDir.magnitude > 0) nextDir = nextDir/(int)nextDir.magnitude;
                if((dir == Vector3Int.right &&  nextDir == Vector3Int.back) || (dir == Vector3Int.forward && nextDir == Vector3.left)){
                    p += (-nextDir)*(width-1);
                }
                i += 1;
            }
        }
    }

    public List<Vector3Int> Path{
        get{return path;}
        set{path = value;}
    }
    public int Width{
        get{return width;}
        set{width = value;}
    }   
    public float Height{
        get{return height;}
        set{height = value;}
    }
    public DungeonRoom StartRoom{
        get{return startRoom;}
        set{startRoom = value;}
    }
    public DungeonRoom EndRoom{
        get{return endRoom;}
        set{endRoom = value;}
    }
    public DungeonRoom.Face StartFace{
        get{return startFace;}
        set{startFace = value;}
    }
    public DungeonRoom.Face EndFace{
        get{return endFace;}
        set{endFace = value;}
    }
    public int StartIndex{
        get{return startIndex;}
        set{startIndex = value;}
    }
    public int EndIndex{
        get{return endIndex;}
        set{endIndex = value;}
    }
    public Vector3Int PartitionPoint{
        get{return partitionPoint;}
        set{partitionPoint = value;}
    }
}
class DungeonCorridorMeta : MonoBehaviour{
    public List<Vector3Int> path;
    public float length;
    public DungeonCorridorMeta(){
    }
    public void SetValues(List<Vector3Int> path){
        this.path = path;
    }
    public void SetValues(DungeonCorridor c){
        path = c.Path;
        length = c.Length();
    }
}
