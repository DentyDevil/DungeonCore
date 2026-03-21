using System.Collections.Generic;
using UnityEngine;

public class DiggingManager : MonoBehaviour
{
    public static DiggingManager Instance;

    public Dictionary<Vector3Int, int> currentTileHealth = new();

    private HashSet<Vector3Int> previewCells = new();
    private GameMode CurrentMode => GameModeManager.Instance.CurrentMode;
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        GameModeManager.Instance.OnGameModeChanged += HandleGameModeChanged;
        InputManager.Instance.OnAreaSelected += HandleDiggingCells;
        InputManager.Instance.OnAreaDragging += HandleAreaDragging;
    }
    private void OnDestroy()
    {
        if (GameModeManager.Instance != null) GameModeManager.Instance.OnGameModeChanged -= HandleGameModeChanged;
        if (InputManager.Instance != null) { InputManager.Instance.OnAreaSelected -= HandleDiggingCells; InputManager.Instance.OnAreaDragging -= HandleAreaDragging; }

    }

    private void HandleGameModeChanged(GameMode newMode)
    {
        
    }
    void HandleDiggingCells(Vector3 startPos, Vector3 endPos)
    {
        ClearPreviev();
        if (CurrentMode == GameMode.Digging || CurrentMode == GameMode.CancelOrders)
        {
            var bounds = InputManager.Instance.CalculateBoundsSelectedArea(startPos, endPos);

            for (int x = bounds.startX; x <= bounds.endX; x++)
            {
                for(int y = bounds.startY; y <= bounds.endY; y++)
                {
                    Vector3Int cell = new Vector3Int(x, y, 0);
                    CustomTile tile = GameReference.Instance.WallsTilemap.GetTile<CustomTile>(cell);
                    if (tile != null && tile.isDiggable && CurrentMode == GameMode.Digging && GameReference.Instance.HighlightTilemap.HasTile(cell) != true)
                    {
                        GameReference.Instance.HighlightTilemap.SetTile(cell, GameReference.Instance.PickaxeHighlightTile);
                        JobManager.Instance.AddDigJob(cell);
                    }
                    else if (CurrentMode == GameMode.CancelOrders)
                    {
                        GameReference.Instance.HighlightTilemap.SetTile(cell, null);

                        if (JobManager.Instance.jobQueues[JobType.Dig].HasJobAt(cell))
                        {
                            JobManager.Instance.RemoveDigJob(cell);
                        }
                    }
                }
            }
        }
    }

    void HandleAreaDragging(Vector3 startPos, Vector3 endPos)
    {
        PrevievArea(startPos, endPos);
    }

    void PrevievArea(Vector3 startPos, Vector3 endPos)
    {
        ClearPreviev();

        var bounds = InputManager.Instance.CalculateBoundsSelectedArea(startPos, endPos);

        for (int x = bounds.startX; x <= bounds.endX; x++)
        {
            for (int y = bounds.startY; y <= bounds.endY; y++)
            {
                Vector3Int currentCell = new Vector3Int(x, y, 0);
                CustomTile tile = GameReference.Instance.WallsTilemap.GetTile<CustomTile>(currentCell);

                if (CurrentMode == GameMode.Digging && tile != null && tile.isDiggable && !GameReference.Instance.HighlightTilemap.HasTile(currentCell))
                {
                    GameReference.Instance.HighlightTilemap.SetTile(currentCell, GameReference.Instance.SelectionBoxHighlightTile);
                    previewCells.Add(currentCell);
                }
                else if(CurrentMode == GameMode.CancelOrders && GameReference.Instance.HighlightTilemap.HasTile(currentCell))
                {
                    GameReference.Instance.HighlightTilemap.SetTile(currentCell, GameReference.Instance.SelectionBoxHighlightTile);
                    previewCells.Add(currentCell);
                }
            }
        }
    }

    void ClearPreviev()
    {
        foreach (Vector3Int cell in previewCells)
        {
            if (CurrentMode == GameMode.Digging)
            {
                GameReference.Instance.HighlightTilemap.SetTile(cell, null);
            }
            else if (CurrentMode == GameMode.CancelOrders && JobManager.Instance.jobQueues[JobType.Dig].HasJobAt(cell))
            {
                GameReference.Instance.HighlightTilemap.SetTile(cell, GameReference.Instance.PickaxeHighlightTile);
            }
        }
        previewCells.Clear();
    }
    public bool DamageTile(Vector3Int cell, int damage)
    {
        CustomTile tile = GameReference.Instance.WallsTilemap.GetTile<CustomTile>(cell);
        if (tile == null) return false;
        if (!currentTileHealth.ContainsKey(cell))
        {
            currentTileHealth.Add(cell, tile.baseHealth);
        }

        int tileHealth = currentTileHealth[cell];
        tileHealth -= damage;
        currentTileHealth[cell] = tileHealth;

        if (tileHealth <= 0)
        {
            currentTileHealth.Remove(cell); FinishDiggingTask(cell);
            return true;
        }
        return false;
    }
    public void FinishDiggingTask(Vector3Int cell)
    {
        CustomTile tile = GameReference.Instance.WallsTilemap.GetTile<CustomTile>(cell);
        GameReference.Instance.HighlightTilemap.SetTile(cell, null);
        GameReference.Instance.WallsTilemap.SetTile(cell, null);

        Vector3 cellToVec = new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0);
        PathfindingManager.Instance.Grid.UpdateNodeWalkability(cellToVec, true);

        LootManager.Instance.DropFromTile(cell, tile.resourceData);

        JobManager.Instance.RemoveDigJob(cell);

        JobManager.Instance.WakeUpDelayedTasks();
    }
}
