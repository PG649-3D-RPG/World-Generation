using UnityEngine;

[CreateAssetMenu(fileName = "WorldGeneratorSettings", menuName = "PG649-WorldGeneration/World Generator Settings")]
public class WorldGeneratorSettings : ScriptableObject {
    [Header("Random")]
    public int seed = 42;
    [Header("Space Partitioning")]
    public int size = 1024;
    public SPTreeT.PartitionMode partitionMode;
    public int minPartitionWidth = 64;
    public int minPartitionDepth = 64;
    public int skipChildren = 0;
    [Header("Level Placement")]
    public int leftRightMinMargin = 11;
    public int leftRightMaxMargin = 16;
    public int frontBackMinMargin = 11;
    public int frontBackMaxMargin = 16;
    public int levelPlacementProbability = 60;
    [Header("Level Options")]
    public int numberOfTypes = 1;
    public int spawnPointsPerRoom = 1;
    public int spawnPointSize = 1;
    public bool markSpawnPoints = true;
    [Header("Corridors")]
    public int minCorridorWidth = 6;
    public int maxCorridorWidth = 10;
    public float minCorridorHeight = 2;
    public float maxCorridorHeight = 3;
    public float maxDistance = 32;
    public bool freeCorridors = true;
    [Header("PlaceObjects")]
    public bool placeObjects = true;
    public int cubesPerRoom = 12;
    public int freeSpaceBetweenObjects = 2;
    [Header("NavMesh")]
    public float agentRadius = 2.25f;
    public float noNavMeshAboveHeight = 0.125f;
    [Space]
    public BiomeSettings[] biomeSettings;
}
