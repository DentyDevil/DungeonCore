using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Building : MonoBehaviour
{
    public List<ResourceCost> recipe = new List<ResourceCost>();
    [SerializeField] public JobManager jobManager;

    public void Deconstruct()
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
        Destroy(gameObject);
    }
}
