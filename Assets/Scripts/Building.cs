using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public List<ResourceCost> recipe = new List<ResourceCost>();
    public JobManager jobManager;
    float timer = 0;
    float timeToDestroy = 5f;

    public bool Deconstruct()
    {
        if (TakeDeconstructWork(timeToDestroy))
        {
            ReturnResource();
            return true;
        }
        return false;
    }

    void ReturnResource()
    {
        foreach (ResourceCost returneResource in recipe)
        {
            int refund = Mathf.Max(1, returneResource.count / 2);

            for (int i = 0; i < refund; i++)
            {
                WorldResource worldRes = Instantiate(returneResource.resourceData.prefab, transform.position, Quaternion.identity).GetComponent<WorldResource>();
                jobManager.AddHaulJob(worldRes);
            }
        }

        Vector3Int cell = Vector3Int.FloorToInt(transform.position);
        cell.z = 0;
        jobManager.ClearHighlight(cell);
        jobManager.RemoveDeconstruct(this);
        PathfindingManager.Instance.Grid.UpdateNodeWalkability(cell, true);
        PathfindingManager.Instance.Grid.UpdateDoorNodeWalkability(cell, false);
        BuildManager.instance.occupiedCells.Remove(Vector3Int.FloorToInt(transform.position));
        Destroy(gameObject);
    }
    bool TakeDeconstructWork(float _timeToDestroy)
    {
        timer += Time.deltaTime;
        if( timer >= _timeToDestroy)
        {
            return true;
        }
        return false;
    }
}
