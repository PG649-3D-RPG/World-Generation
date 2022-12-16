using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceableCube : Placeable {

    private int size;

    public PlaceableCube(int size = 1){
        this.size = size;
    }

    public override GameObject ToGameObject(){
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.localScale = new Vector3(size,size,size);
        return go;
    }

    public override int Width{
        get{return 1;}
    }
    public override int Height{
        get{return 1;}
    }

}
