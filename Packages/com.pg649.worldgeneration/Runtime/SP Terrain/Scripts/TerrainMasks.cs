using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum TerrainMasksE{
    Full,
    LevelsCorridors,
    Levels,
    LevelsFree,
    LevelsOccupied,
    Corridors,
    Intermediate,
    LevelType,
    LevelTypeFree,
    levelTypeOccupied
}

public class TerrainMasks{
    public Mask full,levelsCorridors, levels, levelsFree, levelsOccupied, corridors, intermediate;
    public Mask[] levelType, levelTypeFree, levelTypeOccupied;

    public TerrainMasks(Mask levels, Mask levelsFree, Mask corridors, Mask[] levelType){
        this.full = levels.Map(x => true);
        this.levelsCorridors = levels + corridors;
        this.levels = levels;
        this.levelsFree = levelsFree;
        this.levelsOccupied = levels * (!levelsFree);
        this.corridors = corridors;
        this.intermediate = !levelsCorridors;
        this.levelType = levelType;
        levelTypeFree = new Mask[levelType.Length];
        levelTypeOccupied = new Mask[levelType.Length];
        for(int i = 0; i < levelType.Length; i++){
            levelTypeFree[i] = levelType[i] - levelsOccupied;
            levelTypeOccupied[i] = levelsOccupied * levelType[i];
        }
        
    }

    public Mask MaskByEnum(TerrainMasksE tm, int type = 0){
        switch(tm){
            case TerrainMasksE.Full: return full;
            case TerrainMasksE.LevelsCorridors: return levelsCorridors;
            case TerrainMasksE.Levels: return levels;
            case TerrainMasksE.LevelsFree: return levelsFree;
            case TerrainMasksE.LevelsOccupied : return levelsOccupied;
            case TerrainMasksE.Corridors: return corridors;
            case TerrainMasksE.Intermediate: return intermediate;
            case TerrainMasksE.LevelType: return levelType[type];
            case TerrainMasksE.LevelTypeFree: return levelTypeFree[type];
            case TerrainMasksE.levelTypeOccupied: return levelTypeOccupied[type];
            default: return levelsCorridors;

        }
    }
    

}
