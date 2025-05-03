using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Match3/Level Data")]
public class LevelData : ScriptableObject
{
    public Vector2Int boardSize = new(5, 5);
    public int targetRed;
    public int targetYellow;
    public int targetGreen;
    public int targetBlue;
}

