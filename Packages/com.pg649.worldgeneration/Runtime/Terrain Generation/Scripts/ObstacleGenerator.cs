using UnityEngine;

public class ObstacleGenerator
{
    // general terrain fields
    private readonly int TerrainSize;
    private readonly float Scale;

    // obstacle setting fields
    private readonly int NumberOfObstacles;
    private readonly int ObstacleWidth;
    private readonly int ObstacleHeight;
    private readonly int ObstaclePadding;

    private readonly BorderGenerator BorderGenerator;

    private readonly bool[,] ObstacleZone;

    public ObstacleGenerator(int terrainSize, float scale, int numberOfObstacles, int obstacleWidth, int obstacleHeight, int obstaclePadding, BorderGenerator borderGenerator)
    {
        TerrainSize = terrainSize;
        Scale = scale;
        NumberOfObstacles = numberOfObstacles;
        ObstacleWidth = obstacleWidth;
        ObstacleHeight = obstacleHeight;
        ObstaclePadding = obstaclePadding;
        BorderGenerator = borderGenerator;

        ObstacleZone = new bool[terrainSize, terrainSize];
    }
    public bool[,] GetObstacleZone()
    {
        return ObstacleZone;
    }
    public TerrainData GenerateObstacles(TerrainData terrainData)
    {
        var heights = terrainData.GetHeights(0, 0, TerrainSize, TerrainSize);
        //TODO Random edges for obstacles as well
        for (int i = 0; i < NumberOfObstacles; i++)
        {
            var obstaclePositions = GenerateObstaclePosition();
            if (!obstaclePositions.HasValue) continue; // if no suitable position could be found
            var obstaclePosX = obstaclePositions.Value.x;
            var obstaclePosY = obstaclePositions.Value.y;

            // create obstacle
            for (int x = obstaclePosX; x < obstaclePosX + ObstacleWidth; x++)
            {
                for (int y = obstaclePosY; y < obstaclePosY + ObstacleHeight; y++)
                {
                    heights[y, x] += Mathf.PerlinNoise((float)x / TerrainSize * Scale * 3, (float)y / TerrainSize * Scale * 3);
                    ObstacleZone[x, y] = true;
                }
            }
        }

        terrainData.SetHeights(0, 0, heights);
        return terrainData;
    }

    Vector2Int? GenerateObstaclePosition()
    {
        for (int n = 0; n < 5; n++)
        {
            var obstaclePosX = Random.Range(BorderGenerator.MaxBorderSize + 1, TerrainSize - BorderGenerator.MaxBorderSize - ObstacleWidth);
            var obstaclePosY = Random.Range(BorderGenerator.MaxBorderSize + 1, TerrainSize - BorderGenerator.MaxBorderSize - ObstacleHeight);
            // check if left/top position is outside of Border+Padding and bottom/right is outside of Border+Padding
            if (obstaclePosX > BorderGenerator.BorderLeft[obstaclePosY] + ObstaclePadding &&
            obstaclePosX + ObstacleWidth < BorderGenerator.BorderRight[obstaclePosY] + ObstaclePadding &&
            obstaclePosY > BorderGenerator.BorderTop[obstaclePosX] + ObstaclePadding &&
            obstaclePosY + ObstacleHeight < BorderGenerator.BorderBottom[obstaclePosX] + ObstaclePadding)
            {
                return new(obstaclePosX, obstaclePosY);
            }
        }
        return null; // could not find a suitable position
    }
}
