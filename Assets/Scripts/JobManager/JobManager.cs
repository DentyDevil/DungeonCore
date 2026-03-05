using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class JobManager : MonoBehaviour
{
    [SerializeField] private PathfindingGrid pathfindingGrid;
    [SerializeField] public Tilemap highlight;
    [SerializeField] private DungeonCore dungeonCore;
    [SerializeField] private Pathfinding pathfinding;
    public Dictionary<Vector3Int, Building> buildings = new Dictionary<Vector3Int, Building>();
    public HashSet<Job> unreachebleTasks = new HashSet<Job>();

    [Header("Priority tasks")]
    public int priorityDigTask = 3;
    public int priorityBuildTask = 3;
    public int priorityHaulTask = 3;
    public int priorityDeconstructTask = 3;

    public HaulJobQueueMB haulJobs;
    public DigJobQueueMB digJobs;
    public BuildJobQueueMB buildJobs;
    public DeconstructJobQueueMB deconstructJobs;

    AssignmentSystem assignmentSystem;

    private void Awake()
    {
        assignmentSystem = new AssignmentSystem(haulJobs, digJobs, buildJobs, deconstructJobs, unreachebleTasks, priorityDigTask, priorityBuildTask, priorityHaulTask, priorityDeconstructTask);
    }

    //Методы для работы с копанием
    public void AddDigJob(Vector3Int cellPos)
    {
        var job = new DigJob(cellPos, priorityDigTask);
        digJobs.queue.Add(job);
    }
    public void RemoveDigJob(Vector3Int cellPos)
    {
        foreach (Job deleteDigJob in digJobs.queue.GetJobs())
        {
            if (deleteDigJob is DigJob dig && dig.GetWorldPosition() == cellPos)
            {
                digJobs.queue.Remove(deleteDigJob);
                unreachebleTasks.Remove(deleteDigJob);
                break;
            }
        }
    }
    public void JobBecomeFree(Job job, int workers)
    {
        job.workersInWork = Mathf.Max(0, job.workersInWork - workers);
    }

    public void WakeUpDelayedTasks()
    {
        unreachebleTasks.Clear();
    }

    //Методы работы с строительством
    public void AddBuildJob(ConstructionSite constructionSite)
    {
        var job = new BuildJob(constructionSite, priorityBuildTask);
        buildJobs.queue.Add(job);
    }
    public void RemoveBuildJob(ConstructionSite constructionSite)
    {
        foreach (Job deleteBuildJob in buildJobs.queue.GetJobs())
        {
            if (deleteBuildJob is BuildJob BuildTask && BuildTask.constructionSite == constructionSite)
            {
                buildJobs.queue.Remove(deleteBuildJob);
                break;
            }
        }
    }

    //Методы работы с переноской ресурсов

    public void AddHaulJob(WorldResource drop)
    {
        var job = new HaulJob(drop, priorityHaulTask);
        haulJobs.queue.Add(job);
    }

    //Методы работы с разбором зданий
    public void AddDeconstructJob(Building building)
    {
        var job = new DeconstructJob(building, priorityDeconstructTask);
        deconstructJobs.queue.Add(job);
    }
    public void RemoveDeconstruct(Building building)
    {
        foreach (Job deleteDeconstructdJob in deconstructJobs.queue.GetJobs())
        {
            if (deleteDeconstructdJob is DeconstructJob deconstruct && deconstruct.building == building)
            {
                deconstructJobs.queue.Remove(deleteDeconstructdJob);
                break;
            }
        }
    }
    //Методы добавления зданий в список зданий
    public void AddBuilding(Building building)
    {
        Vector3Int position = Vector3Int.FloorToInt(building.transform.position);
        buildings.TryAdd(position, building);
    }
    public void RemoveBuilding(Building building)
    {
        Vector3Int position = Vector3Int.FloorToInt(building.transform.position);
        buildings.Remove(position);
    }

    //Методы выдачи работы
    public Job DelegateWork(Vector3 unitPosition)
    {
        return assignmentSystem.GetBestJob(unitPosition);
    }

    public Job GetResourceForBuild(Vector3 unitPostion, List<ResourceData> resourceDatas)
    {
        Job haulJob = null;
        float minDistance = Mathf.Infinity;
        Job closestJob = null;

        foreach (var task in haulJobs.queue.GetJobs())
        {
            if (task is HaulJob haulTask)
            {
                if (resourceDatas.Contains(haulTask.worldResource.resourceData) == false) continue;
                if (haulTask.workersInWork >= 1) continue;

                float distance = Vector3.Distance(unitPostion, haulTask.worldResource.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    haulJob = haulTask;
                    closestJob = haulTask;
                }
            }
        }
        if (closestJob != null) closestJob.workersInWork++;
        return haulJob;
    }
    public void ClearHighlight(Vector3Int position)
    {
        highlight.SetTile(position, null);
    }

    public bool HasUnreachableJobAt(Vector3Int pos)
    {
        foreach (var unreac in unreachebleTasks)
        {
            Vector3Int curPos = Vector3Int.FloorToInt(unreac.GetWorldPosition());
            if(pos == curPos)
            {
                return true;

            }
        }
        return false;
    }
}
