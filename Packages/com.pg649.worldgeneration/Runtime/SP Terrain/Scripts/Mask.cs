using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Mask
{
    private bool[,] m;
    private int width;
    private int height;

    public Mask(bool[,] m){
        this.m = m;
        this.width = m.GetLength(0);
        this.height = m.GetLength(1);
    }
    public Mask(float[,] floatMask) : this(floatMask.Map(x => x > 0)){}
    public Mask(int width, int heigth) : this(new bool[width, heigth]){}

    public void Invert(){
        m.MapI(x => !x);
    }
    public void Add(Mask m1){
       m.ZipMapI(m1.Array, (x,y) => x || y);
    }
    public void Subtract(Mask m1){
        m.ZipMapI(m1.Array, (x,y) => x && !y);
    }
    public static Mask operator !(Mask m1){
        return m1.Map(x => !x);
    }
    public static Mask operator +(Mask m1, Mask m2){
        return m1.ZipMap(m2, (x,y) => x || y);
    }
    public static Mask operator -(Mask m1, Mask m2){
        return m1.ZipMap(m2, (x,y) => x && !y);
    }
    public static Mask operator *(Mask m1, Mask m2){
        return m1.ZipMap(m2, (x,y) => x && y);
    }
    public Mask Map(Func<bool,bool> f){
        return new Mask(m.Map(f));
    }
    public Mask ZipMap(Mask m1, Func<bool,bool,bool> f){
        return new Mask(m.ZipMap(m1.Array, f));
    }
    public Mask InvertedBorderMask(int width){
        float[,] b = ToFloatArray();
        for(int i = 0; i < width; i++) HeightmapTransforms.ApplyFilter(b, HeightmapTransforms.extensionFilter, mask : this, invertMask : true, add : false);
        return new Mask(b);
    }
    public Mask SubtractedBorderMask(int width){
        float[,] b = ToFloatArray();
        for(int i = 0; i < width; i++) HeightmapTransforms.ApplyFilter(b, HeightmapTransforms.extensionFilter, mask : this, invertMask : false, multiplyFilter : true);
        return new Mask(b);
    }
    public float[,] ToFloatArray(){
        return m.Map(x => x ? 1f : 0f);
    }
    public Texture2D ToTexture(){
        Texture2D t = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for(int i = 0; i < m.GetLength(0); i ++){
            for(int j = 0; j < m.GetLength(1); j++){
                if(m[i,j]) t.SetPixel(i,j, Color.black);
            }
        }
        t.Apply();
        return t;
    }

    public bool[,] Array{
        get{return m;}
        set{m = value;}
    }
    public bool this[int i, int j]{
        get{return m[i,j];}  
        set{m[i,j] = value;}
    }
}