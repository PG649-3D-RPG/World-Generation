using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class DungeonTreeMeta : MonoBehaviour{
    public int[] partitionSize;
    public int[] point;
    public Vector3 roomPoint;
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
    public Vector3 RoomPoint{
        get{return roomPoint;}
        set{roomPoint = value;}
    }
}

public class DungeonRoom : IGameObjectable{
    private int width, depth;
    private float height;
    private List<DungeonCorridor> corridors;
    private List<Tuple<Face,float>> corridorPoints;
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
        corridorPoints = new List<Tuple<Face,float>>();
    }
    public DungeonRoom(IEnumerable<int> x, Vector3 roomPoint, System.Random rand = null) : this(x.ToArray()[0],x.ToArray()[1],x.ToArray()[2], roomPoint, rand : rand){}

    //doesnt work if both points are on the same boundary
    public void SetFreePath(Face f1, Face f2, float p1, float p2){
        if((f1 == Face.Left || f1 == Face.Right) && (f1 == Face.Left || f2 == Face.Right)){
            
        }
        else if(true){}
    }

    public Vector3 FacePoint(Face face){
        switch(face){
            case Face.Right:
                return LowerRightPoint;
            case Face.Back:
                return UpperLeftPoint;
            default:
                return roomPoint;
        }
    }

    public void AddCorridorPoint(Tuple<Face, float> t){
        foreach(Tuple<Face,float> tp in corridorPoints){
            if(tp.Item1 != t.Item1){
                SetFreePath(tp.Item1, t.Item1, tp.Item2, t.Item2);
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
        envSettings.Depth = width/10; 
        envSettings.Scale = 5f;
        envSettings.TerrainSize = width;
        envSettings.GenerateBorders = false;
        envSettings.UseSmoothing = false;
        envSettings.GenerateObstacles = false;
        EnvironmentGenerator generator = new EnvironmentGenerator(ref terrain, envSettings);
        generator.Build();
        return go;
    }

    public Vector3 RoomPoint{
        get{return roomPoint;}
    }
    public Vector3 UpperLeftPoint{
        get{return new Vector3(roomPoint.x, roomPoint.y, roomPoint.z + depth);}
    }
    public Vector3 UpperRightPoint{
        get{return new Vector3(roomPoint.x + width, roomPoint.y, roomPoint.z + depth);}
    }
    public Vector3 LowerRightPoint{
        get{return new Vector3(roomPoint.x + width, roomPoint.y, roomPoint.z);}
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


    public void SetValues(int width, int depth, float height, Vector3 roomPoint){
        this.width = width;
        this.depth = depth;
        this.height = height;
        this.roomPoint = roomPoint;
    }
}


public class DungeonCorridor : IGameObjectable {
    private List<Vector3> path;
    private int width;
    private float height;
    public DungeonRoom startRoom, endRoom;
    public DungeonRoom.Face startFace, endFace;
    private int startIndex, endIndex;

    public DungeonCorridor(){
        path = new List<Vector3>();
    }

    public GameObject ToGameObject(){
        GameObject go = new GameObject("DungeonCorridor");
        DungeonCorridorMeta dcm = go.AddComponent<DungeonCorridorMeta>();
        dcm.SetValues(this);
        MeshGeneration.CorridorGround(path,width).transform.parent = go.transform;
        return go;
    }

    public float Length(){
        float l = 0;
        for(int i = 0; i < path.Count-1; i++){
            l += Vector3.Distance(path[i],path[i+1]);
        }
        return l;
    }

    public List<Vector3> Path{
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
}
class DungeonCorridorMeta : MonoBehaviour{
    public List<Vector3> path;
    public float length;
    public DungeonCorridorMeta(){
    }
    public void SetValues(List<Vector3> path){
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
    public void PlaceCorridor2(int minWidth, int maxWidth, float minHeight, float maxHeight, float maxDistance = float.MaxValue){
        int width = node.Rand.Next(minWidth, maxWidth+1);
        float height = (float)node.Rand.NextDouble()*(maxHeight-minHeight)+minHeight;
        if((node.Size[0] == children[0].Node.Size[0] && children[0].Node.RoomsNorth.Count > 0 && children[1].Node.RoomsSouth.Count > 0) || node.Size[1] == children[0].Node.Size[1] && children[0].Node.RoomsEast.Count > 0 && children[1].Node.RoomsWest.Count > 0){
            Vector3 start,mid,mid2,end;
            DungeonRoom startRoom, endRoom;
            DungeonRoom.Face startFace, endFace;
            DungeonCorridor corridor = new DungeonCorridor();
            if(node.Size[0] == children[0].Node.Size[0] && children[0].Node.RoomsNorth.Count > 0 && children[1].Node.RoomsSouth.Count > 0){
                int cri1 = node.Rand.Next(0,children[0].Node.RoomsNorth.Count);
                startRoom = children[0].Node.RoomsNorth[cri1];
                int cri2 = FindConnectingRoom(startRoom.UpperLeftPoint, children[1].Node.RoomsSouth, DungeonRoom.Face.Front, maxDistance);
                endRoom = children[1].Node.RoomsSouth[cri2];
                int startPointIndex = node.Rand.Next(0,startRoom.Width - width+1);
                float startPoint = startPointIndex + ((float)width/2);
                int endPointIndex = node.Rand.Next(0,endRoom.Width - width+1);
                float endPoint = endPointIndex + ((float)width/2);
                corridor.StartIndex = startPointIndex;
                corridor.EndIndex = endPointIndex;
                start = startRoom.RoomPoint - node.PointV + new Vector3(startPoint,0,startRoom.Depth);
                mid = new Vector3(start.x,start.y, children[0].Node.Size[1]);
                end = endRoom.RoomPoint - node.PointV + new Vector3(endPoint,0,0);
                mid2 = new Vector3(end.x,end.y, children[0].Node.Size[1]);
                startFace = DungeonRoom.Face.Back;
                endFace = DungeonRoom.Face.Front;
            }   
            else{
                int cri1 = node.Rand.Next(0,children[0].Node.RoomsEast.Count);
                startRoom = children[0].Node.RoomsEast[cri1];
                //int cri2 = node.Rand.Next(0,children[1].Node.RoomsWest.Count);
                int cri2 = FindConnectingRoom(startRoom.LowerRightPoint, children[1].Node.RoomsWest, DungeonRoom.Face.Left, maxDistance);
                endRoom = children[1].Node.RoomsWest[cri2];
                int startPointIndex = node.Rand.Next(0,startRoom.Depth - width+1);
                float startPoint = startPointIndex + ((float)width/2);
                //float startPoint = (float)node.Rand.NextDouble() * ((startRoom.Depth - width) - width) + width;
                //float endPoint = (float)node.Rand.NextDouble() * ((endRoom.Depth - width) - width) + width; 
                int endPointIndex = node.Rand.Next(0,endRoom.Depth - width+1);
                float endPoint = endPointIndex + ((float)width/2);
                corridor.StartIndex = startPointIndex;
                corridor.EndIndex = endPointIndex;
                start = startRoom.RoomPoint - node.PointV + new Vector3(startRoom.Width,0,startPoint);
                mid = new Vector3(children[0].Node.Size[0],start.y, start.z);
                end = endRoom.RoomPoint - node.PointV + new Vector3(0,0,endPoint);
                mid2 = new Vector3(children[0].Node.Size[0],end.y, end.z);
                startFace = DungeonRoom.Face.Right;
                endFace = DungeonRoom.Face.Left;
            }
            corridor.StartRoom = startRoom;
            corridor.StartRoom = endRoom;
            corridor.StartFace = startFace;
            corridor.EndFace = endFace;
            corridor.Width = width;
            corridor.Height = height;
            corridor.Path = new List<Vector3>(){start,mid,mid2,end};
            node.Corridors.Add(corridor);
        }
    }

    public void PlaceCorridors4(int minWidth, int maxWidth, float minHeight, float maxHeight){
        if(children[0].Node.RoomsEast.Count > 0 && children[1].Node.RoomsWest.Count > 0){
            int width = node.Rand.Next(minWidth, maxWidth+1);
            float height = (float)node.Rand.NextDouble()*(maxHeight-minHeight)+minHeight;
            DungeonCorridor corridor = new DungeonCorridor();
            corridor.StartRoom = children[0].Node.RoomsEast[node.Rand.Next(0,children[0].Node.RoomsEast.Count)];
            corridor.EndRoom = children[1].Node.RoomsWest[node.Rand.Next(0,children[1].Node.RoomsWest.Count)];
            corridor.StartIndex = node.Rand.Next(0,corridor.StartRoom.Depth-width+1);
            corridor.EndIndex = node.Rand.Next(0,corridor.EndRoom.Depth-width+1);
            Vector3 start = corridor.StartRoom.RoomPoint - node.PointV + new Vector3(corridor.StartRoom.Width,0,corridor.StartIndex+((float)width/2));
            corridor.Path.Add(start);
            Vector3 end = corridor.EndRoom.RoomPoint - node.PointV + new Vector3(0,0,corridor.EndIndex+((float)width/2));
            if(start.z != end.z){
                Vector3 mid = new Vector3(children[0].Node.Size[0],start.y, start.z);
                Vector3 mid2 = new Vector3(children[0].Node.Size[0], end.y, end.z);
                corridor.Path.Add(mid);
                corridor.Path.Add(mid2);
            }
            corridor.Path.Add(end);
            corridor.Width = width;
            corridor.Height = height;
            corridor.StartFace = DungeonRoom.Face.Right;
            corridor.EndFace = DungeonRoom.Face.Left;
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
            Vector3 start = corridor.StartRoom.RoomPoint - node.PointV + new Vector3(corridor.StartIndex+((float)width/2),0,corridor.StartRoom.Depth);
            corridor.Path.Add(start);
            Vector3 end = corridor.EndRoom.RoomPoint - node.PointV + new Vector3(corridor.EndIndex+((float)width/2),0,0);
            if(start.z != end.z){
                Vector3 mid = new Vector3(start.x,start.y, children[1].Node.Size[1]);
                Vector3 mid2 = new Vector3(end.x, end.y, children[1].Node.Size[1]);
                corridor.Path.Add(mid);
                corridor.Path.Add(mid2);
            }
            corridor.Path.Add(end);
            corridor.Width = width;
            corridor.Height = height;
            corridor.StartFace = DungeonRoom.Face.Top;
            corridor.EndFace = DungeonRoom.Face.Bottom;
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
            Vector3 start = corridor.StartRoom.RoomPoint - node.PointV + new Vector3(corridor.StartRoom.Width,0,corridor.StartIndex+((float)width/2));
            corridor.Path.Add(start);
            Vector3 end = corridor.EndRoom.RoomPoint - node.PointV + new Vector3(0,0,corridor.EndIndex+((float)width/2));
            if(start.z != end.z){
                Vector3 mid = new Vector3(children[0].Node.Size[0],start.y, start.z);
                Vector3 mid2 = new Vector3(children[0].Node.Size[0], end.y, end.z);
                corridor.Path.Add(mid);
                corridor.Path.Add(mid2);
            }
            corridor.Path.Add(end);
            corridor.Width = width;
            corridor.Height = height;
            corridor.StartFace = DungeonRoom.Face.Right;
            corridor.EndFace = DungeonRoom.Face.Left;
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
            Vector3 start = corridor.StartRoom.RoomPoint - node.PointV + new Vector3(corridor.StartIndex+((float)width/2),0,corridor.StartRoom.Depth);
            corridor.Path.Add(start);
            Vector3 end = corridor.EndRoom.RoomPoint - node.PointV + new Vector3(corridor.EndIndex+((float)width/2),0,0);
            if(start.z != end.z){
                Vector3 mid = new Vector3(start.x,start.y, children[1].Node.Size[1]);
                Vector3 mid2 = new Vector3(end.x, end.y, children[1].Node.Size[1]);
                corridor.Path.Add(mid);
                corridor.Path.Add(mid2);
            }
            corridor.Path.Add(end);
            corridor.Width = width;
            corridor.Height = height;
            corridor.StartFace = DungeonRoom.Face.Top;
            corridor.EndFace = DungeonRoom.Face.Bottom;
            node.Corridors.Add(corridor);
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
}