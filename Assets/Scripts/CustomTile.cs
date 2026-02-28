using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Custom Tile", menuName = "Tiles/Custom Tile")]
public class CustomTile : RuleTile
{
    [Header("Настройки копания")]
    public bool isDiggable = false;
    public int baseHealth = 100;
    public ResourceData resourceData;
}