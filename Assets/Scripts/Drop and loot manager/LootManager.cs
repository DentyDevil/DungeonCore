using Unity.VisualScripting;
using UnityEngine;

public class LootManager : MonoBehaviour
{
    public static LootManager Instance;
    public JobManager jobManager;

    private void Awake()
    {
        Instance = this;
    }
    public void DropFromTile(Vector3Int cell, ResourceData resourceData)
    {
        if (resourceData == null) { Debug.Log("This tile dont drop anyfing"); return; }
        int chanceDropBones = Random.Range(1, 101);

        if (chanceDropBones > 80)
        {
        }
            Vector3 worldCell = new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0);

            GameObject spawnedResource = Instantiate(resourceData.prefab, worldCell, Quaternion.identity);

            WorldResource worldResource = spawnedResource.GetComponent<WorldResource>();

            jobManager.AddHaulJob(worldResource);
    }
}
