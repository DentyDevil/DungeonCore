using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildManager : MonoBehaviour
{
    public static BuildManager instance;

    [Header("Ссылки")]
    [SerializeField] private SpriteRenderer buildPreview;
    [SerializeField] private GameObject constructionSitePrefab;
    private ConstructionSite constructionSite;

    [Header("Данные")]
    public HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();
    private BuildingData currentBuildingData;
    private TrapData currentTrapData;
    private GameMode CurrentMode => GameModeManager.Instance.CurrentMode;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        GameModeManager.Instance.OnGameModeChanged += HandleGameModeChanged;

        InputManager.Instance.OnCellClicked += HandleCellClicked;

        InputManager.Instance.OnAreaSelected += HandleAreaSelected;
    }

    private void OnDestroy()
    {
        if (GameModeManager.Instance != null) GameModeManager.Instance.OnGameModeChanged -= HandleGameModeChanged;
        if (InputManager.Instance != null) { InputManager.Instance.OnCellClicked -= HandleCellClicked; InputManager.Instance.OnAreaSelected -= HandleAreaSelected; }
    }

    private void Update()
    {
        if (CurrentMode == GameMode.Building) { CheckObstaclesForBuild(); Vector3 cell = InputManager.Instance.GetMouseCellPosition(); cell.x += 0.5f; cell.y += 0.5f; buildPreview.transform.position = cell; }
    }

    void HandleAreaSelected(Vector3 startPos, Vector3 endPos)
    {
        var bounds = InputManager.Instance.CalculateBoundsSelectedArea(startPos, endPos);

        for(int x = bounds.startX; x <= bounds.endX; x++)
        {
            for(int y = bounds.startY; y <= bounds.endY; y++)
            {
                    Vector3Int cell = new Vector3Int(x, y, 0);
                if (CurrentMode == GameMode.CancelOrders)
                {

                    if (JobManager.Instance.jobQueues[JobType.Build].GetJobAt(cell) is BuildJob buildTask)
                    {
                        buildTask.constructionSite.CancelConstruction();
                        PathfindingManager.Instance.Grid.UpdateNodeWalkability(cell, true);
                        BuildManager.instance.occupiedCells.Remove(cell);
                    }
                    if (JobManager.Instance.jobQueues[JobType.Deconstruct].GetJobAt(cell) is DeconstructJob deconstructJob)
                    {
                        Building build = deconstructJob.building;
                        JobManager.Instance.RemoveDeconstruct(build);
                        GameReference.Instance.HighlightTilemap.SetTile(cell, null);
                        JobManager.Instance.AddBuilding(build);
                    }
                }
                else if (CurrentMode == GameMode.DeconstructBuilding)
                {
                    if (JobManager.Instance.buildings.ContainsKey(cell))
                    {
                        Building build = JobManager.Instance.buildings[cell];
                        JobManager.Instance.AddDeconstructJob(build);
                        GameReference.Instance.HighlightTilemap.SetTile(cell, GameReference.Instance.PickaxeHighlightTile);
                        JobManager.Instance.RemoveBuilding(build);
                    }
                }
            }
        }
    }

    void HandleCellClicked(Vector3Int clickedCell)
    {
        if (CurrentMode == GameMode.Building)
        {
            Vector3 cell = clickedCell; 
            cell.x += 0.5f; cell.y += 0.5f;
            if ((occupiedCells.Contains(clickedCell) || GameReference.Instance.WallsTilemap.HasTile(clickedCell) == true))
            {
                Debug.Log("You cant build this here!");
                return;
            }
            if (!occupiedCells.Contains(clickedCell))
            {
                GameObject newBuild = Instantiate(constructionSitePrefab, cell, Quaternion.identity);
                constructionSite = newBuild.GetComponent<ConstructionSite>();
                if (currentBuildingData != null)
                {
                    constructionSite.setBuilding(currentBuildingData);
                    PathfindingManager.Instance.Grid.UpdateNodeWalkability(clickedCell, false);
                }
                else if (currentTrapData != null)
                {
                    constructionSite.SetTrap(currentTrapData);
                }
                constructionSite.JobManager = JobManager.Instance;
                JobManager.Instance.AddBuildJob(constructionSite);
                occupiedCells.Add(clickedCell);

                Debug.Log("You plase site to build");
            }
        }
    }

    void HandleGameModeChanged(GameMode newMode)
    {
        if (newMode != GameMode.Building)
        {
            buildPreview.gameObject.SetActive(false);
            currentBuildingData = null;
            currentTrapData = null;
        }
    }

    public void Build(BuildingData selectedBuilding)
    {
        if (CurrentMode == GameMode.Building)
        {
            currentBuildingData = selectedBuilding;
            currentTrapData = null;
            buildPreview.sprite = currentBuildingData.previewSprite;
            buildPreview.gameObject.SetActive(true);
        }
        else
        {
            buildPreview.gameObject.SetActive(false);
        }
    }
    public void BuildTrap(TrapData trapData)
    {
        if (CurrentMode == GameMode.Building)
        {
            currentTrapData = trapData;
            currentBuildingData = null;
            buildPreview.sprite = trapData.previewSprite;
            buildPreview.gameObject.SetActive(true);
        }
        else
        {
            buildPreview.gameObject.SetActive(false);
        }
    }
    void CheckObstaclesForBuild()
    {
        if (occupiedCells.Contains(InputManager.Instance.GetMouseCellPosition()) || GameReference.Instance.WallsTilemap.HasTile(InputManager.Instance.GetMouseCellPosition()))
        {
            buildPreview.color = Color.red;
        }
        else
        {
            buildPreview.color = Color.white;
        }
    }
}
