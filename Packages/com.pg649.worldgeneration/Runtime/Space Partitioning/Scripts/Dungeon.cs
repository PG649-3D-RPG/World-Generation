using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class DungeonTree : SPTree {
    private Vector3 roomPoint;
    private DungeonRoom room;
    private DungeonCorridor corridor;
    private Tuple<int,int>[] minMaxMargin;
    private Tuple<int,int> minMaxHeight;
    private Func<SPTree,float> fHeight;





    public DungeonTree(IEnumerable<int> size, System.Random rand = null) : base(size,rand){
        minMaxMargin = new Tuple<int,int>[d];
        for(int i = 0; i < d; i++){
            minMaxMargin[i] = new Tuple<int,int>(0,0);
        }
        fHeight = fHeightAreaBased();
    }
    public DungeonTree(SPTree tree) : this(tree.Label, tree.Rand){
        point = tree.Point;
        rand = tree.Rand;
        d = tree.D;
        foreach(SPTree c in tree.Children){
            AddChild(new DungeonTree(c));
        }
    }





    private static Func<SPTree,float> fHeight2DMinMax(int min, int max){
    return x => {
        return x.Rand.Next(min, max+1);
    };
    }
    private static Func<SPTree,float> fHeightAreaBased(int min = 2, int max = int.MaxValue, float scale = 1f){
        return x => MathF.Min(MathF.Max(min, x.label[0]*x.label[1]*scale), max);
    }





    public void PlaceRoomsBinary(){
        if(IsLeaf()){
            int leftMargin = rand.Next(minMaxMargin[0].Item1,minMaxMargin[0].Item2);
            int rightMargin = rand.Next(minMaxMargin[0].Item1,minMaxMargin[0].Item2);
            int depthLowerMargin = rand.Next(minMaxMargin[1].Item1,minMaxMargin[1].Item2);
            int depthUpperMargin = rand.Next(minMaxMargin[1].Item1,minMaxMargin[1].Item2);
           if(d == 2){
                room = new DungeonRoom(label[0]-leftMargin-rightMargin, label[1]-depthLowerMargin-depthUpperMargin, fHeight(this));
                roomPoint = new Vector3(point[0]+leftMargin, 0, point[1] + depthUpperMargin);
           }
           else if(d == 3){
                int heightLowerMargin = d > 2 ? rand.Next(minMaxMargin[2].Item1,minMaxMargin[2].Item2) : 0;
                int heightUpperMargin = d > 2 ? rand.Next(minMaxMargin[2].Item1,minMaxMargin[2].Item2) : 0;
                room = new DungeonRoom(label[0]-leftMargin-rightMargin, label[1]-depthLowerMargin-depthUpperMargin, label[2]-heightLowerMargin-heightUpperMargin);
                roomPoint = new Vector3(point[0]+leftMargin, point[2] + heightLowerMargin, point[1] + depthUpperMargin);
           }
        }
        else{
            foreach(DungeonTree c in children) c.PlaceRoomsBinary();
        }
    }
    public void PlaceRoomsQuad(){
        
    }
    public void PlaceRoomsOct(){
        
    }
    public void PlaceRooms(){
        switch(children.Count){
            case 2:
                PlaceRoomsBinary(); 
                break;
            case 4:
                PlaceRoomsQuad();
                break;
            case 8:
                PlaceRoomsOct();
                break;
        }
    }





    public GameObject ToGameObject(){
        GameObject go = new GameObject("DungeonTree");
        if(corridor != null){
            corridor.ToGameObject().transform.parent = go.transform;
        }
        if(room != null) room.ToGameObject().transform.parent = go.transform;
        foreach(DungeonTree c in children){
            c.ToGameObject().transform.parent = go.transform;
        }
        return go;
    }





    public Tuple<int,int>[] MinMaxMargin{
        get{ return minMaxMargin;}
        set{
            if(value.Length != d) throw new DimensionMismatchException(String.Format("Dimension of MinMaxMargin Array (value: {0}) is not equal to the dimension of the DungeonTree (value = {1}).", value, d));
            else minMaxMargin = value;
            }
    }
    public DungeonRoom Room{
        get{return room;}
    }
}





public class DungeonRoom{
    private int width, depth;
    private float height;



    public DungeonRoom(int width, int depth, float height){
        this.width = width;
        this.depth = depth;
        this.height = height;
    }
    public DungeonRoom(IEnumerable<int> x){
        int[] xa = x.ToArray();
        if(xa.Length == 3){
            width = xa[0];
            depth = xa[1];
            height = xa[1];
        }
    }



    public Mesh ToMesh(){
        Mesh mesh = new Mesh();

        return mesh;
    }
    public GameObject ToGameObject(){
        GameObject go = new GameObject("DungeonRoom");

        return go;
    }

}





public class DungeonCorridor {
    private List<Vector3> path;
    private List<Vector3> boundaryPoints;
    public DungeonCorridor(List<Vector3> path, float width, float height){}





    public GameObject ToGameObject(){
        GameObject go = new GameObject("DungeonCorridor");

        return go;
    }
}

public class DimensionMismatchException : Exception{

    public DimensionMismatchException(int a, int b) :
        base(String.Format("Dimensions are not equal for values {0} and {1}",a,b)){}

    public DimensionMismatchException(string s) : base(s){}

}