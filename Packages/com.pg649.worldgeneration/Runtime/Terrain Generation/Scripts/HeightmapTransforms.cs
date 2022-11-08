using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightmapTransforms
{
    public static float[,] AverageFilter(int width, int height){
        float[,] f = new float[width, height];
        float v = 1/(float)(width*height);
        for(int i = 0; i < width; i++) for(int j = 0; j < height; j++) f[i,j] = v;
        return f;
    }

    public static void ApplyFilter(float[,] a, float[,] filter){
        int fw = filter.GetLength(0);
        int fh = filter.GetLength(1);
        float[,] b = new float[a.GetLength(0), a.GetLength(1)];
        for(int i = fw/2; i < a.GetLength(0)-(fw/2); i++){
            for(int j = fh/2; j < a.GetLength(1)-(fh/2);j++){
                float s = 0;
                for(int k = i-(fw/2); k <= i + (fw/2); k ++){
                    for(int l = j-(fh/2); l <= j + (fh/2); l ++){
                        s += a[k,l] * filter[k-(i-(fw/2)),l-(j-(fh/2))];
                    }
                }
                b[i,j] = s;
            }
        }
        //fix
        for(int i = 0; i < a.GetLength(0); i++) for(int j = 0; j < a.GetLength(1); j++) a[i,j] = b[i,j];
    }

    public static void ApplyPerlinNoiseByHeigth(float [,] a, bool[,] mask = null){
        int w = a.GetLength(0);
        int h = a.GetLength(1);
        for(int i = 0; i < a.GetLength(0); i++){
            for(int j = 0; j < a.GetLength(1); j++){
                if(mask == null || mask[i,j]){
                    a[i,j] += 0.05f;
                    a[i,j] *= Mathf.PerlinNoise(i*a[i,j], j*a[i,j]);
                }
            }
        }
    }
}

