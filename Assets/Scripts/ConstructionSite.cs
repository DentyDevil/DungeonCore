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
        FinishConstruction();
    }

    void FinishConstruction()
    {
        Debug.Log("Стройка завершена");
        Instantiate(finalBuildingPrefab, transform.position, Quaternion.identity);
        JobManager.RemoveBuildJob(this);
        Destroy(gameObject);
    }
}
