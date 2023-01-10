using System;
using Unity.AI.Navigation;
using UnityEngine;

public class WorldGenerator {

    public static GameObject Generate(WorldGeneratorSettings settings) {
        int[] spsize = new int[] { settings.size, settings.size };
        int[] minSize = new int[] { settings.minPartitionWidth, settings.minPartitionDepth };
        Tuple<int, int>[] minMaxMargin = new Tuple<int, int>[] { new Tuple<int, int>(settings.leftRightMinMargin, settings.leftRightMaxMargin), new Tuple<int, int>(settings.frontBackMinMargin, settings.frontBackMaxMargin) };
        SPTreeT spTree;
        System.Random rand = new System.Random(settings.seed);
        switch (settings.partitionMode) {
            case SPTreeT.PartitionMode.KDTreeRandom:
                spTree = new SPTreeT(spsize, SPTreeT.KDTreeRandom(minSize), rand: rand, skipChildren: settings.skipChildren);
                break;
            case SPTreeT.PartitionMode.QuadTreeUniform:
                spTree = new SPTreeT(spsize, SPTreeT.QuadTreeUniform(), SPTreeT.StopMinSize(minSize), rand: rand, skipChildren: settings.skipChildren);
                break;
            default:
                spTree = new SPTreeT(spsize, SPTreeT.KDTreeRandom(minSize), rand: rand, skipChildren: settings.skipChildren);
                break;
        }
        DungeonTreeT dTree = new DungeonTreeT(spTree);
        dTree.Root.Node.FHeight = DungeonTreeNode.fHeight2DMinMax(3, 4);
        dTree.Root.Node.MinMaxMargin = minMaxMargin;
        dTree.PlaceRooms(settings.levelPlacementProbability);
        dTree.PlaceCorridors(settings.minCorridorWidth, settings.maxCorridorWidth, settings.minCorridorHeight, settings.maxCorridorHeight, maxDistance: settings.maxDistance);
        dTree.AssignTypes(settings.numberOfTypes);
        dTree.CreateSpawnPoints(settings.spawnPointsPerRoom, settings.spawnPointSize);

        Heightmap h = new Heightmap(settings.size);

        TerrainMasks tm = dTree.ToTerrainMasks();

        h.SetByMask(tm.intermediate, 70);
        h.AverageFilter(mask: tm.intermediate, numberOfRuns: 600);
        h.PerlinNoise(maxAddedHeight: 20f, scale: 0.03f, tm.intermediate, fractalRuns: 3);
        h.Power(3, mask: tm.intermediate);
        h.PerlinNoise(maxAddedHeight: 5f, scale: 0.15f, mask: tm.intermediate, fractalRuns: 2);
        h.AverageFilter(mask: tm.levelsCorridors.InvertedBorderMask(6), numberOfRuns: 15);
        h.SetByMask(tm.levelsCorridors.InvertedBorderMask(1), 1.1f);

        GameObject tgo = new GameObject("Terrain") {
            tag = "ground"
        };
        h.AddTerrainToGameObject(tgo);

        if(settings.placeObjects){
            PlaceableCube cube3 = new PlaceableCube(size : 3);
            PlaceableCube cube5 = new PlaceableCube(size : 5);
            PlaceableCube cube7 = new PlaceableCube(size : 7);
            dTree.AddPlaceableToRooms(cube3,settings.cubesPerRoom/3, freeSpace : settings.freeSpaceBetweenObjects);
            dTree.AddPlaceableToRooms(cube5,settings.cubesPerRoom/3, freeSpace : settings.freeSpaceBetweenObjects);
            dTree.AddPlaceableToRooms(cube7,settings.cubesPerRoom/3, freeSpace : settings.freeSpaceBetweenObjects);
            dTree.AddPlaceablesToGameObject(tgo);
        }

        NavMeshSurface nms = tgo.GetComponent<NavMeshSurface>();
        if (nms == null) nms = tgo.AddComponent<NavMeshSurface>();
        nms.BuildNavMesh();

        TerrainCollider col = tgo.GetComponent<TerrainCollider>();
        if (col == null) col = tgo.AddComponent<TerrainCollider>();
        col.terrainData = tgo.GetComponent<Terrain>().terrainData;

        // Add spawnPoints to misc terrain data Component
        var miscData = tgo.AddComponent<MiscTerrainData>();
        miscData.SpawnPoints = dTree.SpawnPoints();

        return tgo;
    }
}
