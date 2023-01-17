using System;
using System.Collections.Generic;
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
        dTree.PlaceCorridors(settings.minCorridorWidth, settings.maxCorridorWidth, settings.minCorridorHeight, settings.maxCorridorHeight, maxDistance: settings.maxDistance, freeCorridors : settings.freeCorridors);
        dTree.AssignTypes(settings.numberOfTypes);
        dTree.CreateSpawnPoints(settings.spawnPointsPerRoom, settings.spawnPointSize, agentRadius: settings.agentRadius);

        Heightmap h = new Heightmap(settings.size);

        TerrainMasks tm = dTree.ToTerrainMasks();

        h.SetByMask(tm.intermediate, 70);
        h.AverageFilter(mask: tm.intermediate, numberOfRuns: 600);
        h.PerlinNoise(maxAddedHeight: 20f, scale: 0.03f, tm.intermediate, fractalRuns: 3);
        h.Power(3, mask: tm.intermediate);
        h.PerlinNoise(maxAddedHeight: 5f, scale: 0.15f, mask: tm.intermediate, fractalRuns: 2);
        h.AverageFilter(mask: tm.levelsCorridors.InvertedBorderMask(6), numberOfRuns: 15);
        h.SetByMask(tm.levelsCorridors.InvertedBorderMask(1), 1.1f);

        List<Tuple<Vector3Int, int>> sp = dTree.SpawnPoints();
        // foreach(Tuple<Vector3Int, int> t in sp){
        //     h.heights[t.Item1.x,t.Item1.z] = .01f;
        // }

        GameObject tgo = new GameObject("Terrain") {
            tag = "ground"
        };
        h.AddTerrainToGameObject(tgo);

        TerrainMod tmod = new TerrainMod(tgo.GetComponent<Terrain>(), settings.size, settings.size);
        if(settings.markSpawnPoints) tmod.MarkSpawnPoints( sp, Texture2D.grayTexture);

        if(settings.biomeSettings.Length == settings.numberOfTypes && settings.numberOfTypes > 0){
            List<DungeonRoom>[] drla = dTree.GetRoomsByType();
            for(int i = 0; i < drla.Length; i++){
                Placeable[] pl = settings.biomeSettings[i].GetPlaceables(settings.seed);
                foreach(DungeonRoom drl in drla[i]){
                    float n = settings.biomeSettings[i].objectsSquareMeter * drl.Width * drl.Depth;
                    for(int j = 0; j < settings.biomeSettings[i].objects.Length; j++){
                        for(int k = 0; k < settings.biomeSettings[i].objects[j].p * n; k++){
                            if(!drl.PlacePlaceable(pl[j], freeSpace : settings.freeSpaceBetweenObjects)) break;
                        }
                    }
                }
            }
        }
        else if(settings.placeObjects){
            PlaceableCube cube3 = new PlaceableCube(size : 3);
            PlaceableCube cube5 = new PlaceableCube(size : 5);
            PlaceableCube cube7 = new PlaceableCube(size : 7);
            dTree.AddPlaceableToRooms(cube7,settings.cubesPerRoom/3, freeSpace : settings.freeSpaceBetweenObjects);
            dTree.AddPlaceableToRooms(cube5,settings.cubesPerRoom/3, freeSpace : settings.freeSpaceBetweenObjects);
            dTree.AddPlaceableToRooms(cube3,settings.cubesPerRoom/3, freeSpace : settings.freeSpaceBetweenObjects);
        }
        dTree.AddPlaceablesToGameObject(tgo);

        NavMeshSurface nms = tgo.GetComponent<NavMeshSurface>();
        if (nms == null) nms = tgo.AddComponent<NavMeshSurface>();
        // generate no navmesh above noNavMeshAboveHeight
        NavMeshModifierVolume nmv = tgo.AddComponent<NavMeshModifierVolume>();
        nmv.area = 1; //non walkable
        nmv.center = new Vector3(settings.size / 2, h.heightScale / 2 + settings.noNavMeshAboveHeight, settings.size / 2);
        nmv.size = new Vector3(settings.size, h.heightScale, settings.size);
        nms.BuildNavMesh();

        TerrainCollider col = tgo.GetComponent<TerrainCollider>();
        if (col == null) col = tgo.AddComponent<TerrainCollider>();
        col.terrainData = tgo.GetComponent<Terrain>().terrainData;

        // Add spawnPoints to misc terrain data Component
        var miscData = tgo.AddComponent<MiscTerrainData>();
        miscData.SpawnPoints = sp;
        


        return tgo;
    }
}
