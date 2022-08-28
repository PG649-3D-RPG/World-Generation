using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MeshGeneration{


//only for xz quads atm
public static GameObject Quad(float width, float height){
    GameObject go = new GameObject("Quad");
    Mesh mesh = new Mesh();
    Vector3[] vertices = new Vector3[4]{
        new Vector3(0,0,0),
        new Vector3(width, 0, 0),
        new Vector3(width, 0, height),
        new Vector3(0, 0, height)
    };
    mesh.vertices = vertices;
    int[] triangles = new int[6]{
        0,3,1,
        3,2,1
    };
    mesh.triangles = triangles;
    Vector3[] normals = new Vector3[4]
        {
            Vector3.up,
            Vector3.up,
            Vector3.up,
            Vector3.up
        };
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

}
