using UnityEngine;


public enum ZONES
{
    BORDERS,
    OBSTACLES
}
public class ZoneManager
{
    //// from https://forum.unity.com/threads/terrain-layers-api-can-you-tell-me-the-starting-point.606019/#post-4966541
    ///// <summary>
    ///// Adds the given texture as an extra layer to the given terrain.
    ///// </summary>
    ///// <param name="terrainData"><see cref="TerrainData"/> to modify the texture of.</param>
    ///// <param name="texture">Texture to be used.</param>
    ///// <param name="size">Size of the <see cref="Terrain"/> in meters.</param>
    //private TerrainLayer GetTerrainTextureLayer(Texture2D texture, float size)
    //{
    //    var newTextureLayer = new TerrainLayer();
    //    newTextureLayer.diffuseTexture = texture;
    //    newTextureLayer.tileOffset = Vector2.zero;
    //    newTextureLayer.tileSize = Vector2.one * size;
    //    return newTextureLayer;

    //}
    //public TerrainLayer ShowBorderZone(int size, bool[,] borderZone)
    //{
    //    Texture2D texture = new(size, size);
    //    for (int y = 0; y < texture.height; y++)
    //    {
    //        for (int x = 0; x < texture.width; x++)
    //        {
    //            Color color = borderZone[x, y] ? Color.red : Color.clear;
    //            texture.SetPixel(x, y, color);
    //        }
    //    }
    //    texture.Apply();
    //    return GetTerrainTextureLayer(texture, size);
    //}

    ////public void RemoveBorderZoneLayer()
    ////{
    ////    RemoveTerrainLayer(Terrain.terrainData, "border");
    ////}

    //public TerrainLayer ShowObstacleZone(int size, bool[,] obstacleZone)
    //{
    //    Texture2D texture = new(size, size);
    //    for (int x = 0; x < texture.width; x++)
    //    {
    //        for (int y = 0; y < texture.height; y++)
    //        {
    //            texture.SetPixel(x, y, obstacleZone[x, y] ? Color.green : Color.clear);
    //        }
    //    }
    //    texture.Apply();
    //    return GetTerrainTextureLayer(texture, size);
    //}
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

    //public void RemoveObstacleZone()
    //{
    //    RemoveTerrainLayer(Terrain.terrainData, "obstacles");
    //}
}
