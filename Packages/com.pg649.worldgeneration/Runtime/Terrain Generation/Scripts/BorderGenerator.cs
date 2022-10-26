using UnityEngine;

public class BorderGenerator
{
    // general terrain fields
    private readonly int TerrainSizeX;
    private readonly int TerrainSizeY;
    private readonly float Scale;
    private readonly int OffsetX;
    private readonly int OffsetY;

    // border fields
    private readonly int MinBorderSize;
    public int MaxBorderSize { get; }
    private readonly int BorderPadding = 10;

    // Smoothing fields
    private readonly bool UseSmoothing;
    private readonly int SmoothPasses;
    private readonly int SmoothRadius;
    private readonly bool StrongerSmoothing;


    private readonly bool[,] BorderZone;
    // storage for the border limits on all sides
    public readonly int[] BorderLeft;
    public readonly int[] BorderRight;
    public readonly int[] BorderTop;
    public readonly int[] BorderBottom;

    Vector2Int CornerTopLeft;
    Vector2Int CornerTopRight;
    Vector2Int CornerBottomLeft;
    Vector2Int CornerBottomRight;

    // coordinates where borders are located, also [y,x] indexed
    private bool[,] BorderSafeZone = null;

    public BorderGenerator(int terrainSizeY, int terrainSizeX, float scale, int offsetX, int offsetY, int minBorderSize, int maxBorderSize, bool useSmoothing, int smoothPasses, int smoothRadius, bool strongerSmoothing)
    {
        TerrainSizeX = terrainSizeX;
        TerrainSizeY = terrainSizeY;
        Scale = scale;
        OffsetX = offsetX;
        OffsetY = offsetY;
        MinBorderSize = minBorderSize;
        MaxBorderSize = maxBorderSize;
        UseSmoothing = useSmoothing;
        SmoothPasses = smoothPasses;
        SmoothRadius = smoothRadius;
        StrongerSmoothing = strongerSmoothing;

        BorderZone = new bool[terrainSizeY, terrainSizeX];
        BorderLeft = new int[terrainSizeY];
        BorderRight = new int[terrainSizeY];
        BorderTop = new int[terrainSizeX];
        BorderBottom = new int[terrainSizeX];

        // corners with placeholder values
        //TODO improve the placeholder assignment
        CornerTopLeft = new(1, 1);
        CornerTopRight = new(1, 1);
        CornerBottomLeft = new(1, 1);
        CornerBottomRight = new(1, 1);
    }

    public bool[,] GetBorderZone()
    {
        return BorderZone;
    }

    public void SetBorderSafeZone(bool[,] safeZoneArr)
    {
        // if (safeZoneArr.GetLength(0) != TerrainSizeY || safeZoneArr.GetLength(1) != TerrainSizeX)
        //     throw new System.ArgumentOutOfRangeException("BorderSafeZone must be of dimension [TerrainSize,TerrainSize]");
        BorderSafeZone = safeZoneArr;
    }

    // generate hills around edges
    public void GenerateBorders(ref float[,] heights)
    {
        //var heights = terrainData.GetHeights(0, 0, TerrainSize, TerrainSize);

        BorderSafeZone ??= new bool[TerrainSizeY, TerrainSizeX];

        // left border
        int randBorderSize = Random.Range(MinBorderSize, MaxBorderSize + 1);
        for (var y = 0; y < TerrainSizeY; y++)
        {
            randBorderSize += Random.Range(-1, 2);
            if (randBorderSize < MinBorderSize) randBorderSize = MinBorderSize;
            if (randBorderSize > MaxBorderSize) randBorderSize = MaxBorderSize;
            BorderLeft[y] = randBorderSize;
            for (var x = 0; x < randBorderSize; x++)
            {
                if (!BorderSafeZone[x, y])
                {
                    heights[y, x] += Mathf.PerlinNoise((float)x / TerrainSizeX * Scale * 3 + OffsetX, (float)y / TerrainSizeY * Scale * 3 + OffsetY);
                    BorderSafeZone[x, y] = true;
                    // populate border zone
                    for (int i = 0; i < BorderPadding; i++)
                    {
                        BorderZone[x + i, y] = true;
                    }
                }
            }
        }
        // right border
        randBorderSize = Random.Range(MinBorderSize, MaxBorderSize + 1);
        for (var y = 0; y < TerrainSizeY; y++)
        {
            randBorderSize += Random.Range(-1, 2);
            if (randBorderSize < MinBorderSize) randBorderSize = MinBorderSize;
            if (randBorderSize > MaxBorderSize) randBorderSize = MaxBorderSize;
            BorderRight[y] = TerrainSizeX - randBorderSize;
            for (var x = TerrainSizeX - randBorderSize - 1; x < TerrainSizeX; x++)
            {
                if (!BorderSafeZone[x, y])
                {
                    heights[y, x] += Mathf.PerlinNoise((float)x / TerrainSizeX * Scale * 3 + OffsetX, (float)y / TerrainSizeY * Scale * 3 + OffsetY);
                    BorderSafeZone[x, y] = true;
                    // populate border zone
                    for (int i = 0; i < BorderPadding; i++)
                    {
                        BorderZone[x - i, y] = true;
                    }
                }
            }
        }
        //top border
        randBorderSize = Random.Range(MinBorderSize, MaxBorderSize + 1);
        bool determinedCornerTopLeft = false;
        for (var x = 0; x < TerrainSizeX; x++)
        {
            randBorderSize += Random.Range(-1, 2);
            if (randBorderSize < MinBorderSize) randBorderSize = MinBorderSize;
            if (randBorderSize > MaxBorderSize) randBorderSize = MaxBorderSize;
            BorderTop[x] = randBorderSize;
            for (var y = 0; y < randBorderSize; y++)
            {
                if (!BorderSafeZone[x, y])
                {
                    heights[y, x] += Mathf.PerlinNoise((float)x / TerrainSizeX * Scale * 3 + OffsetX, (float)y / TerrainSizeY * Scale * 3 + OffsetY);
                    BorderSafeZone[x, y] = true;
                    if (!determinedCornerTopLeft) { CornerTopLeft = new(x, randBorderSize); determinedCornerTopLeft = true; }
                    if (determinedCornerTopLeft) CornerTopRight = new(x, y);
                    // populate border zone
                    for (int i = 0; i < BorderPadding; i++)
                    {
                        BorderZone[x, y + i] = true;
                    }
                }
            }
        }
        // bottom border
        randBorderSize = Random.Range(MinBorderSize, MaxBorderSize + 1);
        bool determinedCornerBottomLeft = false;
        for (var x = 0; x < TerrainSizeX; x++)
        {
            randBorderSize += Random.Range(-1, 2);
            if (randBorderSize < MinBorderSize) randBorderSize = MinBorderSize;
            if (randBorderSize > MaxBorderSize) randBorderSize = MaxBorderSize;
            BorderBottom[x] = TerrainSizeX - randBorderSize;
            for (var y = TerrainSizeY - randBorderSize - 1; y < TerrainSizeY; y++)
            {
                if (!BorderSafeZone[x, y])
                {
                    heights[y, x] += Mathf.PerlinNoise((float)x / TerrainSizeX * Scale * 3 + OffsetX, (float)y / TerrainSizeY * Scale * 3 + OffsetY);
                    BorderSafeZone[x, y] = true;
                    if (!determinedCornerBottomLeft) { CornerBottomLeft = new(x, y); determinedCornerBottomLeft = true; }
                    if (determinedCornerBottomLeft) CornerBottomRight = new(x, y);
                    for (int i = 0; i < BorderPadding; i++)
                    {
                        BorderZone[x, y - i] = true;
                    }
                }
            }
        }

        //Debug.Log("corners: " + CornerTopLeft + "," + CornerTopRight + "," + CornerBottomLeft + "," + CornerBottomRight);

        if (UseSmoothing) SmooothBorders(ref heights);

        // terrainData.SetHeights(0, 0, heights);
        // return terrainData;
    }

    private void SmooothBorders(ref float[,] heights)
    {
        for (int n = 0; n < SmoothPasses; n++)
        {
            //left border
            for (int y = CornerTopLeft.y; y < CornerBottomLeft.y; y++)
            {
                Vector2Int pos = new(BorderLeft[y], y);
                heights[pos.y, pos.x] = GetSmoothedValue(pos, heights, StrongerSmoothing);
                // also smooth neighbours in radius
                for (int i = 1; i <= SmoothRadius; i++)
                {
                    pos = new(BorderLeft[y] - i, y);
                    if (pos.x > 0) heights[pos.y, pos.x] = GetSmoothedValue(pos, heights, StrongerSmoothing);
                    pos = new(BorderLeft[y] + i, y);
                    if (pos.x < TerrainSizeX - 1) heights[pos.y, pos.x] = GetSmoothedValue(pos, heights, StrongerSmoothing);
                }
            }
            //right border
            for (int y = CornerTopRight.y; y < CornerBottomRight.y; y++)
            {
                Vector2Int pos = new(BorderRight[y], y);
                heights[pos.y, pos.x] = GetSmoothedValue(pos, heights, StrongerSmoothing);

                for (int i = 1; i <= SmoothRadius; i++)
                {
                    pos = new(BorderRight[y] - i, y);
                    if (pos.x > 0) heights[pos.y, pos.x] = GetSmoothedValue(pos, heights, StrongerSmoothing);
                    pos = new(BorderRight[y] + i, y);
                    if (pos.x < TerrainSizeX - 1) heights[pos.y, pos.x] = GetSmoothedValue(pos, heights, StrongerSmoothing);
                }
            }
            //top border
            for (int x = CornerTopLeft.x; x < CornerTopRight.x; x++)
            {
                Vector2Int pos = new(x, BorderTop[x]);
                heights[pos.y, pos.x] = GetSmoothedValue(pos, heights, StrongerSmoothing);

                for (int i = 1; i <= SmoothRadius; i++)
                {
                    pos = new(x, BorderTop[x] - i);
                    if (pos.y > 0) heights[pos.y, pos.x] = GetSmoothedValue(pos, heights, StrongerSmoothing);
                    pos = new(x, BorderTop[x] + i);
                    if (pos.y < TerrainSizeY - 1) heights[pos.y, pos.x] = GetSmoothedValue(pos, heights, StrongerSmoothing);
                }
            }
            //bottom border
            for (int x = CornerBottomLeft.x; x < CornerBottomRight.x; x++)
            {
                Vector2Int pos = new(x, BorderBottom[x]);
                heights[pos.y, pos.x] = GetSmoothedValue(pos, heights, StrongerSmoothing);

                for (int i = 1; i <= SmoothRadius; i++)
                {
                    pos = new(x, BorderBottom[x] - i);
                    if (pos.y > 0) heights[pos.y, pos.x] = GetSmoothedValue(pos, heights, StrongerSmoothing);
                    pos = new(x, BorderBottom[x] + i);
                    if (pos.y < TerrainSizeY - 1) heights[pos.y, pos.x] = GetSmoothedValue(pos, heights, StrongerSmoothing);
                }
            }
        }
    }

    //TODO fix IndexOutOfBounds error
    private float GetSmoothedValue(Vector2Int pos, float[,] heights, bool strongSmoothing)
    {
        // calculate neighbours -> pos > 0 this works
        var heightTopLeft = heights[pos.y - 1, pos.x - 1];
        var heightTop = heights[pos.y - 1, pos.x];
        var heightTopRight = heights[pos.y - 1, pos.x + 1];

        var heightLeft = heights[pos.y, pos.x - 1];
        var height = heights[pos.y, pos.x];
        var heightRight = heights[pos.y, pos.x + 1];

        var heightBottomLeft = heights[pos.y + 1, pos.x - 1];
        var heightBottom = heights[pos.y + 1, pos.x];
        var heightBottomRight = heights[pos.y + 1, pos.x + 1];

        float mean = (heightTopLeft + heightTop + heightTopRight + heightLeft + heightRight + heightBottomLeft + heightBottom + heightBottomRight) / 8 - height;

        return strongSmoothing ? height + mean : height + mean / 2;
    }
}
