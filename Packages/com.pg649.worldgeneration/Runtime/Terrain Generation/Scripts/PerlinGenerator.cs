using UnityEngine;

public class PerlinGenerator
{
    private readonly int TerrainSize;
    private readonly float Scale;
    private readonly int OffsetX;
    private readonly int OffsetY;

    public PerlinGenerator(int terrainSize, float scale, int offsetX, int offsetY)
    {
        TerrainSize = terrainSize;
        Scale = scale;
        OffsetX = offsetX;
        OffsetY = offsetY;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="terrainData">Current TerrainData</param>
    /// <returns></returns>
    public TerrainData GenerateTerrain(TerrainData terrainData)
    {

        // Generate terrain data
        // if (!GenerateHeights) return terrainData; // Do not generate terrain with heights
        // !!!IMPORTANT!!! The heights array is indexed as [y,x]. https://docs.unity3d.com/ScriptReference/TerrainData.SetHeights.html
        var heights = terrainData.GetHeights(0, 0, TerrainSize, TerrainSize);

        for (var x = 0; x < TerrainSize; x++)
        {
            for (var y = 0; y < TerrainSize; y++)
            {
                heights[y, x] = Mathf.PerlinNoise((float)x / TerrainSize * Scale + OffsetX, (float)y / TerrainSize * Scale + OffsetY);
            }
        }
        terrainData.SetHeights(0, 0, heights);
        return terrainData;
    }
}
