using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum JobType
{
    Dig, Build, Haul, Deconstruct
}

public class JobManager : MonoBehaviour
{
    [SerializeField] private PathfindingGrid pathfindingGrid;
    [SerializeField] public Tilemap highlight;
    [SerializeField] private DungeonCore dungeonCore;
    [SerializeField] private Pathfinding pathfinding;
    public Dictionary<Vector3Int, Building> buildings = new Dictionary<Vector3Int, Building>();

    public List<SkeletonWorker> allWorkers = new List<SkeletonWorker>();

    [Header("Priority tasks")]
    public int priorityDigTask = 3;
    public int priorityBuildTask = 3;
    public int priorityHaulTask = 3;
    public int priorityDeconstructTask = 3;

    public Dictionary<JobType, JobQueue<Job>> jobQueues;

    AssignmentSystem assignmentSystem;

    public static JobManager Instance;

    private void Awake()
    {
        Instance = this;

        jobQueues = new Dictionary<JobType, JobQueue<Job>>
        {
            { JobType.Haul, new JobQueue<Job>() },
            { JobType.Dig, new JobQueue<Job>() },
            { JobType.Build, new JobQueue<Job>() },
            {JobType.Deconstruct, new JobQueue<Job>() },
        };

        assignmentSystem = new AssignmentSystem(this);

        StartCoroutine(CleanUpQueues(1f));
    }

    IEnumerator CleanUpQueues(float time)
    {
        while (true)
        {
            yield return new WaitForSeconds(time);
            foreach (var queue in jobQueues.Values)
            {
                queue.CleanUp();
            }
        }
    }

    //Методы для работы с копанием
    public void AddDigJob(Vector3Int cellPos)
    {
        var job = new DigJob(cellPos);
        jobQueues[JobType.Dig].Add(job);
    }
    public void RemoveDigJob(Vector3Int cellPos)
    {
        foreach (Job deleteDigJob in jobQueues[JobType.Dig].GetJobs())
        {
            if (deleteDigJob is DigJob dig && dig.GetWorldPosition() == cellPos)
            {
                jobQueues[JobType.Dig].Remove(deleteDigJob);
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
        foreach (var worker in allWorkers)
        {
            worker.unreachableJobs.Clear();
        }
    }

    //Методы работы с строительством
    public void AddBuildJob(ConstructionSite constructionSite)
    {
        var job = new BuildJob(constructionSite);
        jobQueues[JobType.Build].Add(job);
    }
    public void RemoveBuildJob(ConstructionSite constructionSite)
    {
        foreach (Job deleteBuildJob in jobQueues[JobType.Build].GetJobs())
        {
            if (deleteBuildJob is BuildJob BuildTask && BuildTask.constructionSite == constructionSite)
            {
                jobQueues[JobType.Build].Remove(deleteBuildJob);
                break;
            }
        }
    }

    //Методы работы с переноской ресурсов

    public void AddHaulJob(WorldResource drop)
    {
        var job = new HaulJob(drop);
        jobQueues[JobType.Haul].Add(job);
    }

    //Методы работы с разбором зданий
    public void AddDeconstructJob(Building building)
    {
        var job = new DeconstructJob(building);
        jobQueues[JobType.Deconstruct].Add(job);
    }
    public void RemoveDeconstruct(Building building)
    {
        foreach (Job deleteDeconstructdJob in jobQueues[JobType.Deconstruct].GetJobs())
        {
            if (deleteDeconstructdJob is DeconstructJob deconstruct && deconstruct.building == building)
            {
                jobQueues[JobType.Deconstruct].Remove(deleteDeconstructdJob);
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
    public Job DelegateWork(SkeletonWorker worker)
    {
        return assignmentSystem.GetBestJob(worker);
    }

    public Job GetResourceForBuild(Vector3 unitPostion, List<ResourceData> resourceDatas)
    {
        Job haulJob = null;
        float minDistance = Mathf.Infinity;
        Job closestJob = null;

        foreach (var task in jobQueues[JobType.Haul].GetJobs())
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
        return haulJob;
    }
    public void ClearHighlight(Vector3Int position)
    {
        highlight.SetTile(position, null);
    }
    public int GetLivePriority(JobType type)
    {
        switch (type)
        {
            case JobType.Dig: return priorityDigTask;
            case JobType.Build: return priorityBuildTask;
            case JobType.Haul: return priorityHaulTask;
            case JobType.Deconstruct: return priorityDeconstructTask;
            default: return 3;
        }
    }
}
