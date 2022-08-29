using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public int ObstacleSize = 10;
    public int NumberOfObstacles = 5;
    public int BorderSize = 10;

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

    public void Start()
    {
        // OffsetX = Random.Range(0f, 9999f);
        // OffsetY = Random.Range(0f, 9999f);
        Terrain = GetComponent<Terrain>();
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

        // Generate Terrain
        Terrain.terrainData = GenerateTerrain(Terrain.terrainData);

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
        for (var x = 0; x < BorderSize; x++)
        {
            for (var y = 0; y < TerrainSize; y++)
            {
                heights[x, y] += Mathf.PerlinNoise((float)x / TerrainSize * Scale * 3 + OffsetX, (float)y / TerrainSize * Scale * 3 + OffsetY);
            }
        }
        for (var x = TerrainSize - BorderSize; x < TerrainSize; x++)
        {
            for (var y = 0; y < TerrainSize; y++)
            {
                heights[x, y] += Mathf.PerlinNoise((float)x / TerrainSize * Scale * 3 + OffsetX, (float)y / TerrainSize * Scale * 3 + OffsetY);
            }
        }
        for (var y = 0; y < BorderSize; y++)
        {
            for (var x = 0; x < TerrainSize; x++)
            {
                heights[x, y] += Mathf.PerlinNoise((float)x / TerrainSize * Scale * 3 + OffsetX, (float)y / TerrainSize * Scale * 3 + OffsetY);
            }
        }
        for (var y = TerrainSize - BorderSize; y < TerrainSize; y++)
        {
            for (var x = 0; x < TerrainSize; x++)
            {
                heights[x, y] += Mathf.PerlinNoise((float)x / TerrainSize * Scale * 3 + OffsetX, (float)y / TerrainSize * Scale * 3 + OffsetY);
            }
        }


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
        for (int i = 0; i < NumberOfObstacles; i++)
        {
            // TODO check if there is overlap with another obstacle => try again up to n times
            var obstacleX = Random.Range(BorderSize + 1, TerrainSize - BorderSize - ObstacleSize);
            var obstacleY = Random.Range(BorderSize + 1, TerrainSize - BorderSize - ObstacleSize);
            for (int x = obstacleX; x < obstacleX + ObstacleSize; x++)
            {
                for (int y = obstacleY; y < obstacleY + ObstacleSize; y++)
                {
                    heights[x, y] += Mathf.PerlinNoise((float)x / TerrainSize * Scale * 3, (float)y / TerrainSize * Scale * 3);
                }
            }
        }

        terrainData.SetHeights(0, 0, heights);
        return terrainData;
    }
}
