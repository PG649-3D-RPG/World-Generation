using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LSP Settings", menuName = "PG649-WorldGeneration/LSP Settings")]
public class LSP_Settings : ScriptableObject
{
    [Header("L-System Settings")]
    public float distance = 1f;
    public short defaultAngle = 90;
    public LS_LSystem.INITIAL_DIRECTION initialDirection = LS_LSystem.INITIAL_DIRECTION.DOWN;
    public float thickness = 1f;
    public bool translatePoints = true;
    public string startString = "FABFCD";
    public uint iterations = 1;
    public string[] rules = { "A=[+F(1.5)]", "B=[-F(1.5)]", "C=[+(30)F(1.5)C]", "D=[-(30)F(1.5)D]" };

    public LS_LSystem BuildLSystem(){
        return new LS_LSystem(distance, defaultAngle,0,0, initialDirection, startString, iterations, LS_LSystem.ParseRuleInput(rules), true);
    } 
}