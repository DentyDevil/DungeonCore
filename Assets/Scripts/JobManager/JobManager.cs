using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class JobManager : MonoBehaviour
{
    [SerializeField] private PathfindingGrid pathfindingGrid;
    [SerializeField] public Tilemap highlight;
    [SerializeField] private DungeonCore dungeonCore;
    [SerializeField] private Pathfinding pathfinding;
    public Dictionary<Vector3Int, Building> buildings = new Dictionary<Vector3Int, Building>();
    public HashSet<Vector3Int> unreachebleTasks = new HashSet<Vector3Int>();

    [Header("Priority tasks")]
    public int priorityDigTask = 3;
    public int priorityBuildTask = 3;
    public int priorityHaulTask = 3;
    public int priorityDeconstructTask = 3;

    public HaulJobQueueMB haulJobs;
    public DigJobQueueMB digJobs;
    public BuildJobQueueMB buildJobs;
    public DeconstructJobQueueMB deconstructJobs;

    //Методы для работы с копанием
    public void AddDigJob(Vector3Int cellPos)
    {
        var job = new Job(JobType.Dig, cellPos);
        digJobs.queue.Add(job);
    }
    public void RemoveDigJob(Vector3Int cellPos)
    {
        foreach (Job deleteDigJob in digJobs.queue.GetJobs())
        {
            if (deleteDigJob.position == cellPos)
            {
                digJobs.queue.Remove(deleteDigJob);
                break;
            }
        }
            unreachebleTasks.Remove(cellPos);
    }
    public void JobBecomeFree(Job job, int workers)
    {
        job.workersInWork = Mathf.Max(0, job.workersInWork - workers);
    }

    public void WakeUpDelayedTasks()
    {
        foreach (Vector3Int delayedTasks in unreachebleTasks)
        {
            Vector3Int cell = delayedTasks;
            AddDigJob(cell);
        }
        unreachebleTasks.Clear();
    }

    //Методы работы с строительством
    public void AddBuildJob(ConstructionSite constructionSite)
    {
        var job = new Job(JobType.Build, constructionSite, 2);
        buildJobs.queue.Add(job);
    }
    public void RemoveBuildJob(ConstructionSite constructionSite)
    {
        foreach (Job deleteBuildJob in buildJobs.queue.GetJobs())
        {
            if (deleteBuildJob.constructionSite == constructionSite)
            {
                buildJobs.queue.Remove(deleteBuildJob);
                break;
            }
        }
    }

    //Методы работы с переноской ресурсов

    public void AddHaulJob(WorldResource drop)
    {
        var job = new Job(JobType.Haul, drop);
        haulJobs.queue.Add(job);
        
    }

    //Методы работы с разбором зданий
    public void AddDeconstructJob(Building building)
    {
        var job = new Job(JobType.Deconstruct, building);
        deconstructJobs.queue.Add(job);
    }
    public void RemoveDeconstruct(Building building)
    {
        foreach (Job deleteDeconstructdJob in deconstructJobs.queue.GetJobs())
        {
            if (deleteDeconstructdJob.building == building)
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
        Job closestJob = null;
        int bestPriority = int.MaxValue;
        float minDistance = Mathf.Infinity;

        List<(Job job, float dist, int priority)> candidates = new List<(Job job, float dist, int priority)> ();


        var (bestBuildJob, buildDist) = TryGetBuildJob(unitPosition);
        if (bestBuildJob != null) candidates.Add((bestBuildJob, buildDist, priorityBuildTask));
        var (bestDigdJob, digDist) = TryGetDigJob(unitPosition);
        if (bestDigdJob != null) candidates.Add((bestDigdJob, digDist, priorityDigTask));
        var (bestHaulJob, haulDist) = TryGetHaulJob(unitPosition);
        if (bestHaulJob != null) candidates.Add((bestHaulJob, haulDist, priorityHaulTask));
        var (BestDestrucrionJob, destructionDistance) = TryGetDeconstructionJob(unitPosition);
        if (BestDestrucrionJob != null) candidates.Add((BestDestrucrionJob, destructionDistance, priorityDeconstructTask));
        
        foreach (var cand in candidates)
        {
            int priority = cand.priority;
            float distance = cand.dist;

            if(priority < bestPriority)
            {
                bestPriority = priority;
                minDistance = distance;
                closestJob = cand.job;
            }
            else if (priority == bestPriority)
            {
                if(distance < minDistance)
                {
                    minDistance = distance;
                    closestJob = cand.job;
                }
            }
        }



        if (closestJob != null) closestJob.workersInWork++;
        return closestJob;
    }

    public Job GetResourceForBuild(Vector3 unitPostion, List<ResourceData> resourceDatas)
    {
        Job haulJob = null;
        float minDistance = Mathf.Infinity;
        Job closestJob = null;

        foreach (var haulTask in haulJobs.queue.GetJobs())
        {
            if(resourceDatas.Contains(haulTask.worldResource.resourceData) == false) continue;
            if (haulTask.workersInWork >= 1) continue;

            float distance = Vector3.Distance(unitPostion, haulTask.worldResource.transform.position);
            if(distance < minDistance)
            {
                minDistance = distance;
                haulJob = haulTask;
                closestJob = haulTask;
            }
        }
        if (closestJob != null) closestJob.workersInWork++;
        return haulJob;
    }

    (Job BestJob, float ClosestDistance) TryGetBuildJob(Vector3 unitPosition)
    {
        Job closestJob = null;
        int bestPriority = int.MaxValue;
        float minDistance = Mathf.Infinity;

        foreach (var buildTask in buildJobs.queue.GetJobs())
        {
            if (buildTask.workersInWork >= 1) continue;
            if(!HasResourcesForBuild(buildTask.constructionSite) && !buildTask.constructionSite.isReadyToBuild) continue;
            if (pathfinding.FindPath(unitPosition, buildTask.position) == null) continue;

            Vector3 jobPos = buildTask.position;

            float distance = Vector3.Distance(unitPosition, jobPos);

            if (buildTask.workPriority < bestPriority)
            {
                bestPriority = buildTask.workPriority;
                minDistance = distance;
                closestJob = buildTask;
            }
            else if (buildTask.workPriority == bestPriority)
            {
                if (distance < minDistance)
                {
                    closestJob = buildTask;
                    minDistance = distance;
                }
            }
        }
        return (closestJob, minDistance);
    }

    (Job BestJob, float ClosestDistance) TryGetDigJob(Vector3 unitPosition)
    {
        Job closestJob = null;
        int bestPriority = int.MaxValue;
        float minDistance = Mathf.Infinity;

        List<Job> taskToDelay = new List<Job>();

        foreach (var digTask in digJobs.queue.GetJobs())
        {
            if (digTask.workersInWork >= 1) continue;
            if (pathfinding.FindPath(unitPosition, digTask.position) == null) continue;

            Vector3 taskWorldPos = new Vector3(digTask.position.x + 0.5f, digTask.position.y + 0.5f, 0);

            Node taskNode = pathfindingGrid.NodeFromWorldPoint(taskWorldPos);

            bool isReachable = false;

            foreach (Node neigbor in pathfindingGrid.GetOrthogonalNeighbors(taskNode))
            {
                if (neigbor.isWalkable)
                {
                    isReachable = true;
                    break;
                }
            }

            if (!isReachable)
            {
                taskToDelay.Add(digTask);
            }
            float distance = Vector3.Distance(unitPosition, taskWorldPos);

            if (isReachable == false) continue;

            if (digTask.workPriority < bestPriority)
            {
                bestPriority = digTask.workPriority;
                minDistance = distance;
                closestJob = digTask;
            }
            else if (digTask.workPriority == bestPriority)
            {
                if (distance < minDistance)
                {
                    closestJob = digTask;
                    minDistance = distance;
                }
            }


        }
        foreach (Job _taskToDelay in taskToDelay)
        {
            unreachebleTasks.Add(_taskToDelay.position);
            digJobs.queue.Remove(_taskToDelay);
        }

        return (closestJob, minDistance);
    }

    (Job BestJob, float ClosestDistance) TryGetHaulJob(Vector3 unitPosition)
    {
        float distance = Mathf.Infinity;
        Job bestJob = null;
        haulJobs.queue.TryGetBestFor(unitPosition, out distance, out bestJob);

        return (bestJob, distance);
    }

    (Job BestJob, float bestDistance) TryGetDeconstructionJob(Vector3 unitPosition)
    {
        Job closestJob = null;
        int bestPriority = int.MaxValue;
        float minDistance = Mathf.Infinity;

        foreach(var deconstructionTask in deconstructJobs.queue.GetJobs())
        {
            if(deconstructionTask.workersInWork >= 1) continue;
            float distance = Vector3.Distance(unitPosition, deconstructionTask.building.transform.position);
            if(deconstructionTask.workPriority < bestPriority)
            {
                bestPriority=deconstructionTask.workPriority;
                minDistance = distance;
                closestJob = deconstructionTask;
            }
            else if(deconstructionTask.workPriority == bestPriority)
            {
                if (distance < minDistance)
                {
                    closestJob = deconstructionTask;
                    minDistance = distance;
                }
            }

        }
        return (closestJob, minDistance);
    }

    bool HasResourcesForBuild (ConstructionSite site)
    {
        foreach (var neededRes in site.resourceCost)
        {
            var collected = site.resourcesCollected.Find(r => r.resourceData == neededRes.resourceData);

            int collectedCount = 0;
            if (collected != null) collectedCount = collected.count;

            if (collectedCount < neededRes.count)
            {
                foreach (var freeRes in haulJobs.queue.GetJobs())
                {
                    if (freeRes.workersInWork >= 1) continue;
                    if (freeRes.worldResource.resourceData == neededRes.resourceData)
                    {
                        return true;
                    }

                }
            }

        }
        return false;
    }
    public void ClearHighlight(Vector3Int position)
    {
        highlight.SetTile(position, null);
    }
}
