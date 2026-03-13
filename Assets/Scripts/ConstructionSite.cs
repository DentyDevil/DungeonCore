using System.Collections.Generic;
using UnityEngine;

public class ConstructionSite : MonoBehaviour
{
    private SpriteRenderer cunstructionSprite;

    public JobManager JobManager;

    [SerializeField] public List<ResourceCost> resourceCost = new List<ResourceCost>();
    [SerializeField] public List<ResourceCost> resourcesCollected = new List<ResourceCost>();
    public Dictionary<ResourceData, int> incomingResources = new Dictionary<ResourceData, int>();
    [SerializeField] public int maxWorkers = 1;
    [SerializeField] public bool isReadyToBuild = false;
    BuildingData buildingData;
    TrapData trapData;

    float timeToBuild;
    GameObject finalBuildingPrefab;

    private void Awake()
    {
        cunstructionSprite = GetComponent<SpriteRenderer>();
    }

    public void setBuilding(BuildingData _buildingData)
    {

        timeToBuild = _buildingData.timeToBuild;

        finalBuildingPrefab = _buildingData.actualPrefab;

        buildingData = _buildingData;

        resourceCost = new List<ResourceCost>(_buildingData.resourceCost);
        if (timeToBuild <= 0)
        {
            cunstructionSprite.sprite = _buildingData.previewSprite;
        }
    }
    public void SetTrap(TrapData _trapData)
    {
        timeToBuild = _trapData.timeToBuild;
        finalBuildingPrefab = _trapData.trapPrefab;
        trapData = _trapData;
        resourceCost = new List<ResourceCost>(_trapData.resourceCost);
        if(timeToBuild <= 0)
        {
            cunstructionSprite.sprite= _trapData.previewSprite;
        }
    }

    public void AddResource(ResourceData incomingData)
    {
        ResourceCost collected = resourcesCollected.Find(r => r.resourceData == incomingData);

        if (collected == null)
        {
            collected = new ResourceCost { resourceData = incomingData, count = 0};
            resourcesCollected.Add(collected);
        }

        collected.count++;
        Debug.Log($"Ресурс {incomingData} добавлен! Всего: {collected.count}");

        CheckBuildStatus();
    }

    void CheckBuildStatus()
    {
        foreach (ResourceCost needed in resourceCost)
        {
            ResourceCost collected = resourcesCollected.Find(r => r.resourceData == needed.resourceData);

            if(collected == null || collected.count < needed.count)
            {
                return;
            }
        }

        isReadyToBuild = true;
    }

    public void Construct(float workAmount)
    {
        timeToBuild -= workAmount;
        if(timeToBuild <= 0)
        {
            FinishConstruction();
        }
    }

    void FinishConstruction()
    {
        Debug.Log("Стройка завершена");
        GameObject newBuild = Instantiate(finalBuildingPrefab, transform.position, Quaternion.identity);

        Building build = newBuild.GetComponent<Building>();

        if (build != null)
        {
            build.recipe = new List<ResourceCost>(resourceCost);
            build.jobManager = JobManager.Instance;
        }
        if (buildingData != null)
        {
            if (buildingData.isDoor) PathfindingManager.Instance.Grid.UpdateDoorNodeWalkability(Vector3Int.FloorToInt(build.transform.position), true);
            else PathfindingManager.Instance.Grid.UpdateNodeWalkability(Vector3Int.FloorToInt(build.transform.position), true);
        }
        else if(trapData != null)
        {
            PathfindingManager.Instance.Grid.UpdateNodeWalkability(Vector3Int.FloorToInt(transform.position), true);
        }
        JobManager.AddBuilding(build);
        JobManager.RemoveBuildJob(this);
        Destroy(gameObject);
    }

    public void CancelConstruction()
    {
        foreach(var resource in resourcesCollected)
        {
            for (int i = 0; i < resource.count; i++)
            {
                WorldResource worldRes = Instantiate(resource.resourceData.prefab, transform.position, Quaternion.identity).GetComponent<WorldResource>();
                JobManager.AddHaulJob(worldRes);
            }
        }
        JobManager.RemoveBuildJob(this);
        InputManager.Instance.occupiedCells.Remove(Vector3Int.FloorToInt(transform.position));
        Destroy(gameObject);
    }

    public int GetTotalResourceCount(ResourceData resourceType)
    {
        int total = 0;

        var collected = resourcesCollected.Find(r => r.resourceData == resourceType);

        if(collected != null) total += collected.count;

        if (incomingResources.ContainsKey(resourceType)) total += incomingResources[resourceType];

        return total;
    }

    public void AddIncomingResource(ResourceData resourceData)
    {
        if (incomingResources.ContainsKey(resourceData)) incomingResources[resourceData]++;
        else incomingResources.Add(resourceData, 1);
    }
    public void RemoveIncomingResource(ResourceData resourceData)
    {
        if(resourceData == null) return;
        if (incomingResources.ContainsKey(resourceData))
        {
            incomingResources[resourceData]--;
            if(incomingResources[resourceData] <= 0) incomingResources.Remove(resourceData);
        }
    }

    public List<ResourceData> GetNextAllRequiredResource()
    {
        List<ResourceData> resources = new List<ResourceData>();
        foreach (var neededRes in resourceCost)
        {
            if (GetTotalResourceCount(neededRes.resourceData) < neededRes.count)
            {
                resources.Add(neededRes.resourceData);
            }
        }
        if (resources.Count > 0)
        {
            return resources;
        }
        else return null;
    }
}
