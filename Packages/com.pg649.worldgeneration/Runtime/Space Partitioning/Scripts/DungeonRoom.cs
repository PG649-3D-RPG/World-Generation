using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonRoom : IGameObjectable{
    private int width, depth;
    private float height;
    private List<DungeonCorridor> corridors;
    private List<Tuple<Face,int,int>> corridorPoints;
    private bool[,] free;
    private System.Random rand;
    private Vector3Int roomPoint;
    private int type;
    private List<Tuple<Vector3Int,int>> spawnPoints;


    public enum Face{
        Left, Right, Front, Back, Top, Bottom
    }


    public DungeonRoom(int width, int depth, float height, Vector3Int roomPoint, System.Random rand = null){
        this.width = width;
        this.depth = depth;
        this.height = height;
        this.roomPoint = roomPoint;
        free = new bool[width,depth];
        free.MapI<bool>(x => true);
        this.rand = rand == null ? new System.Random() : rand;
        corridorPoints = new List<Tuple<Face,int,int>>();
        this.type = 0;
        this.spawnPoints = new List<Tuple<Vector3Int,int>>();
    }
    public DungeonRoom(IEnumerable<int> x, Vector3Int roomPoint, System.Random rand = null) : this(x.ToArray()[0],x.ToArray()[1],x.ToArray()[2], roomPoint, rand : rand){}

    //doesnt work if both points are on the same boundary
    public void SetFreePath(Face f1, Face f2, int p1, int p2, int width){
        (int,int) rp1 = FacePointIndex(f1,p1);
        (int,int) rp2 = FacePointIndex(f2,p2);
        (int,int) p = rp1;
        if((f2 == Face.Left || f2 == Face.Right) && (f1 == Face.Back || f1 == Face.Front)){
            Face ft = f1;
            int pt = p1;
            (int,int) rpt = rp1;
            f1 = f2;
            f2 = ft;
            p1 = p2;
            p2 = pt;
            rp1 = rp2;
            rp2 = rpt;
        }
        if((f1 == Face.Left || f1 == Face.Right) && (f2 == Face.Left || f2 == Face.Right)){
            //fix            
            int mid = width/2;
            int dir = rp2.Item1 - rp1.Item1 == 0 ? 1 : (rp2.Item1 - rp1.Item1) / Math.Abs((rp2.Item1 - rp1.Item1));
            while(p.Item1 != mid){
                for(int k = 0; k < width; k++) free[p.Item1,p.Item2+k] = false;
                p.Item1 += dir;
            }
            int dir2 = rp2.Item2 - rp1.Item2 == 0 ? 1 : (rp2.Item2 - rp1.Item2) / Math.Abs((rp2.Item2 - rp1.Item2));
            if(dir2 == -1) p.Item2 = Math.Min(p.Item2 + width -1 , depth-1);
            while(p.Item2 != rp2.Item2){
                for(int k = 0; k < width; k++) free[p.Item1+k,p.Item2] = false;
                p.Item2 += dir2;
            }
            if(dir2 == -1) p.Item2 += 1;
            while(p.Item1 != rp2.Item1){
                for(int k = 0; k < width; k++) free[p.Item1,p.Item2+k] = false;
                p.Item1 += dir;
            }
            for(int k = 0; k < width; k++) free[p.Item1,p.Item2+k] = false;
        }
        else if((f1 == Face.Front || f1 == Face.Back) && (f2 == Face.Front || f2 == Face.Back)){
            //fix            
            int mid = depth/2;
            int dir = rp2.Item2 - rp1.Item2 == 0 ? 1 : (rp2.Item2 - rp1.Item2) / Math.Abs((rp2.Item2 - rp1.Item2));
            while(p.Item2 != mid){
                for(int k = 0; k < width; k++) free[p.Item1+k,p.Item2] = false;
                p.Item2 += dir;
            }
            int dir2 = rp2.Item1 - rp1.Item1 == 0 ? 1 : (rp2.Item1 - rp1.Item1) / Math.Abs((rp2.Item1 - rp1.Item1));
            if(dir2 == -1) p.Item2 = Math.Min(p.Item2 + width -1 , depth-1);
            while(p.Item1 != rp2.Item1){
                for(int k = 0; k < width; k++) free[p.Item1,p.Item2+k] = false;
                p.Item1 += dir2;
            }
            while(p.Item2 != rp2.Item2){
                for(int k = 0; k < width; k++) free[p.Item1+k,p.Item2] = false;
                p.Item2 += dir;
            }
            for(int k = 0; k < width; k++) free[p.Item1+k,p.Item2] = false;
        }
        else{
            int dir = rp2.Item1 - rp1.Item1 == 0 ? 1 : (rp2.Item1 - rp1.Item1) / Math.Abs((rp2.Item1 - rp1.Item1));
            while(p.Item1 != rp2.Item1){
                for(int k = 0; k < width; k++) free[p.Item1,p.Item2+k] = false;
                p.Item1 += dir;
            }
            int dir2 = rp2.Item2 - rp1.Item2 == 0 ? 1 : (rp2.Item2 - rp1.Item2) / Math.Abs((rp2.Item2 - rp1.Item2));
            if(dir2 == -1) p.Item2 = Math.Min(p.Item2 + width -1 , depth-1);
            while(p.Item2 != rp2.Item2){
                for(int k = 0; k < width; k++) free[p.Item1+k,p.Item2] = false;
                p.Item2 += dir2;
            }
            for(int k = 0; k < width; k++) free[p.Item1+k,p.Item2] = false;
        }
    }

    public Vector3Int FacePoint(Face face){
        switch(face){
            case Face.Right:
                return LowerRightPoint;
            case Face.Back:
                return UpperLeftPoint;
            default:
                return roomPoint;
        }
    }
    public Vector3Int FacePoint(Face face, int p){
        switch(face){
            case Face.Left:
                return LowerLeftPoint + new Vector3Int(0,0,p);
            case Face.Right:
                return LowerRightPoint + new Vector3Int(0,0,p);
            case Face.Back:
                return UpperLeftPoint + new Vector3Int(p,0,0);
            case Face.Front:
                return LowerLeftPoint + new Vector3Int(p,0,0);
            default:
                return roomPoint;
        }
    }
    public (int,int) FacePointIndex(Face face, int p){
        switch(face){
            case Face.Left:
                return (0,p);
            case Face.Right:
                return (width-1,p);
            case Face.Back:
                return (p,depth-1);
            case Face.Front:
                return (p,0);
            default:
                return (0,0);
        }
    }

    public void AddCorridorPoint(Tuple<Face, int,int> t){
        corridorPoints.Add(t);
        foreach(Tuple<Face, int,int> tp in corridorPoints){
             if(tp.Item1 != t.Item1){
                SetFreePath(tp.Item1, t.Item1, tp.Item2, t.Item2, Math.Min(t.Item3, tp.Item3));
             }
        }
    }
    public void AddRelativeSpawnPoint(int x, int z, int size){
        spawnPoints.Add(new Tuple<Vector3Int, int>(new Vector3Int(roomPoint.x + x, roomPoint.y, roomPoint.z + z), size)); 
    }
    public void CreateSpawnPoint(int size){
        float[,] a = free.Map(x => x ? 0f : 1f);
        //HeightmapTransforms.ApplyFilter(a, HeightmapTransforms.extensionFilter);
        if(size > 0){
            for(int i = 0; i < a.GetLength(0); i++){
                a[i,0] = 1f;
                a[i,a.GetLength(1)-1] = 1f;
            }
            for(int j = 0; j < a.GetLength(1); j++){
                a[0,j] = 1f;
                a[a.GetLength(0)-1,j] = 1f;
            }
            for(int k = 0; k < size-1; k++) HeightmapTransforms.ApplyFilter(a, HeightmapTransforms.extensionFilter);
        }
        List<Tuple<int,int>> l = new List<Tuple<int,int>>();
        for(int i = 0; i < a.GetLength(0); i++){
            for(int j = 0; j < a.GetLength(1); j++){
                if(a[i,j] == 0f) l.Add(new Tuple<int,int>(i,j));
            }
        }
        if(l.Count > 0){
            int index = rand.Next(0, l.Count);
            Tuple<int,int> t = l[index];
            for(int k = t.Item1 - size; k <= t.Item1 + size; k++){
                for(int m = t.Item2 - size; m <= t.Item2 + size; m++){
                    free[k,m] = false;
                }
            }
            AddRelativeSpawnPoint(t.Item1, t.Item2, size);
        }
    }

    public Mesh ToMesh(){
        Mesh mesh = new Mesh();

        return mesh;
    }
    public GameObject ToGameObject(){
        GameObject go = new GameObject("DungeonRoom");
        DungeonRoomMeta drm = go.AddComponent<DungeonRoomMeta>();
        GameObject q = MeshGeneration.Quad(width,depth);
        q.transform.parent = go.transform;
        GameObject w1 = MeshGeneration.Quad(depth,height, plane : MeshGeneration.Plane.YZ);
        w1.transform.parent = go.transform;
        GameObject w2 = MeshGeneration.Quad(width,height, plane : MeshGeneration.Plane.XY);
        w2.transform.parent = go.transform;
        w2.transform.localPosition += new Vector3(0,0,depth);
        GameObject w3 = MeshGeneration.Quad(width,height, plane : MeshGeneration.Plane.XY);
        w3.transform.parent = go.transform;
        GameObject w4 = MeshGeneration.Quad(depth,height, plane : MeshGeneration.Plane.YZ);
        w4.transform.parent = go.transform;
        w4.transform.localPosition += new Vector3(width, 0,0);
        drm.SetValues(width,depth,height,roomPoint);
        return go;
    }

    public void ApplyToMask(Mask m){
        for(int i = roomPoint.x; i < roomPoint.x + width; i++){
            for(int j = roomPoint.z; j < roomPoint.z + depth; j++){
                m[i,j] = true;
            }
        }
    }

    public void RoomsFreeMask(Mask m){
        for(int i = 0; i < free.GetLength(0); i++){
            for(int j = 0; j < free.GetLength(1); j++){
                m[roomPoint.x+i, roomPoint.z+j] = free[i,j];
            }
        }
    }

    public Vector3Int RoomPoint{
        get{return roomPoint;}
    }
    public Vector3Int LowerLeftPoint{
        get{return roomPoint;}
    }
    public Vector3Int UpperLeftPoint{
        get{return new Vector3Int(roomPoint.x, roomPoint.y, roomPoint.z + depth);}
    }
    public Vector3Int UpperRightPoint{
        get{return new Vector3Int(roomPoint.x + width, roomPoint.y, roomPoint.z + depth);}
    }
    public Vector3Int LowerRightPoint{
        get{return new Vector3Int(roomPoint.x + width, roomPoint.y, roomPoint.z);}
    }
    public List<DungeonCorridor> Corridors{
        get{return corridors;}
    }
    public int Width{
        get{return width;}
        set{width = value;}
    }
    public int Depth{
        get{return depth;}
        set{depth = value;}
    }
    public int Type{
        get{return type;}
        set{type = value;}
    }
    public List<Tuple<Vector3Int,int>> SpawnPoints{
        get{return spawnPoints;}
    }
}
public class DungeonRoomMeta : MonoBehaviour{
    public int width, depth;
    public float height;
    public Vector3 roomPoint;


    public DungeonRoomMeta(){

    }


    public void SetValues(int width, int depth, float height, Vector3 roomPoint){
        this.width = width;
        this.depth = depth;
        this.height = height;
        this.roomPoint = roomPoint;
    }
}