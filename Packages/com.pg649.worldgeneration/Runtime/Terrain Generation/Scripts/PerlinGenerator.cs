using System.Collections.Generic;
// using System.Linq;
using UnityEngine;
// using Unity.AI.Navigation;
using Random = UnityEngine.Random;

public class PerlinGenerator : MonoBehaviour
// {
//     // Start is called before the first frame update
//     void Start()
//     {

//     }

//     // Update is called once per frame
//     void Update()
//     {

//     }
// }



// public class TerrainGenerator : MonoBehaviour
{
    private Terrain _terrain;
    // private  DynamicEnviormentGenerator Deg { get; set; }

    // private NavMeshSurface NavMeshSurface { get; set; }

    private GameObject ObstaclesContainer { get; set; }

    private Terrain Terrain { get; set; }

    private float OffsetX { get; set; } = 0;

    private float OffsetY { get; set; } = 0;

    [Header("Terrain settings")]
    [Space(10)]
    [SerializeField]
    public bool GenerateHeights = true;
    [SerializeField]
    public int Depth = 10;
    [SerializeField]
    public float Scale = 2.5f;
    // [SerializeField]
    // public float ScaleObstacle = 10f;
    private readonly int TerrainSize = 256; // must be 2^n
    public readonly int ObstacleSize = 10;
    public readonly int NumberOfObstacles = 5;
    private readonly int ObstaclePadding = 10;
    // public readonly int BorderSize = 10;

    public int? RandomSeed = null;

    public int MaxBorderSize = 12;
    public int MinBorderSize = 6;

    public readonly bool GeneratePlants = false;

    public bool smoothBorder = true;
    public bool strongerSmoothing = false;
    public int smoothRadius = 4;
    public int smoothPasses = 1;

    private int BorderPadding = 10;

    private bool[,] borderZone;
    private bool[,] ObstacleZone;
    private Dictionary<string, int> CustomTerrainLayerIndices = new();
    // public bool GenerateObstacles = true;
    // [SerializeField] public GameObject ObstaclePrefab;
    // public float ObstacleThreshold = 0.9f;


    /// <summary>
    /// 
    /// </summary>
    // private void Awake()
    // {
    //     _terrain = GetComponent<Terrain>();
    //     // NavMeshSurface = GetComponent<NavMeshSurface>();
    //     // Deg = GameObject.FindObjectOfType<DynamicEnviormentGenerator>();
    //     ObstaclesContainer = new GameObject
    //     {
    //         name = "Obstacle Container",
    //         transform = { parent = this.transform }
    //     };
    // }

    public void BuildTerrain()
    {
        // OffsetX = Random.Range(0f, 9999f);
        // OffsetY = Random.Range(0f, 9999f);
        if (RandomSeed.HasValue) Random.InitState(RandomSeed.Value);

        Terrain = GetComponent<Terrain>();
        // DestroyImmediate(Terrain);
        RegenerateTerrain();
    }

    /// <summary>
    /// 
    /// </summary>
    public void RegenerateTerrain()
    {
        // OffsetX = Random.Range(0f, 9999f);
        // OffsetY = Random.Range(0f, 9999f);

        // Kill obstacles
        // foreach (Transform child in ObstaclesContainer.transform)
        // {
        //     GameObject.Destroy(child.gameObject);
        // }

        //TODO all state management code should be removed by outsourcing the editor control to a meta-environment generator class
        // Generate Terrain
        foreach (var layerName in CustomTerrainLayerIndices.Keys)
        {
            RemoveTerrainLayer(Terrain.terrainData, layerName);
        }
        CustomTerrainLayerIndices.Clear();
        Terrain.terrainData = GenerateTerrain(Terrain.terrainData);
        Terrain.Flush();

        // if (!BakeNavMesh) return; // Skipp Nav Mesh generation
        // NavMeshSurface.BuildNavMesh();
    }

    /// <summary>
    /// Gets height (y) of terrain at Vector x.
    /// </summary>
    /// <param name="position">Position vector of object on terrain</param>
    /// <returns></returns>
    public float GetTerrainHeight(Vector3 position)
    {
        return _terrain.SampleHeight(position);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="terrainData">Current TerrainData</param>
    /// <returns></returns>
    private TerrainData GenerateTerrain(TerrainData terrainData)
    {
        terrainData.heightmapResolution = TerrainSize + 1;
        terrainData.size = new Vector3(TerrainSize, Depth, TerrainSize);
        borderZone = new bool[TerrainSize, TerrainSize];
        ObstacleZone = new bool[TerrainSize, TerrainSize];

        // Generate terrain data
        // if (!GenerateHeights) return terrainData; // Do not generate terrain with heights

        var heights = new float[TerrainSize, TerrainSize];
        for (var x = 0; x < TerrainSize; x++)
        {
            for (var y = 0; y < TerrainSize; y++)
            {
                heights[x, y] = Mathf.PerlinNoise((float)x / TerrainSize * Scale + OffsetX, (float)y / TerrainSize * Scale + OffsetY);
            }
        }

        // hills around edges
        //slower naive random variant


        //storage for the border location
        // storage for the border limits on all sides
        int[] borderLeft = new int[TerrainSize];
        int[] borderRight = new int[TerrainSize];
        int[] borderTop = new int[TerrainSize];
        int[] borderBottom = new int[TerrainSize];
        // corners with placeholder values
        //TODO improve the placeholder assignment
        Vector2Int cornerTopLeft = new(1, 1);
        Vector2Int cornerTopRight = new(1, 1);
        Vector2Int cornerBottomLeft = new(1, 1);
        Vector2Int cornerBottomRight = new(1, 1);
        // coordinates where borders are located
        bool[,] bordersApplied = new bool[TerrainSize, TerrainSize];

        // left border
        int randBorderSize = Random.Range(MinBorderSize, MaxBorderSize + 1);
        for (var y = 0; y < TerrainSize; y++)
        {
            randBorderSize += Random.Range(-1, 2);
            if (randBorderSize < MinBorderSize) randBorderSize = MinBorderSize;
            if (randBorderSize > MaxBorderSize) randBorderSize = MaxBorderSize;
            borderLeft[y] = randBorderSize;
            for (var x = 0; x < randBorderSize; x++)
            {
                heights[x, y] += Mathf.PerlinNoise((float)x / TerrainSize * Scale * 3 + OffsetX, (float)y / TerrainSize * Scale * 3 + OffsetY);
                bordersApplied[x, y] = true;
                // populate border zone
                for (int i = 0; i < BorderPadding; i++)
                {
                    borderZone[x + i, y] = true;
                }
            }
        }
        // right border
        randBorderSize = Random.Range(MinBorderSize, MaxBorderSize + 1);
        for (var y = 0; y < TerrainSize; y++)
        {
            randBorderSize += Random.Range(-1, 2);
            if (randBorderSize < MinBorderSize) randBorderSize = MinBorderSize;
            if (randBorderSize > MaxBorderSize) randBorderSize = MaxBorderSize;
            borderRight[y] = TerrainSize - randBorderSize;
            for (var x = TerrainSize - randBorderSize - 1; x < TerrainSize; x++)
            {
                heights[x, y] += Mathf.PerlinNoise((float)x / TerrainSize * Scale * 3 + OffsetX, (float)y / TerrainSize * Scale * 3 + OffsetY);
                bordersApplied[x, y] = true;
                // populate border zone
                for (int i = 0; i < BorderPadding; i++)
                {
                    borderZone[x - i, y] = true;
                }
            }
        }
        //top border
        randBorderSize = Random.Range(MinBorderSize, MaxBorderSize + 1);
        bool determinedCornerTopLeft = false;
        for (var x = 0; x < TerrainSize; x++)
        {
            randBorderSize += Random.Range(-1, 2);
            if (randBorderSize < MinBorderSize) randBorderSize = MinBorderSize;
            if (randBorderSize > MaxBorderSize) randBorderSize = MaxBorderSize;
            borderTop[x] = randBorderSize;
            for (var y = 0; y < randBorderSize; y++)
            {
                if (!bordersApplied[x, y])
                {
                    heights[x, y] += Mathf.PerlinNoise((float)x / TerrainSize * Scale * 3 + OffsetX, (float)y / TerrainSize * Scale * 3 + OffsetY);
                    bordersApplied[x, y] = true;
                    if (!determinedCornerTopLeft) { cornerTopLeft = new(x, randBorderSize); determinedCornerTopLeft = true; }
                    if (determinedCornerTopLeft) cornerTopRight = new(x, y);
                    // populate border zone
                    for (int i = 0; i < BorderPadding; i++)
                    {
                        borderZone[x, y + i] = true;
                    }
                }
            }
        }
        // bottom border
        randBorderSize = Random.Range(MinBorderSize, MaxBorderSize + 1);
        bool determinedCornerBottomLeft = false;
        for (var x = 0; x < TerrainSize; x++)
        {
            randBorderSize += Random.Range(-1, 2);
            if (randBorderSize < MinBorderSize) randBorderSize = MinBorderSize;
            if (randBorderSize > MaxBorderSize) randBorderSize = MaxBorderSize;
            borderBottom[x] = TerrainSize - randBorderSize;
            for (var y = TerrainSize - randBorderSize - 1; y < TerrainSize; y++)
            {
                if (!bordersApplied[x, y])
                {
                    heights[x, y] += Mathf.PerlinNoise((float)x / TerrainSize * Scale * 3 + OffsetX, (float)y / TerrainSize * Scale * 3 + OffsetY);
                    bordersApplied[x, y] = true;
                    if (!determinedCornerBottomLeft) { cornerBottomLeft = new(x, y); determinedCornerBottomLeft = true; }
                    if (determinedCornerBottomLeft) cornerBottomRight = new(x, y);
                    for (int i = 0; i < BorderPadding; i++)
                    {
                        borderZone[x, y - i] = true;
                    }
                }
            }
        }

        Debug.Log("corners: " + cornerTopLeft + "," + cornerTopRight + "," + cornerBottomLeft + "," + cornerBottomRight);
        if (smoothBorder)
        {
            for (int n = 0; n < smoothPasses; n++)
            {
                //left border
                for (int y = cornerTopLeft.y; y < cornerBottomLeft.y; y++)
                {
                    Vector2Int pos = new(borderLeft[y], y);
                    heights[pos.x, pos.y] = GetSmoothedValue(pos, heights, strongerSmoothing);
                    // also smooth neighbours in radius
                    for (int i = 1; i <= smoothRadius; i++)
                    {
                        pos = new(borderLeft[y] - i, y);
                        if (pos.x > 0) heights[pos.x, pos.y] = GetSmoothedValue(pos, heights, strongerSmoothing);
                        pos = new(borderLeft[y] + i, y);
                        if (pos.x < TerrainSize - 1) heights[pos.x, pos.y] = GetSmoothedValue(pos, heights, strongerSmoothing);
                    }
                }
                //right border
                for (int y = cornerTopRight.y; y < cornerBottomRight.y; y++)
                {
                    Vector2Int pos = new(borderRight[y], y);
                    heights[pos.x, pos.y] = GetSmoothedValue(pos, heights, strongerSmoothing);

                    for (int i = 1; i <= smoothRadius; i++)
                    {
                        pos = new(borderRight[y] - i, y);
                        if (pos.x > 0) heights[pos.x, pos.y] = GetSmoothedValue(pos, heights, strongerSmoothing);
                        pos = new(borderRight[y] + i, y);
                        if (pos.x < TerrainSize - 1) heights[pos.x, pos.y] = GetSmoothedValue(pos, heights, strongerSmoothing);
                    }
                }
                //top border
                for (int x = cornerTopLeft.x; x < cornerTopRight.x; x++)
                {
                    Vector2Int pos = new(x, borderTop[x]);
                    heights[pos.x, pos.y] = GetSmoothedValue(pos, heights, strongerSmoothing);

                    for (int i = 1; i <= smoothRadius; i++)
                    {
                        pos = new(x, borderTop[x] - i);
                        if (pos.y > 0) heights[pos.x, pos.y] = GetSmoothedValue(pos, heights, strongerSmoothing);
                        pos = new(x, borderTop[x] + i);
                        if (pos.y < TerrainSize - 1) heights[pos.x, pos.y] = GetSmoothedValue(pos, heights, strongerSmoothing);
                    }
                }
                //bottom border
                for (int x = cornerBottomLeft.x; x < cornerBottomRight.x; x++)
                {
                    Vector2Int pos = new(x, borderBottom[x]);
                    heights[pos.x, pos.y] = GetSmoothedValue(pos, heights, strongerSmoothing);

                    for (int i = 1; i <= smoothRadius; i++)
                    {
                        pos = new(x, borderBottom[x] - i);
                        if (pos.y > 0) heights[pos.x, pos.y] = GetSmoothedValue(pos, heights, strongerSmoothing);
                        pos = new(x, borderBottom[x] + i);
                        if (pos.y < TerrainSize - 1) heights[pos.x, pos.y] = GetSmoothedValue(pos, heights, strongerSmoothing);
                    }
                }
            }
        }

        // for (var x = 0; x < BorderSize; x++)
        // {
        //     for (var y = 0; y < TerrainSize; y++)
        //     {
        //         heights[x, y] += Mathf.PerlinNoise((float)x / TerrainSize * Scale * 3 + OffsetX, (float)y / TerrainSize * Scale * 3 + OffsetY);
        //     }
        // }
        // for (var x = TerrainSize - BorderSize; x < TerrainSize; x++)
        // {
        //     for (var y = 0; y < TerrainSize; y++)
        //     {
        //         heights[x, y] += Mathf.PerlinNoise((float)x / TerrainSize * Scale * 3 + OffsetX, (float)y / TerrainSize * Scale * 3 + OffsetY);
        //     }
        // }
        // for (var y = 0; y < BorderSize; y++)
        // {
        //     for (var x = 0; x < TerrainSize; x++)
        //     {
        //         heights[x, y] += Mathf.PerlinNoise((float)x / TerrainSize * Scale * 3 + OffsetX, (float)y / TerrainSize * Scale * 3 + OffsetY);
        //     }
        // }
        // for (var y = TerrainSize - BorderSize; y < TerrainSize; y++)
        // {
        //     for (var x = 0; x < TerrainSize; x++)
        //     {
        //         heights[x, y] += Mathf.PerlinNoise((float)x / TerrainSize * Scale * 3 + OffsetX, (float)y / TerrainSize * Scale * 3 + OffsetY);
        //     }
        // }


        // Generate obstacles
        // if (!GenerateObstacles) return terrainData; // Do not generate obstacles



        // for (var x = 11; x < TerrainSize - 10 - ObstacleSize; x++)
        // {
        //     for (var y = 1; y < TerrainSize - 10 - ObstacleSize; y++)
        //     {
        //         for (var o_x = 0; o_x < ObstacleSize;)
        //         // if (!(Mathf.PerlinNoise((x + OffsetX) / TerrainSize * ScaleObstacle, (y + OffsetY) / TerrainSize * ScaleObstacle + OffsetY) > ObstacleThreshold)) continue;
        //         // var newObstacle = GameObject.Instantiate(ObstaclePrefab, Vector3.zero, Quaternion.identity, ObstaclesContainer.transform);
        //         // newObstacle.transform.localPosition = new Vector3(x, terrainData.GetHeight(x, y) + 2f, y);
        //     }
        // }
#nullable enable
        Vector2Int? GenerateObstaclePosition()
        {
            for (int n = 0; n < 5; n++)
            {
                var obstaclePosX = Random.Range(MaxBorderSize + 1, TerrainSize - MaxBorderSize - ObstacleSize);
                var obstaclePosY = Random.Range(MaxBorderSize + 1, TerrainSize - MaxBorderSize - ObstacleSize);
                // check if left/top position is outside of Border+Padding and bottom/right is outside of Border+Padding
                if (obstaclePosX > borderLeft[obstaclePosY] + ObstaclePadding &&
                obstaclePosX + ObstacleSize < borderRight[obstaclePosY] + ObstaclePadding &&
                obstaclePosY > borderTop[obstaclePosX] + ObstaclePadding &&
                obstaclePosY + ObstacleSize < borderBottom[obstaclePosX] + ObstaclePadding)
                {
                    return new(obstaclePosX, obstaclePosY);
                }
            }
            return null; // could not find a suitable position
        }
#nullable disable
        //TODO Random edges for obstacles as well
        for (int i = 0; i < NumberOfObstacles; i++)
        {
            var obstaclePositions = GenerateObstaclePosition();
            if (!obstaclePositions.HasValue) continue; // if no suitable position could be found
            var obstaclePosX = obstaclePositions.Value.x;
            var obstaclePosY = obstaclePositions.Value.y;
            for (int x = obstaclePosX; x < obstaclePosX + ObstacleSize; x++)
            {
                for (int y = obstaclePosY; y < obstaclePosY + ObstacleSize; y++)
                {
                    heights[x, y] += Mathf.PerlinNoise((float)x / TerrainSize * Scale * 3, (float)y / TerrainSize * Scale * 3);
                    ObstacleZone[x, y] = true;
                }
            }
        }

        terrainData.SetHeights(0, 0, heights);
        return terrainData;
    }

    public void ShowBorderZone()
    {
        Texture2D texture = new(TerrainSize, TerrainSize);
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                Color color = borderZone[x, y] ? Color.red : Color.clear;
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
        SetTerrainTexture(Terrain.terrainData, texture, TerrainSize, "border");
    }

    public void RemoveBorderZoneLayer()
    {
        RemoveTerrainLayer(Terrain.terrainData, "border");
    }

    public void ShowObstacleZone()
    {
        Texture2D texture = new(TerrainSize, TerrainSize);
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                //TODO fix: Unity texture and terrain coords are not the same
                Color color = ObstacleZone[x, y] ? Color.green : Color.clear;
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
        SetTerrainTexture(Terrain.terrainData, texture, TerrainSize, "obstacles");
    }

    public void RemoveObstacleZone()
    {
        RemoveTerrainLayer(Terrain.terrainData, "obstacles");
    }

    private float GetSmoothedValue(Vector2Int pos, float[,] heights, bool strongSmoothing)
    {
        // calculate neighbours -> pos > 0 this works
        var heightTopLeft = heights[pos.x - 1, pos.y - 1];
        var heightTop = heights[pos.x, pos.y - 1];
        var heightTopRight = heights[pos.x + 1, pos.y - 1];

        var heightLeft = heights[pos.x - 1, pos.y];
        var height = heights[pos.x, pos.y];
        var heightRight = heights[pos.x + 1, pos.y];

        var heightBottomLeft = heights[pos.x - 1, pos.y + 1];
        var heightBottom = heights[pos.x, pos.y + 1];
        var heightBottomRight = heights[pos.x + 1, pos.y + 1];

        float mean = (heightTopLeft + heightTop + heightTopRight + heightLeft + heightRight + heightBottomLeft + heightBottom + heightBottomRight) / 8 - height;

        return strongSmoothing ? height + mean : height + mean / 2;
    }

    // from https://forum.unity.com/threads/terrain-layers-api-can-you-tell-me-the-starting-point.606019/#post-4966541
    /// <summary>
    /// Adds the given texture as an extra layer to the given terrain.
    /// </summary>
    /// <param name="terrainData"><see cref="TerrainData"/> to modify the texture of.</param>
    /// <param name="texture">Texture to be used.</param>
    /// <param name="size">Size of the <see cref="Terrain"/> in meters.</param>
    private void SetTerrainTexture(TerrainData terrainData, Texture2D texture, float size, string name)
    {
        var newTextureLayer = new TerrainLayer();
        newTextureLayer.diffuseTexture = texture;
        newTextureLayer.tileOffset = Vector2.zero;
        newTextureLayer.tileSize = Vector2.one * size;

        AddTerrainLayer(terrainData, newTextureLayer, name);
    }

    /// <summary>
    /// Adds new <see cref="TerrainLayer"/> to the given <see cref="TerrainData"/> object.
    /// </summary>
    /// <param name="terrainData"><see cref="TerrainData"/> to add layer to.</param>
    /// <param name="inputLayer"><see cref="TerrainLayer"/> to add.</param>
    private void AddTerrainLayer(TerrainData terrainData, TerrainLayer inputLayer, string layerName)
    {
        if (inputLayer == null)
            return;

        var layers = terrainData.terrainLayers;
        for (var idx = 0; idx < layers.Length; ++idx)
        {
            if (layers[idx] == inputLayer)
                return;
        }

        int newIndex = layers.Length;
        var newarray = new TerrainLayer[newIndex + 1];
        System.Array.Copy(layers, 0, newarray, 0, newIndex);
        newarray[newIndex] = inputLayer;
        CustomTerrainLayerIndices[layerName] = newIndex;

        terrainData.terrainLayers = newarray;
    }

    private void RemoveTerrainLayer(TerrainData terrainData, string layerName)
    {
        if (CustomTerrainLayerIndices.TryGetValue(layerName, out int layernum))
        {
            var oldLayers = terrainData.terrainLayers;
            var newLayers = new TerrainLayer[oldLayers.Length - 1];
            for (int i = 0; i < oldLayers.Length; i++)
            {
                if (i == layernum) continue;
                newLayers[i] = oldLayers[i];
            }
            terrainData.terrainLayers = newLayers;
            CustomTerrainLayerIndices.Remove(layerName);
        }
    }
}
