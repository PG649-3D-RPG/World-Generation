using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonTreeMeta : MonoBehaviour{
    public int[] partitionSize;
    public int[] point;
    public Vector3Int roomPoint;
    public int northRooms,eastRooms,southRooms,westRooms;
    public DungeonTreeMeta(){

    }
    public void SetValues(DungeonTreeNode node){
        partitionSize = node.Size;
        point = node.Point;
        northRooms = node.RoomsNorth.Count;
        eastRooms = node.RoomsEast.Count;
        southRooms = node.RoomsSouth.Count;
        westRooms = node.RoomsWest.Count;
    }

    public void SetValues(int[] size, int[] point){
        this.partitionSize = size;
        this.point = point;
    }
    public Vector3Int RoomPoint{
        get{return roomPoint;}
        set{roomPoint = value;}
    }
}

public class DungeonRoom : IGameObjectable{
    private int width, depth;
    private float height;
    private List<DungeonCorridor> corridors;
    private List<Tuple<Face,int,int>> corridorPoints;
    private bool[,] free;
    private System.Random rand;
    private Vector3Int roomPoint;
    private int type;


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
        rand ??= new System.Random();
        corridorPoints = new List<Tuple<Face,int,int>>();
        this.type = 0;
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

    public GameObject ToGameObjectTerrain(){
        GameObject go = new GameObject("DungeonRoom");
        Terrain terrain = go.AddComponent<Terrain>();
        terrain.terrainData = new TerrainData();
        terrain.materialTemplate = new Material(Shader.Find("Nature/Terrain/Diffuse"));
        EnvironmentGeneratorSettings envSettings = ScriptableObject.CreateInstance<EnvironmentGeneratorSettings>();
        envSettings.Depth = depth/10; 
        envSettings.Scale = 5f;
        envSettings.TerrainSizeX = width;
        envSettings.TerrainSizeY = depth;
        envSettings.GenerateHeights = true;
        envSettings.GenerateBorders = true;
        envSettings.UseSmoothing = false;
        envSettings.GenerateObstacles = false;
        EnvironmentGenerator generator = new EnvironmentGenerator(ref terrain, envSettings,corridorPoints);
        generator.Build();
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


public class DimensionMismatchException : Exception{

    public DimensionMismatchException(int a, int b) :
        base(String.Format("Dimensions are not equal for values {1} and {1}",a,b)){}

    public DimensionMismatchException(string s) : base(s){}

}


public class DungeonTreeNode : SPTreeNode{
    private Vector3Int roomPoint;
    private DungeonRoom room;
    private List<DungeonCorridor> corridors;
    private Tuple<int,int>[] minMaxMargin;
    private Tuple<int,int> minMaxHeight;
    private Func<SPTreeNode,float> fHeight;
    private int numberOfTypes;

    private List<DungeonRoom> roomsNorth, roomsEast, roomsSouth, roomsWest, roomsBelow, roomsAbove;

    public DungeonTreeNode(int[] size, System.Random rand = null) : base(size,rand){
        minMaxMargin = new Tuple<int,int>[Dim];
        for(int i = 0; i < Dim; i++){
            minMaxMargin[i] = new Tuple<int,int>(0,0);
        }
        //fHeight = fHeightSizeBased();
        corridors = new List<DungeonCorridor>();
        roomsNorth = new(); roomsEast = new(); roomsSouth = new(); roomsWest = new(); roomsBelow = new(); roomsAbove = new();
        numberOfTypes = 1;
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

    public Vector3Int PointV{
        get{return point.Length == 2 ? new Vector3Int(point[0], 0 , point[1]) : new Vector3Int(point[0], point[2], point[1]);}
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
    public Vector3Int RoomPoint{
        get{return roomPoint;}
        set{roomPoint = value;}
    }
    public List<DungeonCorridor> Corridors{
        get{return corridors;}
        set{corridors = value;}
    }
    public int NumberOfTypes{
        get{return numberOfTypes;}
        set{numberOfTypes = value;}
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


    public void PlaceRooms(int p = -1, bool quadraticTerrain = false, int quadraticTerrainMin = 0, int quadraticTerrainMax = 0){
        Tuple<int,int>[] rMinMaxMargin = root.Node.MinMaxMargin;
        if(IsLeaf() && (p == -1 || (node.Rand.Next(1,101) <= p)) ){
            int leftMargin = node.Rand.Next(rMinMaxMargin[0].Item1,rMinMaxMargin[0].Item2+1);
            int rightMargin = node.Rand.Next(rMinMaxMargin[0].Item1,rMinMaxMargin[0].Item2+1);
            int depthLowerMargin = node.Rand.Next(rMinMaxMargin[1].Item1,rMinMaxMargin[1].Item2+1);
            int depthUpperMargin = node.Rand.Next(rMinMaxMargin[1].Item1,rMinMaxMargin[1].Item2+1);
            if(quadraticTerrain){
                int marginDivide = node.Rand.Next(quadraticTerrainMin, quadraticTerrainMax + 1);
                int margin = (node.Size[0] - (node.Size[0] / (int)Math.Pow(2, marginDivide)))/2; 
                leftMargin = margin;
                rightMargin = margin;
                depthLowerMargin = margin;
                depthUpperMargin = margin;
            }
           if(node.Dim == 2){
                node.RoomPoint = new Vector3Int(node.Point[0]+leftMargin, 0, node.Point[1] + depthLowerMargin);
                node.Room = new DungeonRoom(node.Size[0]-leftMargin-rightMargin, node.Size[1]-depthLowerMargin-depthUpperMargin, root.Node.FHeight(node), node.RoomPoint, rand : node.Rand);
                
           }
           else if(node.Dim == 3){
                int heightLowerMargin = node.Dim > 2 ? node.Rand.Next(rMinMaxMargin[2].Item1,rMinMaxMargin[2].Item2+1) : 0;
                int heightUpperMargin = node.Dim > 2 ? node.Rand.Next(rMinMaxMargin[2].Item1,rMinMaxMargin[2].Item2+1) : 0;
                node.RoomPoint = new Vector3Int(node.Point[0]+leftMargin, node.Point[2] + heightLowerMargin, node.Point[1] + depthLowerMargin);
                node.Room = new DungeonRoom(node.Size[0]-leftMargin-rightMargin, node.Size[1]-depthLowerMargin-depthUpperMargin, node.Size[2]-heightLowerMargin-heightUpperMargin, node.RoomPoint, rand : node.Rand);
                
           }
            node.RoomsNorth.Add(node.Room); node.RoomsEast.Add(node.Room); node.RoomsSouth.Add(node.Room); node.RoomsWest.Add(node.Room);
        }
        else{
            foreach(DungeonTreeT c in children){
                c.PlaceRooms( p : p, quadraticTerrain : quadraticTerrain, quadraticTerrainMin : quadraticTerrainMin, quadraticTerrainMax : quadraticTerrainMax);
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
                    node.RoomsNorth.AddRange(children[2].Node.RoomsNorth);
                    node.RoomsNorth.AddRange(children[3].Node.RoomsNorth);
                    if(children[2].Node.RoomsNorth.Count == 0 && children[3].Node.RoomsNorth.Count == 0){
                        node.RoomsNorth.AddRange(children[0].Node.RoomsNorth);
                        node.RoomsNorth.AddRange(children[1].Node.RoomsNorth);
                    }
                    node.RoomsEast.AddRange(children[1].Node.RoomsEast);
                    node.RoomsEast.AddRange(children[3].Node.RoomsEast);
                    if(children[1].Node.RoomsEast.Count == 0 && children[3].Node.RoomsEast.Count == 0){
                        node.RoomsEast.AddRange(children[0].Node.RoomsEast);
                        node.RoomsEast.AddRange(children[2].Node.RoomsEast);
                    }
                    node.RoomsSouth.AddRange(children[0].Node.RoomsSouth);
                    node.RoomsSouth.AddRange(children[1].Node.RoomsSouth);
                    if(children[0].Node.RoomsSouth.Count == 0 && children[1].Node.RoomsSouth.Count == 0){
                        node.RoomsSouth.AddRange(children[2].Node.RoomsSouth);
                        node.RoomsSouth.AddRange(children[3].Node.RoomsSouth);
                    }
                    node.RoomsWest.AddRange(children[0].Node.RoomsWest);
                    node.RoomsWest.AddRange(children[2].Node.RoomsWest);
                    if(children[0].Node.RoomsWest.Count == 0 && children[2].Node.RoomsWest.Count == 0){
                        node.RoomsWest.AddRange(children[1].Node.RoomsWest);
                        node.RoomsWest.AddRange(children[3].Node.RoomsWest);
                    }
                    break;
                default:
                    break;
            }
        }
    }


    public void PlaceCorridors(int minWidth, int maxWidth, float minHeight, float maxHeight, float maxDistance = float.MaxValue){
        switch(children.Count){
            case 2:
                PlaceCorridor2(minWidth,maxWidth, minHeight, maxHeight, maxDistance : maxDistance);   
                break;
            case 4:
                PlaceCorridors4(minWidth, maxWidth, minHeight, maxHeight);
                break;
            case 8:
                break;
            default:
                break;
        }
        foreach(DungeonTreeT child in children) child.PlaceCorridors(minWidth, maxWidth, minHeight, maxHeight, maxDistance : maxDistance);
    }
    public int FindConnectingRoom(Vector3 fromPoint, List<DungeonRoom> roomList, DungeonRoom.Face face, float maxDistance){
        List<int> points = new List<int>();
        float minDistance = float.MaxValue;
        int minDistanceIndex = 0;
        for(int i = 0; i < roomList.Count; i++){
            Vector3 toPoint = roomList[i].FacePoint(face);
            float d = Vector3.Distance(fromPoint, toPoint); 
            if(d <= maxDistance){
                points.Add(i);
            }
            if(d <= minDistance){
                minDistance = d;
                minDistanceIndex = i;
            }
        }
        if(points.Count != 0){
            return points[node.Rand.Next(0,points.Count)];
        }
        else{
            return minDistanceIndex;
        }
    }

    //fix remove mid2 if mid = mid2
    private void PlaceCorridor2(int minWidth, int maxWidth, float minHeight, float maxHeight, float maxDistance = float.MaxValue){
        int width = node.Rand.Next(minWidth, maxWidth+1);
        float height = (float)node.Rand.NextDouble()*(maxHeight-minHeight)+minHeight;
        if((node.Size[0] == children[0].Node.Size[0] && children[0].Node.RoomsNorth.Count > 0 && children[1].Node.RoomsSouth.Count > 0) || node.Size[1] == children[0].Node.Size[1] && children[0].Node.RoomsEast.Count > 0 && children[1].Node.RoomsWest.Count > 0){
            Vector3Int start,mid,mid2,end;
            DungeonRoom startRoom, endRoom;
            DungeonRoom.Face startFace, endFace;
            DungeonCorridor corridor = new DungeonCorridor();
            if(node.Size[0] == children[0].Node.Size[0] && children[0].Node.RoomsNorth.Count > 0 && children[1].Node.RoomsSouth.Count > 0){
                int cri1 = node.Rand.Next(0,children[0].Node.RoomsNorth.Count);
                startRoom = children[0].Node.RoomsNorth[cri1];
                int cri2 = FindConnectingRoom(startRoom.UpperLeftPoint, children[1].Node.RoomsSouth, DungeonRoom.Face.Front, maxDistance);
                endRoom = children[1].Node.RoomsSouth[cri2];
                int startPointIndex = node.Rand.Next(0,startRoom.Width - width+1);
                int endPointIndex = node.Rand.Next(0,endRoom.Width - width+1);
                corridor.StartIndex = startPointIndex;
                corridor.EndIndex = endPointIndex;
                start = startRoom.RoomPoint - node.PointV + new Vector3Int(startPointIndex,0,startRoom.Depth);
                mid = new Vector3Int(start.x,start.y, children[0].Node.Size[1]);
                end = endRoom.RoomPoint - node.PointV + new Vector3Int(endPointIndex,0,0);
                mid2 = new Vector3Int(end.x,end.y, children[0].Node.Size[1]);
                startFace = DungeonRoom.Face.Back;
                endFace = DungeonRoom.Face.Front;
                startRoom.AddCorridorPoint(new Tuple<DungeonRoom.Face, int, int>(startFace, startPointIndex, width));
                endRoom.AddCorridorPoint(new Tuple<DungeonRoom.Face, int, int>(endFace, endPointIndex,width));
            }
            else{
                int cri1 = node.Rand.Next(0,children[0].Node.RoomsEast.Count);
                startRoom = children[0].Node.RoomsEast[cri1];
                //int cri2 = node.Rand.Next(0,children[1].Node.RoomsWest.Count);
                int cri2 = FindConnectingRoom(startRoom.LowerRightPoint, children[1].Node.RoomsWest, DungeonRoom.Face.Left, maxDistance);
                endRoom = children[1].Node.RoomsWest[cri2];
                int startPointIndex = node.Rand.Next(0,startRoom.Depth - width+1);
                float startPoint = startPointIndex;// + ((float)width/2);
                //float startPoint = (float)node.Rand.NextDouble() * ((startRoom.Depth - width) - width) + width;
                //float endPoint = (float)node.Rand.NextDouble() * ((endRoom.Depth - width) - width) + width; 
                int endPointIndex = node.Rand.Next(0,endRoom.Depth - width+1);
                float endPoint = endPointIndex;// + ((float)width/2);
                corridor.StartIndex = startPointIndex;
                corridor.EndIndex = endPointIndex;
                start = startRoom.RoomPoint - node.PointV + new Vector3Int(startRoom.Width,0,startPointIndex);
                mid = new Vector3Int(children[0].Node.Size[0],start.y, start.z);
                end = endRoom.RoomPoint - node.PointV + new Vector3Int(0,0,endPointIndex);
                mid2 = new Vector3Int(children[0].Node.Size[0],end.y, end.z);
                startFace = DungeonRoom.Face.Right;
                endFace = DungeonRoom.Face.Left;
                startRoom.AddCorridorPoint(new Tuple<DungeonRoom.Face, int, int>(startFace, startPointIndex,width));
                endRoom.AddCorridorPoint(new Tuple<DungeonRoom.Face, int, int>(endFace, endPointIndex,width));
            }
            corridor.PartitionPoint = node.PointV;
            corridor.StartRoom = startRoom;
            corridor.StartRoom = endRoom;
            corridor.StartFace = startFace;
            corridor.EndFace = endFace;
            corridor.Width = width;
            corridor.Height = height;
            corridor.Path = new List<Vector3Int>(){start,mid,mid2,end};
            node.Corridors.Add(corridor);
        }
    }

    private void PlaceCorridors4(int minWidth, int maxWidth, float minHeight, float maxHeight){
        if(children[0].Node.RoomsEast.Count > 0 && children[1].Node.RoomsWest.Count > 0){
            int width = node.Rand.Next(minWidth, maxWidth+1);
            float height = (float)node.Rand.NextDouble()*(maxHeight-minHeight)+minHeight;
            DungeonCorridor corridor = new DungeonCorridor();
            corridor.StartRoom = children[0].Node.RoomsEast[node.Rand.Next(0,children[0].Node.RoomsEast.Count)];
            corridor.EndRoom = children[1].Node.RoomsWest[node.Rand.Next(0,children[1].Node.RoomsWest.Count)];
            corridor.StartIndex = node.Rand.Next(0,corridor.StartRoom.Depth-width+1);
            corridor.EndIndex = node.Rand.Next(0,corridor.EndRoom.Depth-width+1);
            Vector3Int start = corridor.StartRoom.RoomPoint - node.PointV + new Vector3Int(corridor.StartRoom.Width,0,corridor.StartIndex);
            corridor.Path.Add(start);
            Vector3Int end = corridor.EndRoom.RoomPoint - node.PointV + new Vector3Int(0,0,corridor.EndIndex);
            if(start.z != end.z){
                Vector3Int mid = new Vector3Int(children[0].Node.Size[0],start.y, start.z);
                Vector3Int mid2 = new Vector3Int(children[0].Node.Size[0], end.y, end.z);
                corridor.Path.Add(mid);
                corridor.Path.Add(mid2);
            }
            corridor.Path.Add(end);
            corridor.Width = width;
            corridor.Height = height;
            corridor.StartFace = DungeonRoom.Face.Right;
            corridor.EndFace = DungeonRoom.Face.Left;
            corridor.PartitionPoint = node.PointV;
            node.Corridors.Add(corridor);
        }
        if(children[1].Node.RoomsNorth.Count > 0 && children[3].Node.RoomsSouth.Count > 0){
            int width = node.Rand.Next(minWidth, maxWidth+1);
            float height = (float)node.Rand.NextDouble()*(maxHeight-minHeight)+minHeight;
            DungeonCorridor corridor = new DungeonCorridor();
            corridor.StartRoom = children[1].Node.RoomsNorth[node.Rand.Next(0,children[1].Node.RoomsNorth.Count)];
            corridor.EndRoom = children[3].Node.RoomsSouth[node.Rand.Next(0,children[3].Node.RoomsSouth.Count)];
            corridor.StartIndex = node.Rand.Next(0,corridor.StartRoom.Width-width+1);
            corridor.EndIndex = node.Rand.Next(0,corridor.EndRoom.Width-width+1);
            Vector3Int start = corridor.StartRoom.RoomPoint - node.PointV + new Vector3Int(corridor.StartIndex,0,corridor.StartRoom.Depth);
            corridor.Path.Add(start);
            Vector3Int end = corridor.EndRoom.RoomPoint - node.PointV + new Vector3Int(corridor.EndIndex,0,0);
            if(start.z != end.z){
                Vector3Int mid = new Vector3Int(start.x,start.y, children[1].Node.Size[1]);
                Vector3Int mid2 = new Vector3Int(end.x, end.y, children[1].Node.Size[1]);
                corridor.Path.Add(mid);
                corridor.Path.Add(mid2);
            }
            corridor.Path.Add(end);
            corridor.Width = width;
            corridor.Height = height;
            corridor.StartFace = DungeonRoom.Face.Back;
            corridor.EndFace = DungeonRoom.Face.Front;
            corridor.PartitionPoint = node.PointV;
            node.Corridors.Add(corridor);
        }
        if(children[2].Node.RoomsEast.Count > 0 && children[3].Node.RoomsWest.Count > 0){
            int width = node.Rand.Next(minWidth, maxWidth+1);
            float height = (float)node.Rand.NextDouble()*(maxHeight-minHeight)+minHeight;
            DungeonCorridor corridor = new DungeonCorridor();
            corridor.StartRoom = children[2].Node.RoomsEast[node.Rand.Next(0,children[2].Node.RoomsEast.Count)];
            corridor.EndRoom = children[3].Node.RoomsWest[node.Rand.Next(0,children[3].Node.RoomsWest.Count)];
            corridor.StartIndex = node.Rand.Next(0,corridor.StartRoom.Depth-width+1);
            corridor.EndIndex = node.Rand.Next(0,corridor.EndRoom.Depth-width+1);
            Vector3Int start = corridor.StartRoom.RoomPoint - node.PointV + new Vector3Int(corridor.StartRoom.Width,0,corridor.StartIndex);
            corridor.Path.Add(start);
            Vector3Int end = corridor.EndRoom.RoomPoint - node.PointV + new Vector3Int(0,0,corridor.EndIndex);
            if(start.z != end.z){
                Vector3Int mid = new Vector3Int(children[0].Node.Size[0],start.y, start.z);
                Vector3Int mid2 = new Vector3Int(children[0].Node.Size[0], end.y, end.z);
                corridor.Path.Add(mid);
                corridor.Path.Add(mid2);
            }
            corridor.Path.Add(end);
            corridor.Width = width;
            corridor.Height = height;
            corridor.StartFace = DungeonRoom.Face.Right;
            corridor.EndFace = DungeonRoom.Face.Left;
            corridor.PartitionPoint = node.PointV;
            node.Corridors.Add(corridor);
        }
        if(children[0].Node.RoomsNorth.Count > 0 && children[2].Node.RoomsSouth.Count > 0){
            int width = node.Rand.Next(minWidth, maxWidth+1);
            float height = (float)node.Rand.NextDouble()*(maxHeight-minHeight)+minHeight;
            DungeonCorridor corridor = new DungeonCorridor();
            corridor.StartRoom = children[0].Node.RoomsNorth[node.Rand.Next(0,children[0].Node.RoomsNorth.Count)];
            corridor.EndRoom = children[2].Node.RoomsSouth[node.Rand.Next(0,children[2].Node.RoomsSouth.Count)];
            corridor.StartIndex = node.Rand.Next(0,corridor.StartRoom.Width-width+1);
            corridor.EndIndex = node.Rand.Next(0,corridor.EndRoom.Width-width+1);
            Vector3Int start = corridor.StartRoom.RoomPoint - node.PointV + new Vector3Int(corridor.StartIndex,0,corridor.StartRoom.Depth);
            corridor.Path.Add(start);
            Vector3Int end = corridor.EndRoom.RoomPoint - node.PointV + new Vector3Int(corridor.EndIndex,0,0);
            if(start.z != end.z){
                Vector3Int mid = new Vector3Int(start.x,start.y, children[1].Node.Size[1]);
                Vector3Int mid2 = new Vector3Int(end.x, end.y, children[1].Node.Size[1]);
                corridor.Path.Add(mid);
                corridor.Path.Add(mid2);
            }
            corridor.Path.Add(end);
            corridor.Width = width;
            corridor.Height = height;
            corridor.StartFace = DungeonRoom.Face.Back;
            corridor.EndFace = DungeonRoom.Face.Front;
            corridor.PartitionPoint = node.PointV;
            node.Corridors.Add(corridor);
        }
    }

    public void AssignTypes(int n, float[] p = null){
        node.NumberOfTypes = n;
        if(node.Room != null){
            if(p != null){
                float a = 0f;
                double d = node.Rand.NextDouble();
                for(int i = 0; i < p.Length; i++){
                    a += p[i];
                    if(d <= a){
                        node.Room.Type = i;
                        break;
                    }
                }
            }
            node.Room.Type = node.Rand.Next(0,n);
        }
        foreach(DungeonTreeT c in children){
            c.AssignTypes(n, p);
        }
    }


    public GameObject ToGameObject(bool terrain = false){
        GameObject go = new GameObject("DungeonTreeT");
        DungeonTreeMeta dtm = go.AddComponent<DungeonTreeMeta>();
        //dtm.SetValues(node.Size, node.Point);
        dtm.SetValues(node);
        if(node.Room != null) dtm.RoomPoint = node.RoomPoint;
        foreach(DungeonCorridor corridor in node.Corridors){
            corridor.ToGameObject().transform.parent = go.transform;
        }
        if(node.Room != null){
            GameObject rgo = terrain ? node.Room.ToGameObjectTerrain() : node.Room.ToGameObject();
            rgo.transform.parent = go.transform;
            rgo.transform.localPosition += node.RoomPoint- new Vector3(node.Point[0],0,node.Point[1]);
        }
        foreach(DungeonTreeT c in children){
            GameObject cgo = c.ToGameObject(terrain : terrain);
            cgo.transform.parent = go.transform;
            cgo.transform.localPosition += new Vector3(c.node.Point[0],0,c.node.Point[1]) - new Vector3(node.Point[0],0,node.Point[1]);
        }
        return go;
    }

    // public bool[,] ToBoolArray(bool[,] m = null){
    //     m ??= new bool[node.Size[0], node.Size[1]];
    //     foreach(DungeonCorridor corridor in node.Corridors) corridor.ApplyToBoolArray(m);
    //     if(node.Room != null) node.Room.ApplyToBoolArray(m);
    //     foreach(DungeonTreeT c in children) c.ToBoolArray(m);
    //     return m;
    // }

    public Mask RoomsFreeMask(Mask m = null, int type = -1){
        m ??= new Mask(node.Size[0], node.Size[1]);
        if(node.Room != null && (node.Room.Type == type || type == -1)) node.Room.RoomsFreeMask(m);
        foreach(DungeonTreeT c in children) c.RoomsFreeMask(m, type : type);
        return m;
    }
    
    public Mask RoomsMask(Mask m = null, int type = -1){
        m ??= new Mask(node.Size[0], node.Size[1]);
        if(node.Room != null && (node.Room.Type == type || type == -1)) node.Room.ApplyToMask(m);
        foreach(DungeonTreeT c in children) c.RoomsMask(m, type : type);
        return m;
    }

    public Mask CorridorsMask(Mask m = null){
        m ??= new Mask(node.Size[0], node.Size[1]);
        foreach(DungeonCorridor corridor in node.Corridors) corridor.ApplyToBoolArray(m.Array);
        foreach(DungeonTreeT c in children) c.CorridorsMask(m);
        return m;
    }

    public TerrainMasks ToTerrainMasks(){
        Mask[] typeMasks = new Mask[node.NumberOfTypes];
        for(int i = 0; i < node.NumberOfTypes; i++){
            typeMasks[i] = RoomsMask(type : i);
        }
        return new TerrainMasks(RoomsMask(), RoomsFreeMask(), CorridorsMask(), typeMasks);
    }

}   

