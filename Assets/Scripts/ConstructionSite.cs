using Mono.Cecil;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionSite : MonoBehaviour
{
    private SpriteRenderer cunstructionSprite;

    public JobManager JobManager;

    [SerializeField] public List<ResourceCost> resourceCost = new List<ResourceCost>();
    [SerializeField] public List<ResourceCost> resourcesCollected = new List<ResourceCost>();
    [SerializeField] public int maxWorkers = 1;
    [SerializeField] public bool isReadyToBuild = false;

    float timeToBuild;
    GameObject finalBuildingPrefab;

    private void Awake()
    {
        cunstructionSprite = GetComponent<SpriteRenderer>();
    }

    public void setBuilding(BuildingData buildingData)
    {

        timeToBuild = buildingData.timeToBuild;

        finalBuildingPrefab = buildingData.actualPrefab;

        resourceCost = new List<ResourceCost>(buildingData.resourceCost);
        if (timeToBuild <= 0)
        {
            cunstructionSprite.sprite = buildingData.previewSprite;
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
        build.recipe = new List<ResourceCost>(resourceCost);
        build.jobManager = JobManager;
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
        Destroy(gameObject);
    }
}
