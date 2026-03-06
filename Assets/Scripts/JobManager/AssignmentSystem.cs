using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AssignmentSystem 
{
    public HaulJobQueueMB haulJobs;
    public DigJobQueueMB digJobs;
    public BuildJobQueueMB buildJobs;
    public DeconstructJobQueueMB deconstructJobs;

    public HashSet<Job> unreachebleTasks;

    public JobManager jobManager;

    public AssignmentSystem(HaulJobQueueMB _haulJobs, DigJobQueueMB _digJobs, BuildJobQueueMB _buildJobs, DeconstructJobQueueMB _deconstructJobs, HashSet<Job> _unreachebleTasks, JobManager _jobManager)
    {
        haulJobs = _haulJobs;
        digJobs = _digJobs;
        buildJobs = _buildJobs;
        deconstructJobs = _deconstructJobs;

        unreachebleTasks = _unreachebleTasks;
        jobManager = _jobManager;
    }

    public Job GetBestJob(Vector3 workerPosition)
    {
        Job closestJob = null;
        int bestPriority = int.MaxValue;
        float minDistance = Mathf.Infinity;

        List<(Job job, float dist, int priority)> candidates = new List<(Job job, float dist, int priority)>();

        if (haulJobs.queue.TryGetBestFor(workerPosition, out float distance, out Job haulJob, unreachebleTasks))
            candidates.Add((haulJob, distance, jobManager.priorityHaulTask));
        if(buildJobs.queue.TryGetBestFor(workerPosition, out distance, out Job buildJob, unreachebleTasks))
            candidates.Add((buildJob, distance, jobManager.priorityBuildTask));
        if (deconstructJobs.queue.TryGetBestFor(workerPosition, out distance, out Job deconstructJob, unreachebleTasks))
            candidates.Add((deconstructJob, distance, jobManager.priorityDeconstructTask));
        if(digJobs.queue.TryGetBestFor(workerPosition, out distance, out Job digJob, unreachebleTasks))
            candidates.Add((digJob, distance, jobManager.priorityDigTask));

        foreach (var cand in candidates)
        {
            int priority = cand.priority;
            float currDistance = cand.dist;

            if (priority < bestPriority)
            {
                bestPriority = priority;
                minDistance = currDistance;
                closestJob = cand.job;
            }
            else if (priority == bestPriority)
            {
                if (currDistance < minDistance)
                {
                    minDistance = currDistance;
                    closestJob = cand.job;
                }
            }
        }
        return closestJob;
    }

}
