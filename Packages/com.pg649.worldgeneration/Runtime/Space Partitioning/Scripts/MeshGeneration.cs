using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MeshGeneration{

public enum Plane{
    XY, XZ, YZ
}

//only for xz quads atm
public static GameObject Quad(float width, float height, Plane plane = Plane.XZ){
    GameObject go = new GameObject("Quad");
    Mesh mesh = new Mesh();
    Vector3[] vertices;
    Vector3[] normals;
    if(plane == Plane.XZ){
        vertices = new Vector3[] {
            new Vector3(0,0,0),
            new Vector3(width, 0, 0),
            new Vector3(width, 0, height),
            new Vector3(0, 0, height)
        };
        normals = new Vector3[] {Vector3.up,Vector3.up,Vector3.up,Vector3.up};
    }  
    else if(plane == Plane.YZ){
        vertices = new Vector3[] {
            new Vector3(0,0,0),
             new Vector3(0, 0, width),
            new Vector3(0, height, width),
            new Vector3(0, height, 0)
            
           
        };
        normals = new Vector3[] {Vector3.right,Vector3.right,Vector3.right,Vector3.right};
    }
    else{
        vertices = new Vector3[] {
            new Vector3(0,0,0),
            new Vector3(width, 0, 0),
            new Vector3(width, height, 0),
            new Vector3(0, height, 0)
        };
        normals = new Vector3[] {Vector3.forward,Vector3.forward,Vector3.forward,Vector3.forward};
    }
    
    mesh.vertices = vertices;
    int[] triangles = new int[6]{
        0,3,1,
        3,2,1
    };
    mesh.triangles = triangles;
    mesh.normals = normals;
    Vector2[] uv = new Vector2[4]
    {
      new Vector2(0, 0),
      new Vector2(1, 0),
      new Vector2(1, 1),
      new Vector2(0, 1)
    };
    mesh.uv = uv;   
    MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
    meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
    MeshFilter meshFilter = go.AddComponent<MeshFilter>();
    meshFilter.mesh = mesh;
    return go;
    }

    public static GameObject Wall(Vector3 point, Vector3 dir, float length, float width, float heigth){
        GameObject go = new GameObject("Wall");

        return go;
    }

}
