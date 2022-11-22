using UnityEngine;

public static class TerrainTransforms {
    public static float[,] Twirl(float[,] heights, float angle, float[,] mask) {
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
                //if (distance > radius2) {
                //    xn = x;
                //    yn = y;
                //}
                //else {
                float weight = mask[x, y];
                distance = Mathf.Sqrt(distance);
                float a = Mathf.Atan2(dy, dx) + angle * (distance) / radius;
                xn = Mathf.FloorToInt(cx + distance * Mathf.Cos(a));
                xn = Mathf.FloorToInt(Mathf.Lerp(xn, x, weight));
                yn = Mathf.FloorToInt(cy + distance * Mathf.Sin(a));
                yn = Mathf.FloorToInt(Mathf.Lerp(yn, y, weight));
                //}
                if (xn < width && yn < height && xn > 0 && yn > 0) {
                    result[xn, yn] = heights[x, y];
                    //result[x, y] = Mathf.Lerp(result[xn, yn], heights[x, y], weight);
                }
            }
        }

        return result;
    }

    public static float[,] DistortImage(float[][] _input, float[,] _mask, float _degrees) {

        float[][] output = new float[_input.Length][];
        for (int i = 0; i < _input.Length; i++) {
            output[i] = new float[_input.Length];

        }

        for (int i = 0; i < _input.Length; i++)
            for (int j = 0; j < _input[i].Length; j++) {
                //Quaterion zum Rotieren
                Quaternion rotQuat = Quaternion.AngleAxis(_degrees, Vector3.forward);
                //Normalisierte pixelKoordinaten
                Vector3 coords = new Vector3(j, i);
                //Rotation
                coords = rotQuat * coords;
                //coords *= _input.Length;
                //Debug.Log(Mathf.Abs(Mathf.FloorToInt(coords.x)) % _input.Length);

                //Debug.Log(Mathf.Abs(Mathf.FloorToInt(coords.y)) % _input.Length);
                output[Mathf.Abs(Mathf.FloorToInt(coords.x)) % _input.Length][Mathf.Abs(Mathf.FloorToInt(coords.y)) % _input.Length] = _input[i][j];
            }
        float[,] result = new float[_input.Length, _input.Length];
        for (int i = 0; i < _input.Length; i++) {
            for (int j = 0; j < _input[i].Length; j++) {
                result[i, j] = output[i][j];
            }
        }
        return result;
    }

    public static float DistortedNoise(float x, float y, float distortionStrength) {
        // Take two samples from our distortion function
        // (Shifted in space by > 1 so they don't correlate with each other)
        float xDistortion = distortionStrength * Distort(x + 2.3f, y + 2.9f);
        float yDistortion = distortionStrength * Distort(x - 3.1f, y - 4.3f);

        return Mathf.PerlinNoise(x + xDistortion, y + yDistortion);
    }

    static float Distort(float x, float y) {
        // Optionally, you can scale your internal noise frequency
        // or layer several octaves of noise to control the wiggly shapes.
        float wiggleDensity = 100f;
        return Mathf.PerlinNoise(x * wiggleDensity, y * wiggleDensity);
    }

}
