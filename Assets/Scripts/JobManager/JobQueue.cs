using System.Collections.Generic;
using UnityEngine;

public class JobQueue<T> where T : Job
{
    private HashSet<T> jobs;

    public JobQueue(int capacity = 32)
    {
        jobs = new HashSet<T>(capacity);
    }

    public void Add(T job)
    {
        jobs.Add(job);
    }
    public void Remove(T job)
    {
        if (jobs.Contains(job)) jobs.Remove(job);
        else Debug.Log("Отсутствует задача на удаление!");
    }
    public void CleanUp()
    {
        jobs.RemoveWhere(x => x.IsValid() == false);
    }

    public bool TryGetBestFor(Vector3 unitPosition, out float bestDistance, out T bestJob, HashSet<T> unreachebleTasks)
    {
        bestJob = null;
        bestDistance = Mathf.Infinity;
        int bestPriority = int.MaxValue;

        foreach (T job in jobs)
        {
            if (job.workersInWork >= 1) continue;
            if (job.CanExecute() == false) continue;
            if(unreachebleTasks.Contains(job)) continue;
            float distance = (unitPosition - job.GetWorldPosition()).sqrMagnitude;
            if (job.GetPriority() < bestPriority)
            {
                bestPriority = job.GetPriority();
                bestDistance = distance;
                bestJob = job;
            }
            else if (job.GetPriority() == bestPriority)
            {
                if (distance < bestDistance)
                {
                    bestDistance = distance; 
                    bestJob = job;
                }

            }
        }
        return bestJob != null;
    }

    public IEnumerable<T> GetJobs()
    {
        return jobs;
    }

    public bool HasJobAt(Vector3Int pos)
    {
        foreach (T job in jobs)
        {
            Vector3Int curPos = Vector3Int.FloorToInt(job.GetWorldPosition());
            if(curPos == pos)
            {
                return true;
            }
        }
        return false;
    }

    public T GetJobAt(Vector3Int pos)
    {
        foreach (T job in jobs)
        {
            Vector3Int curPos = Vector3Int.FloorToInt(job.GetWorldPosition());
            if (curPos == pos)
            {
                return job;
            }
        }
        return null;
    }

}
