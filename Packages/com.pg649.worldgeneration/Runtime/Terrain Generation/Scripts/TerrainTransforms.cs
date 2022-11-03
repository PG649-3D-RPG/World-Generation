using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TerrainTransforms {
    public static float[,] Twirl(float[,] heights, float angle) {
        int width = heights.GetLength(0);
        int height = heights.GetLength(0);
        float[,] result = new float[width, height];

        int cx = width / 2 - 1;
        int cy = height / 2 - 1;

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                int dx = x - cx;
                int dy = y - cy;
                int icX = width * cx;
                int icY = height * cy;
                int radius = Mathf.Min(cx, cy);
                int radius2 = radius * radius;
                float distance = dx * dx + dy * dy;
                int xn, yn;
                if (distance > radius2) {
                    xn = x;
                    yn = y;
                }
                else {
                    distance = Mathf.Sqrt(distance);
                    float a = Mathf.Atan2(dy, dx) + angle * (radius - distance) / radius;
                    xn = Mathf.FloorToInt(cx + distance * Mathf.Cos(a));
                    yn = Mathf.FloorToInt(cy + distance * Mathf.Sin(a));
                }
                result[xn, yn] = heights[x, y];
            }
        }

        return result;
    }
}
