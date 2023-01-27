using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSP : Placeable {
    private LSP_Settings settings;
    private int depth, width;
    private LS_LSystem ls;
    private GameObject lspgo;

    public LSP(LSP_Settings settings) {
        this.settings = settings;
        this.lspgo = null;
        ls = settings.BuildLSystem();
        SetWidthHeight();
    }

    public void SetWidthHeight() {
        float width = 0;
        float depth = 0;
        List<Vector3> points = new List<Vector3>();
        foreach (Tuple<Vector3, Vector3> t1 in ls.segments) {
            points.Add(t1.Item1);
            points.Add(t1.Item2);
        }
        foreach (Vector3 v1 in points) {
            foreach (Vector3 v2 in points) {
                float wd = Mathf.Abs(v2.x - v1.x);
                float dd = Mathf.Abs(v2.z - v1.z);
                if (width < wd) width = wd;
                if (depth < dd) depth = dd;
            }
        }
        this.width = (int)Mathf.Ceil(width);
        this.depth = (int)Mathf.Ceil(depth);
    }

    public override GameObject ToGameObject() {
        return ToGameObjectPrimitive();
        //GameObject go = new GameObject("Plant");
        //Segment[] segments = LSystemToSegments();
        //Metaball m = Metaball.BuildFromSegments(segments, FalloffFunctions.POLYNOMIAL2);
        //MeshGenerator meshGen = go.AddComponent<MeshGenerator>();
        //meshGen.Generate(m);
        //return go;
    }
    // public Segment[] LSystemToSegments(){
    //     Segment[] segments = new Segment[ls.segments.Count];
    //     for(int i = 0; i < ls.segments.Count; i++){
    //         segments[i] = new Segment(ls.segments[i].Item1, ls.segments[i].Item2, settings.thickness);
    //     }
    //     return segments;
    // }

    public GameObject ToGameObjectPrimitive(bool combine = true) {
        if (lspgo == null) lspgo = combine ? ToGameObjectPrimitiveCombined() : ToGameObjectPrimitiveSingle();
        return lspgo;
    }

    private GameObject ToGameObjectPrimitiveSingle() {
        GameObject go = new GameObject("Plant");
        for (int i = 0; i < ls.segments.Count; i++) {
            Tuple<Vector3, Vector3> t = ls.segments[i];
            GameObject g = ls.fromRule[i].Item2 == 'A' ? GameObject.CreatePrimitive(PrimitiveType.Cylinder) : GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Vector3 mid = t.Item1 + ((t.Item2 - t.Item1) / 2);
            g.transform.position = mid;
            Vector3 start = t.Item1;
            Vector3 end = t.Item2;
            Vector3 sd = (end - start).normalized;
            g.transform.localScale = new Vector3(.1f, Vector3.Distance(start, end) / 2, .1f);
            g.transform.rotation = Quaternion.LookRotation(end - start) * Quaternion.FromToRotation(Vector3.up, Vector3.forward);// * Quaternion.FromToRotation(Vector3.left, -Vector3.left);
            g.transform.parent = go.transform;
        }
        return go;
    }

    private GameObject ToGameObjectPrimitiveCombined(bool optimize = true) {
        GameObject go = new GameObject("Plant");
        MeshFilter[] meshFilters = new MeshFilter[ls.segments.Count];
        CombineInstance[] combine = new CombineInstance[ls.segments.Count];


        for (int i = 0; i < ls.segments.Count; i++) {
            Tuple<Vector3, Vector3> t = ls.segments[i];
            // GameObject g = ls.fromRule[i].Item2 == 'A' ? GameObject.CreatePrimitive(PrimitiveType.Cylinder) : GameObject.CreatePrimitive(PrimitiveType.Capsule);
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

            Vector3 mid = t.Item1 + ((t.Item2 - t.Item1) / 2);
            g.transform.position = mid;
            Vector3 start = t.Item1;
            Vector3 end = t.Item2;
            // Vector3 sd = (end - start).normalized;
            g.transform.localScale = new Vector3(.1f, Vector3.Distance(start, end) / 2, .1f);
            g.transform.rotation = Quaternion.LookRotation(end - start) * Quaternion.FromToRotation(Vector3.up, Vector3.forward);// * Quaternion.FromToRotation(Vector3.left, -Vector3.left);
            meshFilters[i] = g.GetComponent<MeshFilter>();
        }

        for (int i = 0; i < combine.Length; i++) {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            // meshFilters[i].gameObject.SetActive(false);
            UnityEngine.GameObject.Destroy(meshFilters[i].gameObject);
        }
        MeshFilter mFilter = go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        go.GetComponent<Renderer>().material = Resources.Load<Material>("Materials/instancing");

        mFilter.mesh = new Mesh {
            // indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };
        mFilter.mesh.CombineMeshes(combine);
        if (optimize) {
            mFilter.mesh.RecalculateBounds();
            mFilter.mesh.RecalculateNormals();
            mFilter.mesh.Optimize();
        }

        // Bounds bds = mFilter.mesh.bounds;
        // NavMeshModifierVolume nmv = go.AddComponent<NavMeshModifierVolume>();
        // nmv.area = 1;
        // nmv.size = bds.size;
        // nmv.center = bds.center;

        // var nmm = go.AddComponent<NavMeshModifier>();
        // nmm.overrideArea = true;
        // nmm.area = 1;


        return go;
    }


    public override int Width {
        get { return width; }
    }
    public override int Height {
        get { return depth; }
    }
}
