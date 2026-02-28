using System.Collections.Generic;
using UnityEngine;

public class JobManager : MonoBehaviour
{
    [SerializeField] private PathfindingGrid pathfindingGrid;
    [SerializeField] private DungeonCore dungeonCore;
    public Dictionary<Vector3Int, Job> digJobs = new Dictionary<Vector3Int, Job>();
    public Dictionary<Vector3Int, Job> buildJobs = new Dictionary<Vector3Int, Job>();
    public Dictionary<WorldResource, Job> haulJobs = new Dictionary<WorldResource, Job>();
    public HashSet<Vector3Int> unreachebleTasks = new HashSet<Vector3Int>();

    [Header("Priority tasks")]
    public int priorityDigTask = 3;
    public int priorityBuildTask = 3;
    public int priorityHaulTask = 3;

    //Методы для работы с копанием
    public void AddDigJob(Vector3Int cellPos)
    {
        var job = new Job(JobType.Dig, cellPos);
        digJobs.TryAdd(cellPos, job);
    }
    public void RemoveDigJob(Vector3Int cellPos)
    {
        digJobs.Remove(cellPos);
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

        Vector3Int position = Vector3Int.FloorToInt(constructionSite.transform.position);

        buildJobs.TryAdd(position, job);
    }
    public void RemoveBuildJob(ConstructionSite constructionSite)
    {
        Vector3Int position = Vector3Int.FloorToInt(constructionSite.transform.position);
        buildJobs.Remove(position);
    }

    //Методы работы с переноской ресурсов

    public void AddHaulJob(WorldResource drop)
    {
        var job = new Job(JobType.Haul, drop);
        haulJobs.TryAdd(drop, job);
    }
    public void RemoveHaulJob(WorldResource drop)
    {
        haulJobs.Remove(drop);
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

    public WorldResource GetResourceForBuild(Vector3 unitPostion, ResourceData resType)
    {
        float minDistance = Mathf.Infinity;
        WorldResource closestResource = null;
        Job closestJob = null;

        foreach (var haulTask in haulJobs)
        {
            if(haulTask.Value.worldResource.resourceData != resType) continue;
            if (haulTask.Value.workersInWork >= 1) continue;

            float distance = Vector3.Distance(unitPostion, haulTask.Value.worldResource.transform.position);
            if(distance < minDistance)
            {
                minDistance = distance;
                closestResource = haulTask.Value.worldResource;
                closestJob = haulTask.Value;
            }
        }
        if (closestJob != null) closestJob.workersInWork++;
        return closestResource;
    }

    (Job BestJob, float ClosestDistance) TryGetBuildJob(Vector3 unitPosition)
    {
        Job closestJob = null;
        int bestPriority = int.MaxValue;
        float minDistance = Mathf.Infinity;

        foreach (var buildTask in buildJobs)
        {
            if (buildTask.Value.workersInWork >= buildTask.Value.constructionSite.maxWorkers) continue;
            if(!HasResourcesForBuild(buildTask.Value.constructionSite)) continue;

            Vector3 jobPos = buildTask.Key;

            float distance = Vector3.Distance(unitPosition, jobPos);

            if (buildTask.Value.workPriority < bestPriority)
            {
                bestPriority = buildTask.Value.workPriority;
                minDistance = distance;
                closestJob = buildTask.Value;
            }
            else if (buildTask.Value.workPriority == bestPriority)
            {
                if (distance < minDistance)
                {
                    closestJob = buildTask.Value;
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

        List<Vector3Int> taskToDelay = new List<Vector3Int>();

        foreach (var digTask in digJobs)
        {
            if (digTask.Value.workersInWork >= 1) continue;

            Vector3 taskWorldPos = new Vector3(digTask.Key.x + 0.5f, digTask.Key.y + 0.5f, 0);

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
                taskToDelay.Add(digTask.Key);
            }
            float distance = Vector3.Distance(unitPosition, taskWorldPos);

            if (isReachable == false) continue;

            if (digTask.Value.workPriority < bestPriority)
            {
                bestPriority = digTask.Value.workPriority;
                minDistance = distance;
                closestJob = digTask.Value;
            }
            else if (digTask.Value.workPriority == bestPriority)
            {
                if (distance < minDistance)
                {
                    closestJob = digTask.Value;
                    minDistance = distance;
                }
            }


        }
        foreach (Vector3Int _taskToDelay in taskToDelay)
        {
            unreachebleTasks.Add(_taskToDelay);
            digJobs.Remove(_taskToDelay);
        }

        return (closestJob, minDistance);
    }

    (Job BestJob, float ClosestDistance) TryGetHaulJob(Vector3 unitPosition)
    {
        Job closestJob = null;
        int bestPriority = int.MaxValue;
        float minDistance = Mathf.Infinity;

        foreach (var haulTask in haulJobs)
        {
            if (haulTask.Value.workersInWork >= 1) continue;
            float distance = Vector3.Distance(unitPosition, haulTask.Value.worldResource.transform.position);
            if (haulTask.Value.workPriority < bestPriority)
            {
                bestPriority = haulTask.Value.workPriority;
                minDistance = distance;
                closestJob = haulTask.Value;
            }
            else if (haulTask.Value.workPriority == bestPriority)
            {
                if (distance < minDistance)
                {
                    closestJob = haulTask.Value;
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
                foreach (var freeRes in haulJobs)
                {
                    if (freeRes.Value.workersInWork >= 1) continue;
                    if (freeRes.Key.resourceData == neededRes.resourceData)
                    {
                        return true;
                    }

                }
            }

        }
        return false;
    }
}
