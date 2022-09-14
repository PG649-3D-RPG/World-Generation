using UnityEngine;


public enum ZONES
{
    BORDERS,
    OBSTACLES,
    FREE,
    USED
}
public class ZoneManager
{
    // from https://forum.unity.com/threads/terrain-layers-api-can-you-tell-me-the-starting-point.606019/#post-4966541
    public static TerrainLayer ShowZone(int size, bool[,] coordsMask, Color color)
    {
        Texture2D texture = new(size, size);
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                texture.SetPixel(x, y, coordsMask[x, y] ? color : Color.clear);
            }
        }
        texture.Apply();
        return new TerrainLayer
        {
            diffuseTexture = texture,
            tileOffset = Vector2.zero,
            tileSize = Vector2.one * size
        };
    }
}
