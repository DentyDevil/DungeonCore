using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InputManager : MonoBehaviour
{

    public enum GameMode
    {
        None,
        Digging,
        CancelDigging,
        Building,
        CancelBuilding,
        DeconstructBuilding
    }
    private GameMode currentMode = GameMode.None;

    [SerializeField] private Tilemap wallsTileMap;
    [SerializeField] private Tilemap highlightTilemap;
    [SerializeField] private TileBase pickaxeTile;
    [SerializeField] private RectTransform selectionBox;
    [SerializeField] private float minBounds = 0.2f;
    [SerializeField] private float maxBounds = 0.2f;
    private BuildingData currentBuildingData;
    private ConstructionSite constructionSite;
    [SerializeField] private SpriteRenderer buildPreview;
    [SerializeField] private GameObject constructionSitePrefab;

    private Vector3 startMousePos;
    private Vector3 endMousePos;
    private Vector3 startMouseScreenPos;

    private HashSet<Vector3Int> previewCells = new HashSet<Vector3Int>();

    public HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();

    public Dictionary<Vector3Int, int> currentTileHealth = new Dictionary<Vector3Int, int>();

    public PathfindingGrid pathfinderGrid;
    public DungeonCore dungeonCore;
    public JobManager jobManager;
    public LootManager lootManager;

    private Camera mainCamera;
    private Vector3Int startCell;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        MouseInput();
        ResetAction();
        CheckObstaclesForBuild();
    }

    void MouseInput()
    {
        if (Input.GetMouseButtonDown(0) && (currentMode == GameMode.Digging || currentMode == GameMode.CancelDigging || currentMode == GameMode.CancelBuilding || currentMode == GameMode.DeconstructBuilding))
        {
            selectionBox.gameObject.SetActive(true);

            startMouseScreenPos = Input.mousePosition;
            selectionBox.position = startMouseScreenPos;
            selectionBox.sizeDelta = Vector2.zero;

            startMousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            startMousePos.z = 0;
            startCell = GetMouseCellPosition();
        }
        else if (Input.GetMouseButton(0) && (currentMode == GameMode.Digging || currentMode == GameMode.CancelDigging || currentMode == GameMode.CancelBuilding || currentMode == GameMode.DeconstructBuilding))
        {
            Vector3 currentMouseScreenPos = Input.mousePosition;
            endMousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            selectionBox.position = (startMouseScreenPos + currentMouseScreenPos) / 2;


            PreviewArea(startMousePos, endMousePos);


            float width = Mathf.Abs(currentMouseScreenPos.x - startMouseScreenPos.x);
            float height = Mathf.Abs(currentMouseScreenPos.y - startMouseScreenPos.y);
            selectionBox.sizeDelta = new Vector2(width, height);
        }
        else if (Input.GetMouseButtonUp(0) && (currentMode == GameMode.Digging || currentMode == GameMode.CancelDigging || currentMode == GameMode.CancelBuilding || currentMode == GameMode.DeconstructBuilding))
        {
            if (selectionBox.gameObject.activeSelf)
            {
                ClearPreview();
                SelectArea(startMousePos, endMousePos);
                selectionBox.gameObject.SetActive(false);
            }
        }

        if (currentMode == GameMode.Building)
        {
            Vector3 mousePosition = GetMouseCellPosition();
            mousePosition.y += 0.5f;
            mousePosition.x += 0.5f;
            buildPreview.transform.position = mousePosition;
            if (Input.GetMouseButtonUp(0) && (occupiedCells.Contains(GetMouseCellPosition()) || wallsTileMap.HasTile(GetMouseCellPosition()) == true)) 
            {
                Debug.Log("You cant build this here!");
                return;
            }
            if(Input.GetMouseButtonUp(0) && !occupiedCells.Contains(GetMouseCellPosition()))
            {
                GameObject newBuild = Instantiate(constructionSitePrefab, mousePosition, Quaternion.identity);
                constructionSite = newBuild.GetComponent<ConstructionSite>();
                constructionSite.setBuilding(currentBuildingData);
                constructionSite.JobManager = jobManager;
                jobManager.AddBuildJob(constructionSite);
                occupiedCells.Add(GetMouseCellPosition());
                pathfinderGrid.UpdateNodeWalkability(mousePosition, false);
                Debug.Log("You plase spikes to build");
            }
        }
    }
    public void SetDiggingMode()
    {
        if (currentMode != GameMode.Digging)
        {
            currentMode = GameMode.Digging;
            buildPreview.gameObject.SetActive(false);
            Debug.Log($"Current mode {currentMode}");
        }
        else
        {
            currentMode = GameMode.None;
            Debug.Log($"Current mode {currentMode}");
        }
    }
    public void SetCancelDiggingMode()
    {
        if (currentMode != GameMode.CancelDigging)
        {   
            currentMode = GameMode.CancelDigging;
            buildPreview.gameObject.SetActive(false);
            Debug.Log($"Current mode {currentMode}");
        }
        else
        {
            currentMode = GameMode.None;
            Debug.Log($"Current mode {currentMode}");
        }
    }
    private Vector3Int GetMouseCellPosition()
    {
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0;
        return wallsTileMap.WorldToCell(mouseWorldPosition);
    }
    public void DamageTile(Vector3Int cell, int damage)
    {
        CustomTile tile = wallsTileMap.GetTile<CustomTile>(cell);
        if (tile == null) return;
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
        }
    }
    public void FinishDiggingTask(Vector3Int cell)
    {
        CustomTile tile = wallsTileMap.GetTile<CustomTile>(cell);
        highlightTilemap.SetTile(cell, null);
        wallsTileMap.SetTile(cell, null);

        Vector3 cellToVec = new Vector3(cell.x +0.5f, cell.y + 0.5f, 0);
        pathfinderGrid.UpdateNodeWalkability(cellToVec, true);

        lootManager.DropFromTile(cell, tile.resourceData);

        jobManager.RemoveDigJob(cell);

        jobManager.WakeUpDelayedTasks();
    }
    private void SelectArea(Vector3 start, Vector3 end)
    {
        var bounds = CalculateBoundsSelectedArea(start, end);

        for (int x = bounds.startX; x <= bounds.endX; x++)
        {
            for (int y = bounds.startY; y <= bounds.endY; y++)
            {
                Vector3Int currentCell = new Vector3Int(x, y, 0);

                CustomTile tile = wallsTileMap.GetTile<CustomTile>(currentCell);

                if (tile != null && tile.isDiggable && currentMode == GameMode.Digging && highlightTilemap.HasTile(currentCell) != true)
                {
                    highlightTilemap.SetTile(currentCell, pickaxeTile);
                    jobManager.AddDigJob(currentCell);
                }
                else if (currentMode == GameMode.CancelDigging)
                {
                    highlightTilemap.SetTile(currentCell, null);

                    if (jobManager.digJobs.queue.HasJobAt(currentCell) || jobManager.HasUnreachableJobAt(currentCell))
                    {
                        jobManager.RemoveDigJob(currentCell);
                    }
                }
                else if (currentMode == GameMode.CancelBuilding)
                {
                    if (jobManager.buildJobs.queue.HasJobAt(currentCell))
                    {
                        Job currJob = jobManager.buildJobs.queue.GetJobAt(currentCell);
                        currJob.constructionSite.CancelConstruction();
                        pathfinderGrid.UpdateNodeWalkability(currentCell, true);
                        occupiedCells.Remove(currentCell);
                    }
                }
                else if (currentMode == GameMode.DeconstructBuilding)
                {
                    if (jobManager.buildings.ContainsKey(currentCell))
                    {
                        Building build = jobManager.buildings[currentCell];
                        jobManager.AddDeconstructJob(build);
                        highlightTilemap.SetTile(currentCell, pickaxeTile);
                        jobManager.RemoveBuilding(build);
                    }
                }
            }
        }
        jobManager.WakeUpDelayedTasks();
    }
    private void PreviewArea(Vector3 start, Vector3 end)
    {
        ClearPreview(); 

        var bounds = CalculateBoundsSelectedArea(start, end);

        for (int x = bounds.startX; x <= bounds.endX; x++)
        {
            for (int y = bounds.startY; y <= bounds.endY; y++)
            {
                Vector3Int currentCell = new Vector3Int(x, y, 0);
                CustomTile tile = wallsTileMap.GetTile<CustomTile>(currentCell);

                
                if (currentMode == GameMode.Digging && tile != null && tile.isDiggable && !highlightTilemap.HasTile(currentCell))
                {
                    highlightTilemap.SetTile(currentCell, pickaxeTile);
                    previewCells.Add(currentCell);
                }
            }
        }
    }
    private void ClearPreview()
    {
        foreach (Vector3Int cell in previewCells)
        {
            if (currentMode == GameMode.Digging)
            {
                highlightTilemap.SetTile(cell, null);
            }
            else if (currentMode == GameMode.CancelDigging && (jobManager.digJobs.queue.HasJobAt(cell) || jobManager.HasUnreachableJobAt(cell)))
            {
                highlightTilemap.SetTile(cell, pickaxeTile);
            }
        }
        previewCells.Clear();
    }
    public(int startX, int endX, int startY, int endY) CalculateBoundsSelectedArea(Vector3 start, Vector3 end)
    {
        float minX = Mathf.Min(start.x, end.x) + minBounds;
        float maxX = Mathf.Max(start.x, end.x) - maxBounds;
        float minY = Mathf.Min(start.y, end.y) + minBounds;
        float maxY = Mathf.Max(start.y, end.y) - maxBounds;

        int startX = Mathf.FloorToInt(minX);
        int endX = Mathf.FloorToInt(maxX);
        int startY = Mathf.FloorToInt(minY);
        int endY = Mathf.FloorToInt(maxY);

        return(startX, endX, startY, endY);
    }
    public void SetBuildingMode()
    {
        if(currentMode != GameMode.Building)
        {
            currentMode = GameMode.Building;
            Debug.Log($"Current mode {currentMode}");
        }
        else
        {
            currentMode = GameMode.None;
            buildPreview.gameObject.SetActive(false);
            Debug.Log($"Current mode {currentMode}");
        }
    }
    public void SetDeconstructBuilding()
    {
        if (currentMode != GameMode.DeconstructBuilding)
        {
            currentMode = GameMode.DeconstructBuilding;
            Debug.Log($"Current mode {currentMode}");
        }
        else
        {
            currentMode = GameMode.None;
            buildPreview.gameObject.SetActive(false);
            Debug.Log($"Current mode {currentMode}");
        }
    }
    public void SetCancelBuildingMode()
    {
        if (currentMode != GameMode.CancelBuilding)
        {
            currentMode = GameMode.CancelBuilding;
            Debug.Log($"Current mode {currentMode}");
        }
        else
        {
            currentMode = GameMode.None;
            buildPreview.gameObject.SetActive(false);
            Debug.Log($"Current mode {currentMode}");
        }
    }
    public void Build(BuildingData selectedBuilding)
    {
        if(currentMode == GameMode.Building)
        {
            currentBuildingData = selectedBuilding;
            buildPreview.sprite = currentBuildingData.previewSprite;
            buildPreview.gameObject.SetActive(true);
        }
        else
        {
            buildPreview.gameObject.SetActive(false);
        }
    }
    void ResetAction()
    {
        if (currentMode != GameMode.None)
        {
            if (Input.GetMouseButtonDown(1))
            {
                currentMode = GameMode.None;
                Debug.Log($"Current mode {currentMode}");
                buildPreview.gameObject.SetActive(false);
            }
        }
    }
    void CheckObstaclesForBuild()
    {
        if (occupiedCells.Contains(GetMouseCellPosition()) || wallsTileMap.HasTile(GetMouseCellPosition()))
        {
            buildPreview.color = Color.red;
        }
        else
        {
            buildPreview.color= Color.white;
        }

    }
}

