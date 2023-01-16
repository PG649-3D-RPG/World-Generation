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


    public void PlaceCorridors(int minWidth, int maxWidth, float minHeight, float maxHeight, float maxDistance = float.MaxValue, bool freeCorridors = true){
        switch(children.Count){
            case 2:
                PlaceCorridor2(minWidth,maxWidth, minHeight, maxHeight, maxDistance : maxDistance, freeCorridors : freeCorridors);   
                break;
            case 4:
                PlaceCorridors4(minWidth, maxWidth, minHeight, maxHeight, freeCorridors : freeCorridors);
                break;
            case 8:
                break;
            default:
                break;
        }
        foreach(DungeonTreeT child in children) child.PlaceCorridors(minWidth, maxWidth, minHeight, maxHeight, maxDistance : maxDistance, freeCorridors : freeCorridors);
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
    private void PlaceCorridor2(int minWidth, int maxWidth, float minHeight, float maxHeight, float maxDistance = float.MaxValue, bool freeCorridors = true){
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
                startRoom.AddCorridorPoint(new Tuple<DungeonRoom.Face, int, int>(startFace, startPointIndex, width), freeCorridors : freeCorridors);
                endRoom.AddCorridorPoint(new Tuple<DungeonRoom.Face, int, int>(endFace, endPointIndex,width), freeCorridors : freeCorridors);
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
                startRoom.AddCorridorPoint(new Tuple<DungeonRoom.Face, int, int>(startFace, startPointIndex,width), freeCorridors : freeCorridors);
                endRoom.AddCorridorPoint(new Tuple<DungeonRoom.Face, int, int>(endFace, endPointIndex,width), freeCorridors : freeCorridors);
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

    private void PlaceCorridors4(int minWidth, int maxWidth, float minHeight, float maxHeight, bool freeCorridors = true){
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

    public void CreateSpawnPoints(int spawnPointsPerRoom, int size, float agentRadius = 1f){
        if(node.Room != null){
            for(int i = 0; i < spawnPointsPerRoom; i++) node.Room.CreateSpawnPoint(size, agentRadius : agentRadius);
        }
        foreach(DungeonTreeT c in children) c.CreateSpawnPoints(spawnPointsPerRoom, size, agentRadius : agentRadius);
    }
    public List<Tuple<Vector3Int, int>> SpawnPoints(){
        List<Tuple<Vector3Int, int>> l = new List<Tuple<Vector3Int, int>>();
        if(node.Room != null) l.AddRange(node.Room.SpawnPoints);
        foreach(DungeonTreeT c in children){
            l.AddRange(c.SpawnPoints());
        }
        return l;
    }


    public GameObject ToGameObject(){
        GameObject go = new GameObject("DungeonTreeT");
        DungeonTreeMeta dtm = go.AddComponent<DungeonTreeMeta>();
        //dtm.SetValues(node.Size, node.Point);
        dtm.SetValues(node);
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

    public void AddPlaceableToRooms(Placeable p, int n = 1, int freeSpace = 2){
        if(node.Room != null){
            for(int i = 0; i < n; i++) node.Room.PlacePlaceable(p, freeSpace : freeSpace);
        }
         foreach(DungeonTreeT c in children){
            c.AddPlaceableToRooms(p, n : n, freeSpace : freeSpace);
        }
    }

    public void AddPlaceablesToGameObject(GameObject go){
        if(node.Room != null){
            foreach(Tuple<Placeable, Vector3Int> t in node.Room.Placeables){
                GameObject pgo = t.Item1.ToGameObject();
                pgo.transform.position = t.Item2;
                pgo.transform.parent = go.transform; 
            }
        }
        foreach(DungeonTreeT c in children){
            c.AddPlaceablesToGameObject(go);
        }
    }

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