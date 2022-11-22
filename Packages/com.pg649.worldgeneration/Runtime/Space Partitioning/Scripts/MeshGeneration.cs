using UnityEngine;
using System.Collections.Generic;
using System;
public static class MeshGeneration{

public enum Plane{
    XY, XZ, YZ
}

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
    go.AddComponent<MeshCollider>().sharedMesh = mesh;
    return go;
}


    public static GameObject Wall(Vector3 point1, Vector3 point2, float width, float heigth){
        GameObject go = new GameObject("Wall");

        return go;
    }


    public static bool AddListSet(this List<Vector3> l, Vector3 v){
        bool add = true;
        foreach(Vector3 a in l) add = a == v ? false : true;
        if(add) l.Add(v);
        return add;
    }
         

    //only for axis parallel corridors with y = 0
    public static GameObject CorridorGround(List<Vector3> path, float width){
        GameObject go = new GameObject("Corridor Ground");
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        int vcount = -1;
        float hw = width/2;
        for(int i = 0; i < path.Count-1; i++){
            Vector3 dir = (path[i+1] - path[i]).normalized;
            int m = i == path.Count-2 ? 0 : 1;
            if(dir == Vector3.left || dir == Vector3.right){
                vertices.Add(path[i] + hw*Vector3.forward - (Math.Min(i,1)*(hw*dir)));
                vertices.Add(path[i] + hw*Vector3.back - (Math.Min(i,1)*(hw*dir)));
                vertices.Add(path[i+1] + hw*Vector3.forward - (m*hw*dir));
                vertices.Add(path[i+1] + hw*Vector3.back - (m*hw*dir));
                vcount += 4;
                if(dir == Vector3.left) triangles.AddRange(new int[]{vcount-3, vcount-2, vcount-1, vcount, vcount -1, vcount-2});
                else triangles.AddRange(new int[]{vcount-2, vcount-3, vcount-1, vcount, vcount -2, vcount-1});
            }
            else if(dir == Vector3.forward || dir == Vector3.back){
                vertices.Add(path[i] + hw*Vector3.left - (Math.Min(i,1)*(hw*dir)));
                vertices.Add(path[i] + hw*Vector3.right - (Math.Min(i,1)*(hw*dir)));
                vertices.Add(path[i+1] + hw*Vector3.left - (m*hw*dir));
                vertices.Add(path[i+1] + hw*Vector3.right - (m*hw*dir));
                vcount += 4;
                if(dir == Vector3.forward) triangles.AddRange(new int[]{vcount-1, vcount-2, vcount-3, vcount-2, vcount -1, vcount});
                else triangles.AddRange(new int[]{vcount-2, vcount-1, vcount-3, vcount-1, vcount -2, vcount});
            }
            
        }
        Vector3[] normals = new Vector3[vertices.Count];
        for(int i = 0; i < normals.Length; i++) normals[i] = Vector3.down;
        Vector3[] vertices_a = vertices.ToArray();
        //Vector2[] uvs = new Vector2[vertices_a.Length];
        //for (int i = 0; i < uvs.Length; i++) uvs[i] = new Vector2(vertices_a[i].x, vertices_a[i].z);
        //mesh.uv = uvs;
        mesh.vertices = vertices_a;
        mesh.triangles = triangles.ToArray();
        MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
        MeshFilter meshFilter = go.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        go.AddComponent<MeshCollider>().sharedMesh = mesh;
        return go;
    }
}