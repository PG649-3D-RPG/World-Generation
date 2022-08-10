using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MeshGeneration{


static Mesh GenerateMesh(int[] size){
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    if(size.Length > 0 && size.Length < 4){
        vertices.Add(new Vector3(0,0,0));
        vertices.Add(new Vector3(size[0],0,0));
        if(size.Length > 1){
            foreach(Vector3 v in vertices){
                vertices.Add(v + new Vector3(0,0,size[1]));
            }
            triangles.Add(0);
            triangles.Add(1);
            triangles.Add(2);
            triangles.Add(0);
            triangles.Add(2);
            triangles.Add(3);
        }
        if(size.Length > 2){
            foreach(Vector3 v in vertices){
                vertices.Add(v + new Vector3(0,size[2]));
            }
            //...
        }
        if(size.Length == 1){
            triangles.Add(0);
            triangles.Add(1);
            triangles.Add(0);
        }
    }
    Mesh mesh = new Mesh();
    mesh.vertices = vertices.ToArray();
    mesh.triangles = triangles.ToArray();
    return mesh;
}

}
