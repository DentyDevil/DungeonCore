using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AssignmentSystem 
{

    public JobManager jobManager;

    public AssignmentSystem(JobManager _jobManager)
    {
        jobManager = _jobManager;
    }

    public Job GetBestJob(SkeletonWorker worker)
    {
        Job closestJob = null;
        int bestPriority = int.MaxValue;
        float minDistance = Mathf.Infinity;

        foreach (var queue in jobManager.jobQueues.Values)
        {
            if (queue.TryGetBestFor(worker.transform.position, out float distance, out Job job, worker.unreachableJobs))
            {
                int priority = job.GetPriority();

                if (priority < bestPriority)
                {
                    bestPriority = priority;
                    minDistance = distance;
                    closestJob = job;
                }
                else if (priority == bestPriority)
                {
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestJob = job;
                    }
                }
            }
        }
        return closestJob;
    }

}
