using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.AI.Navigation;

public class WorldGenerator {

    public static GameObject Generate(WorldGeneratorSettings settings){   
            int[] spsize = new int[] {settings.size,settings.size};
            int[] minSize = new int[] {settings.minPartitionWidth,settings.minPartitionDepth};
            Tuple<int,int>[] minMaxMargin = new Tuple<int,int>[] {new Tuple<int,int> (settings.leftRightMinMargin,settings.leftRightMaxMargin), new Tuple<int,int> (settings.frontBackMinMargin,settings.frontBackMaxMargin)};
            SPTreeT spTree;
            System.Random rand = new System.Random(settings.seed);
            switch(settings.partitionMode){
                case SPTreeT.PartitionMode.KDTreeRandom:
                    spTree = new SPTreeT(spsize, SPTreeT.KDTreeRandom(minSize), rand : rand, skipChildren : settings.skipChildren);
                    break;
                case SPTreeT.PartitionMode.QuadTreeUniform:
                    spTree = new SPTreeT(spsize, SPTreeT.QuadTreeUniform(), SPTreeT.StopMinSize(minSize), rand : rand, skipChildren : settings.skipChildren);
                    break;
                default:
                    spTree = new SPTreeT(spsize, SPTreeT.KDTreeRandom(minSize), rand : rand, skipChildren : settings.skipChildren);
                    break;
            }
            DungeonTreeT dTree = new DungeonTreeT(spTree);
            dTree.Root.Node.FHeight = DungeonTreeNode.fHeight2DMinMax(3,4);
            dTree.Root.Node.MinMaxMargin = minMaxMargin;
            dTree.PlaceRooms(settings.levelPlacementProbability);
            dTree.PlaceCorridors(settings.minCorridorWidth, settings.maxCorridorWidth, settings.minCorridorHeight, settings.maxCorridorHeight, maxDistance : settings.maxDistance);
            dTree.AssignTypes(settings.numberOfTypes);
            dTree.CreateSpawnPoints(settings.spawnPointsPerRoom, settings.spawnPointSize);

            Heightmap h = new Heightmap(settings.size);

            TerrainMasks tm = dTree.ToTerrainMasks();

            h.SetByMask(tm.intermediate, 70);
            h.AverageFilterGPU(mask : tm.intermediate, numberOfRuns : 600);
            h.PerlinNoise(maxAddedHeight : 20f, scale : 0.03f, tm.intermediate, fractalRuns : 3);
            h.Power(3, mask : tm.intermediate);
            h.PerlinNoise(maxAddedHeight : 5f, scale : 0.15f, mask : tm.intermediate, fractalRuns : 2);
            h.AverageFilterGPU(mask : tm.levelsCorridors.InvertedBorderMask(6), numberOfRuns : 15);
            h.SetByMask(tm.levelsCorridors.InvertedBorderMask(1), 1.1f);

            GameObject tgo = new GameObject("Terrain");
            tgo.tag = "ground";
            h.AddTerrainToGameObject(tgo);
            NavMeshSurface nms = tgo.GetComponent<NavMeshSurface>();
            if (nms == null) nms = tgo.AddComponent<NavMeshSurface>();
            nms.BuildNavMesh();
            return tgo;
    }
}