using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class DungeonTreeMeta : MonoBehaviour{
    public int[] partitionSize;
    public int[] point;
    public Vector3 roomPoint;
    public DungeonTreeMeta(){

    }


    public void SetValues(int[] size, int[] point){
        this.partitionSize = size;
        this.point = point;
    }
    public Vector3 RoomPoint{
        get{return roomPoint;}
        set{roomPoint = value;}
    }
}

public class DungeonRoom : IGameObjectable{
    private int width, depth;
    private float height;
    private List<DungeonCorridor> corridors;
    private bool[,] free;
    private System.Random rand;
    private Vector3 roomPoint;


    public enum Face{
        Left, Right, Front, Back, Top, Bottom
    }


    public DungeonRoom(int width, int depth, float height, Vector3 roomPoint, System.Random rand = null){
        this.width = width;
        this.depth = depth;
        this.height = height;
        this.roomPoint = roomPoint;
        free = new bool[width,depth];
        rand = rand ?? new System.Random();
    }
    public DungeonRoom(IEnumerable<int> x, Vector3 roomPoint, System.Random rand = null) : this(x.ToArray()[0],x.ToArray()[1],x.ToArray()[2], roomPoint, rand : rand){}

    //doesnt work if both points are on the same boundary
    public void SetFreePath(int px, int pz, int qx, int qz){
        
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
        drm.SetValues(width,depth,height);
        return go;
    }

    public Vector3 RoomPoint{
        get{return roomPoint;}
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
}
public class DungeonRoomMeta : MonoBehaviour{
    public int width, depth;
    public float height;
    public Vector3 roomPoint;


    public DungeonRoomMeta(){

    }


    public void SetValues(int width, int depth, float height){
        this.width = width;
        this.depth = depth;
        this.height = height;
    }
}


public class DungeonCorridor : IGameObjectable {
    private List<Vector3> path;
    private float width, height;
    public DungeonRoom startRoom, endRoom;
    public DungeonRoom.Face startFace, endFace;
    public float startPoint, endPoint;

    public DungeonCorridor(){}

    public GameObject ToGameObject(){
        GameObject go = new GameObject("DungeonCorridor");
        DungeonCorridorMeta dcm = go.AddComponent<DungeonCorridorMeta>();
        dcm.SetValues(path);
        MeshGeneration.CorridorGround(path,width).transform.parent = go.transform;
        return go;
    }


    public List<Vector3> Path{
        get{return path;}
        set{path = value;}
    }
    public float Width{
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
    public float StartPoint{
        get{return startPoint;}
        set{startPoint = value;}
    }
}
class DungeonCorridorMeta : MonoBehaviour{
    public List<Vector3> path;
    public DungeonCorridorMeta(){
    }
    public void SetValues(List<Vector3> path){
        this.path = path;
    }
}


public class DimensionMismatchException : Exception{

    public DimensionMismatchException(int a, int b) :
        base(String.Format("Dimensions are not equal for values {1} and {1}",a,b)){}

    public DimensionMismatchException(string s) : base(s){}

}


public class DungeonTreeNode : SPTreeNode{
    private Vector3 roomPoint;
    private DungeonRoom room;
    private List<DungeonCorridor> corridors;
    private Tuple<int,int>[] minMaxMargin;
    private Tuple<int,int> minMaxHeight;
    private Func<SPTreeNode,float> fHeight;

    private List<DungeonRoom> roomsNorth, roomsEast, roomsSouth, roomsWest, roomsBelow, roomsAbove;

    public DungeonTreeNode(int[] size, System.Random rand = null) : base(size,rand){
        minMaxMargin = new Tuple<int,int>[Dim];
        for(int i = 0; i < Dim; i++){
            minMaxMargin[i] = new Tuple<int,int>(0,0);
        }
        //fHeight = fHeightSizeBased();
        corridors = new List<DungeonCorridor>();
        roomsNorth = new(); roomsEast = new(); roomsSouth = new(); roomsWest = new(); roomsBelow = new(); roomsAbove = new();
    }
    //fix me
    public DungeonTreeNode(SPTreeNode node) : this(node.Size, node.Rand){
        this.point = node.Point;
        this.dim = node.Dim;
        this.degree = node.Degree;
    }

    public static Func<SPTreeNode,float> fHeight2DMinMax(int min, int max){
    return x => {
        return x.Rand.Next(min, max+1);
    };}
    public static Func<SPTreeNode,float> fHeightSizeBased(int min = 2, int max = int.MaxValue, float scale = 1f){
        return x => MathF.Min(MathF.Max(min, x.Size[0]+x.Size[1]*scale), max);
    }   

    public Vector3 PointV{
        get{return point.Length == 2 ? new Vector3(point[0], 0 , point[1]) : new Vector3(point[0], point[2], point[1]);}
    }
    public Tuple<int,int>[] MinMaxMargin{
    get{ return minMaxMargin;}
    set{
        if(value.Length != dim) throw new DimensionMismatchException(String.Format("Dimension of MinMaxMargin Array (value: {1}) is not equal to the dimension of the DungeonTree (value = {1}).", value, dim));
        else minMaxMargin = value;
        }
    }    
    public DungeonRoom Room{
        get{return room;}
        set{room = value;}
    }
    public Vector3 RoomPoint{
        get{return roomPoint;}
        set{roomPoint = value;}
    }
    public List<DungeonCorridor> Corridors{
        get{return corridors;}
        set{corridors = value;}
    }
    public List<DungeonRoom> RoomsNorth{get {return roomsNorth;}}
    public List<DungeonRoom> RoomsEast{get {return roomsEast;}}
    public List<DungeonRoom> RoomsSouth{get {return roomsSouth;}}
    public List<DungeonRoom> RoomsWest{get {return roomsWest;}}
    public List<DungeonRoom> RoomsBelow{get {return roomsBelow;}}
    public List<DungeonRoom> RoomsAbove{get {return roomsAbove;}}
    public Func<SPTreeNode, float> FHeight{
            get {return fHeight;}
            set {fHeight = value;}
        }
}


public class DungeonTreeT : Tree<DungeonTreeNode> {
    public DungeonTreeT(DungeonTreeNode node) : base(node){}
    public DungeonTreeT(SPTreeT tree) : this(new DungeonTreeNode(tree.Node)){
        ConstructMapChildren(tree);
    }
    public void ConstructMapChildren(SPTreeT spTree){
        foreach(SPTreeT spChild in spTree.Children){
            DungeonTreeT dChild = new DungeonTreeT(new DungeonTreeNode(spChild.Node)) ;
            AddChild(dChild);
            dChild.ConstructMapChildren(spChild);
        }
    }


    //p: probability to place a room in leaf * 100
    public void PlaceRooms(int p = -1){
        Tuple<int,int>[] rMinMaxMargin = root.Node.MinMaxMargin;
        if(IsLeaf() && (p == -1 || (node.Rand.Next(1,101) <= p)) ){
            int leftMargin = node.Rand.Next(rMinMaxMargin[0].Item1,rMinMaxMargin[0].Item2+1);
            int rightMargin = node.Rand.Next(rMinMaxMargin[0].Item1,rMinMaxMargin[0].Item2+1);
            int depthLowerMargin = node.Rand.Next(rMinMaxMargin[1].Item1,rMinMaxMargin[1].Item2+1);
            int depthUpperMargin = node.Rand.Next(rMinMaxMargin[1].Item1,rMinMaxMargin[1].Item2+1);
           if(node.Dim == 2){
                node.RoomPoint = new Vector3(node.Point[0]+leftMargin, 0, node.Point[1] + depthLowerMargin);
                node.Room = new DungeonRoom(node.Size[0]-leftMargin-rightMargin, node.Size[1]-depthLowerMargin-depthUpperMargin, root.Node.FHeight(node), node.RoomPoint);
                
           }
           else if(node.Dim == 3){
                int heightLowerMargin = node.Dim > 2 ? node.Rand.Next(rMinMaxMargin[2].Item1,rMinMaxMargin[2].Item2+1) : 0;
                int heightUpperMargin = node.Dim > 2 ? node.Rand.Next(rMinMaxMargin[2].Item1,rMinMaxMargin[2].Item2+1) : 0;
                node.RoomPoint = new Vector3(node.Point[0]+leftMargin, node.Point[2] + heightLowerMargin, node.Point[1] + depthLowerMargin);
                node.Room = new DungeonRoom(node.Size[0]-leftMargin-rightMargin, node.Size[1]-depthLowerMargin-depthUpperMargin, node.Size[2]-heightLowerMargin-heightUpperMargin, node.RoomPoint);
                
           }
            node.RoomsNorth.Add(node.Room); node.RoomsEast.Add(node.Room); node.RoomsSouth.Add(node.Room); node.RoomsWest.Add(node.Room);
        }
        else{
            foreach(DungeonTreeT c in children){
                c.PlaceRooms( p : p);
            }
            switch(children.Count){
                //fix me
                //case 2d tree only
                case 2:
                    if(node.Size[0] == children[0].Node.Size[0]){
                        node.RoomsNorth.AddRange(children[1].Node.RoomsNorth);
                        if(children[1].Node.RoomsNorth.Count == 0) node.RoomsNorth.AddRange(children[0].Node.RoomsNorth);
                        node.RoomsSouth.AddRange(children[0].Node.RoomsSouth);
                        if(children[0].Node.RoomsSouth.Count == 0) node.RoomsSouth.AddRange(children[1].Node.RoomsSouth);
                        node.RoomsEast.AddRange(children[0].Node.RoomsEast);
                        node.RoomsEast.AddRange(children[1].Node.RoomsEast);
                        node.RoomsWest.AddRange(children[0].Node.RoomsWest);
                        node.RoomsWest.AddRange(children[1].Node.RoomsWest);
                    }
                    else{
                        node.RoomsNorth.AddRange(children[0].Node.RoomsNorth);
                        node.RoomsNorth.AddRange(children[1].Node.RoomsNorth);
                        node.RoomsSouth.AddRange(children[0].Node.RoomsSouth);
                        node.RoomsSouth.AddRange(children[1].Node.RoomsSouth);
                        node.RoomsEast.AddRange(children[1].Node.RoomsEast);
                        if(children[1].Node.RoomsEast.Count == 0) node.RoomsEast.AddRange(children[0].Node.RoomsEast);
                        node.RoomsWest.AddRange(children[0].Node.RoomsWest);
                        if(children[0].Node.RoomsWest.Count == 0) node.RoomsWest.AddRange(children[1].Node.RoomsWest);
                    }
                    break;
                case 4:
                    break;
                case 8:
                    break;
                default:
                    break;
            }
        }
    }


    public void PlaceCorridors(float minWidth, float maxWidth, float minHeight, float maxHeight){
        float width = (float)node.Rand.NextDouble()*(maxWidth-minWidth)+minWidth;
        float height = (float)node.Rand.NextDouble()*(maxHeight-minHeight)+minHeight;
        switch(children.Count){
            //fix me
            //case 2d tree only
            case 2:
                if((node.Size[0] == children[0].Node.Size[0] && children[0].Node.RoomsNorth.Count > 0 && children[1].Node.RoomsSouth.Count > 0) || node.Size[1] == children[0].Node.Size[1] && children[0].Node.RoomsEast.Count > 0 && children[1].Node.RoomsWest.Count > 0){
                    Vector3 start,mid,mid2,end;
                    DungeonRoom startRoom, endRoom;
                    DungeonRoom.Face startFace, endFace;
                    bool p = false;
                    if(node.Size[0] == children[0].Node.Size[0] && children[0].Node.RoomsNorth.Count > 0 && children[1].Node.RoomsSouth.Count > 0){
                        p = true;
                        int cri1 = node.Rand.Next(0,children[0].Node.RoomsNorth.Count);
                        int cri2 = node.Rand.Next(0,children[1].Node.RoomsSouth.Count);
                        startRoom = children[0].Node.RoomsNorth[cri1];
                        endRoom = children[1].Node.RoomsSouth[cri2];
                        float startPoint = (float)node.Rand.NextDouble() * ((startRoom.Width - width) - width) + width;
                        float endPoint = (float)node.Rand.NextDouble() * ((endRoom.Width - width) - width) + width; 
                        start = startRoom.RoomPoint - node.PointV + new Vector3(startPoint,0,startRoom.Depth);
                        mid = new Vector3(start.x,start.y, children[0].Node.Size[1]);
                        end = endRoom.RoomPoint - node.PointV + new Vector3(endPoint,0,0);
                        mid2 = new Vector3(end.x,end.y, children[0].Node.Size[1]);
                        startFace = DungeonRoom.Face.Back;
                        endFace = DungeonRoom.Face.Front;
                    }   
                    else{
                        p = true;
                        int cri1 = node.Rand.Next(0,children[0].Node.RoomsEast.Count);
                        int cri2 = node.Rand.Next(0,children[1].Node.RoomsWest.Count);
                        startRoom = children[0].Node.RoomsEast[cri1];
                        endRoom = children[1].Node.RoomsWest[cri2];
                        float startPoint = (float)node.Rand.NextDouble() * ((startRoom.Depth - width) - width) + width;
                        float endPoint = (float)node.Rand.NextDouble() * ((endRoom.Depth - width) - width) + width; 
                        start = startRoom.RoomPoint - node.PointV + new Vector3(startRoom.Width,0,startPoint);
                        mid = new Vector3(children[0].Node.Size[0],start.y, start.z);
                        end = endRoom.RoomPoint - node.PointV + new Vector3(0,0,endPoint);
                        mid2 = new Vector3(children[0].Node.Size[0],end.y, end.z);
                        startFace = DungeonRoom.Face.Right;
                        endFace = DungeonRoom.Face.Left;
                    }
                    DungeonCorridor corridor = new DungeonCorridor();
                    corridor.StartRoom = startRoom;
                    corridor.StartRoom = endRoom;
                    corridor.StartFace = startFace;
                    corridor.EndFace =endFace;
                    corridor.Width = width;
                    corridor.Height = height;
                    corridor.Path = new List<Vector3>(){start,mid,mid2,end};
                    node.Corridors.Add(corridor);
                    }
                break;
            case 4:
                break;
            case 8:
                break;
            default:
                break;
        }
        foreach(DungeonTreeT child in children) child.PlaceCorridors(minWidth, maxWidth, minHeight, maxHeight);
    }


    public GameObject ToGameObject(){
        GameObject go = new GameObject("DungeonTreeT");
        DungeonTreeMeta dtm = go.AddComponent<DungeonTreeMeta>();
        dtm.SetValues(node.Size, node.Point);
        if(node.Room != null) dtm.RoomPoint = node.RoomPoint;
        foreach(DungeonCorridor corridor in node.Corridors){
            corridor.ToGameObject().transform.parent = go.transform;
        }
        if(node.Room != null){
            GameObject rgo = node.Room.ToGameObject();
            rgo.transform.parent = go.transform;
            rgo.transform.localPosition += node.RoomPoint- new Vector3(node.Point[0],0,node.Point[1]);
        }
        foreach(DungeonTreeT c in children){
            GameObject cgo = c.ToGameObject();
            cgo.transform.parent = go.transform;
            cgo.transform.localPosition += new Vector3(c.node.Point[0],0,c.node.Point[1]) - new Vector3(node.Point[0],0,node.Point[1]);
        }
        return go;
    }
}