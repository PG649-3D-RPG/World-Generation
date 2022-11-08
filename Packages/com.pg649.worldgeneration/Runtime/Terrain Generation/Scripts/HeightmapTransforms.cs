using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public static class HeightmapTransforms
{
    public static float[,] extensionFilter = new float[,] {{1,1,1},{1,1,1},{1,1,1}};

    public static float[,] AverageFilter(int width, int height){
        float[,] f = new float[width, height];
        float v = 1/(float)(width*height);
        for(int i = 0; i < width; i++) for(int j = 0; j < height; j++) f[i,j] = v;
        return f;
    }

    public static void ApplyFilter(float[,] a, float[,] filter, bool[,] mask = null, bool invertMask = false, bool add = true){
        int fw = filter.GetLength(0);
        int fh = filter.GetLength(1);
        float[,] b = new float[a.GetLength(0), a.GetLength(1)];
        for(int i = fw/2; i < a.GetLength(0)-(fw/2); i++){
            for(int j = fh/2; j < a.GetLength(1)-(fh/2);j++){
                if(mask == null || (mask[i,j] && !invertMask) || (invertMask && !mask[i,j])){
                    float s = 0;
                    for(int k = i-(fw/2); k <= i + (fw/2); k ++){
                        for(int l = j-(fh/2); l <= j + (fh/2); l ++){
                            s += a[k,l] * filter[k-(i-(fw/2)),l-(j-(fh/2))];
                        }
                    }
                    b[i,j] = s;
                }
                else if(add) b[i,j] = a[i,j];
            }
        }
        //fix
        for(int i = 0; i < a.GetLength(0); i++) for(int j = 0; j < a.GetLength(1); j++) a[i,j] = b[i,j];
    }

    public static void ApplyPerlinNoise(float[,] a, float maxAddedHeight = 1, float scale = .1f, bool[,] mask = null, bool invertMask = false){
        for(int i = 0; i < a.GetLength(0); i++){
            for(int j = 0; j < a.GetLength(1); j++){
                if(mask == null || (mask[i,j] && !invertMask) || (invertMask && !mask[i,j]) ){
                    a[i,j] += Mathf.PerlinNoise(i*scale, j*scale) * maxAddedHeight;
                }
            }
        }
    }

    public static void ApplyPerlinNoiseByHeigth(float [,] a, bool[,] mask = null){
        int w = a.GetLength(0);
        int h = a.GetLength(1);
        for(int i = 0; i < a.GetLength(0); i++){
            for(int j = 0; j < a.GetLength(1); j++){
                if(mask == null || mask[i,j]){
                    a[i,j] += 0.05f;
                    a[i,j] *= Mathf.PerlinNoise(i*a[i,j]*10, j*a[i,j]*10);
                }
            }
        }
    }

    public static bool[,] MaskInvertedBorder(bool[,] mask, int width){
        float[,] b = mask.Map(x => x ? 1f : 0f);
        for(int i = 0; i < width; i++) ApplyFilter(b, extensionFilter, mask : mask, invertMask : true, add : false);
        return b.Map(x => x >= 1 ? true : false);
    }  
}


public static class Ext{
    public static V[,] Map<T,V>(this T[,] a, Func<T,V> f  ){
        V[,] b = new V[a.GetLength(0),a.GetLength(1)];
        for(int i = 0; i < a.GetLength(0); i++){
            for(int j = 0; j < a.GetLength(1); j++){
                b[i,j] = f(a[i,j]);
            }
        }
        return b;
    }
    public static void Map<T>(this T[,] a, Func<T,T> f){
        for(int i = 0; i < a.GetLength(0); i++){
            for(int j = 0; j < a.GetLength(1); j++){
                a[i,j] = f(a[i,j]);
            }
        }
    }
    public static V[,] ZipMap<T,K,V>(this T[,] a, K[,] b, Func<T,K,V> f){
        V[,] c = new V[a.GetLength(0),a.GetLength(1)];
        for(int i = 0; i < a.GetLength(0); i++){
            for(int j = 0; j < a.GetLength(1); j++){
                c[i,j] = f(a[i,j], b[i,j]);
            }
        }
        return c;
    }
}
