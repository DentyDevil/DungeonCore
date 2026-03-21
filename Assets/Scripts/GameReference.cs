using UnityEngine;
using UnityEngine.Tilemaps;

public class GameReference : MonoBehaviour
{
   public static GameReference Instance;
    public Tilemap WallsTilemap;
    public Tilemap HighlightTilemap;
    public TileBase PickaxeHighlightTile;
    public TileBase SelectionBoxHighlightTile;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
}
