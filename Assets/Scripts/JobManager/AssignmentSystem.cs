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


    public int priorityDigTask;
    public int priorityBuildTask;
    public int priorityHaulTask;
    public int priorityDeconstructTask;

    public AssignmentSystem(HaulJobQueueMB _haulJobs, DigJobQueueMB _digJobs, BuildJobQueueMB _buildJobs, DeconstructJobQueueMB _deconstructJobs, HashSet<Job> _unreachebleTasks, int _priorityDigTask, int _priorityBuildTask,int _priorityHaulTask, int _priorityDeconstructTask)
    {
        haulJobs = _haulJobs;
        digJobs = _digJobs;
        buildJobs = _buildJobs;
        deconstructJobs = _deconstructJobs;

        unreachebleTasks = _unreachebleTasks;

        priorityDigTask = _priorityDigTask;
        priorityBuildTask = _priorityBuildTask;
        priorityHaulTask = _priorityHaulTask;
        priorityDeconstructTask = _priorityDeconstructTask;
    }

    public Job GetBestJob(Vector3 workerPosition)
    {
        Job closestJob = null;
        int bestPriority = int.MaxValue;
        float minDistance = Mathf.Infinity;

        List<(Job job, float dist, int priority)> candidates = new List<(Job job, float dist, int priority)>();

        if (haulJobs.queue.TryGetBestFor(workerPosition, out float distance, out Job haulJob, unreachebleTasks))
            candidates.Add((haulJob, distance, priorityHaulTask));
        if(buildJobs.queue.TryGetBestFor(workerPosition, out distance, out Job buildJob, unreachebleTasks))
            candidates.Add((buildJob, distance, priorityBuildTask));
        if (deconstructJobs.queue.TryGetBestFor(workerPosition, out distance, out Job deconstructJob, unreachebleTasks))
            candidates.Add((deconstructJob, distance, priorityDeconstructTask));
        if(digJobs.queue.TryGetBestFor(workerPosition, out distance, out Job digJob, unreachebleTasks))
            candidates.Add((digJob, distance, priorityDigTask));

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
