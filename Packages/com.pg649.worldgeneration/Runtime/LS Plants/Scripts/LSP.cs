using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MarchingCubesProject;

public class LSP : Placeable {
    private LSP_Settings settings;
    private int height, width;
    private LS_LSystem ls;

    public LSP(LSP_Settings settings){
        this.settings = settings;
        ls = settings.BuildLSystem();
        width = (int)Math.Ceiling(settings.thickness);
        height = width;
    }



    public override GameObject ToGameObject(){
        GameObject go = new GameObject("Plant");
        Segment[] segments = LSystemToSegments();
        //Metaball m = Metaball.BuildFromSegments(segments, FalloffFunctions.POLYNOMIAL2);
        //MeshGenerator meshGen = go.AddComponent<MeshGenerator>();
        //meshGen.Generate(m);
        return go;
    }
    public Segment[] LSystemToSegments(){
        Segment[] segments = new Segment[ls.segments.Count];
        for(int i = 0; i < ls.segments.Count; i++){
            segments[i] = new Segment(ls.segments[i].Item1, ls.segments[i].Item2, settings.thickness);
        }
        return segments;
    }

    public override int Width{
        get{return width;}
    }
    public override int Height{ 
        get{return height;}
    }
}
