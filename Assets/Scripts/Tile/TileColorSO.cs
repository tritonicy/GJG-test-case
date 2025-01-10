using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// TileColor enum to store the color of the tile, regardless of the group size
/// </summary>
public enum TileColor {
    red,
    green,
    blue,
    yellow,
    purple,
    pink,
    none
}

/// <summary>
/// TileColorSO class to store the tile color scriptable object
/// </summary>
[CreateAssetMenu(fileName = "TileColorSO", menuName = "ScriptableObjects/TileColorSO")]
public class TileColorSO : ScriptableObject
{
    [SerializeField] public Tile tileDefaut;
    [SerializeField] public Tile tileA;
    [SerializeField] public Tile tileB;
    [SerializeField] public Tile tileC;
    [SerializeField] public TileColor color;
}
