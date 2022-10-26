using UnityEngine;

public class PerlinGenerator
{
    // private readonly int TerrainSize;
    private readonly float Scale;
    private readonly int OffsetX;
    private readonly int OffsetY;

    public PerlinGenerator(float scale, int offsetX, int offsetY)
    {
        // TerrainSize = terrainSize;
        Scale = scale;
        OffsetX = offsetX;
        OffsetY = offsetY;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="terrainData">Current TerrainData</param>
    /// <returns></returns>
    public void GenerateTerrain(ref float[,] heights)
    {

        // Generate terrain data
        // if (!GenerateHeights) return terrainData; // Do not generate terrain with heights
        // !!!IMPORTANT!!! The heights array is indexed as [y,x]. https://docs.unity3d.com/ScriptReference/TerrainData.SetHeights.html
        //var heights = terrainData.GetHeights(0, 0, TerrainSize, TerrainSize);

        for (var x = 0; x < heights.GetLength(1); x++)
        {
            for (var y = 0; y < heights.GetLength(0); y++)
            {
                heights[y, x] = Mathf.PerlinNoise((float)x / heights.GetLength(0) * Scale + OffsetX, (float)y / heights.GetLength(1) * Scale + OffsetY);
            }
        }
        //terrainData.SetHeights(0, 0, heights);
        //return terrainData;
    }
}
