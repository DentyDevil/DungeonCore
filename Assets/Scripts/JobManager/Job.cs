    using UnityEngine;

[System.Serializable]
public abstract class Job
{
    public JobType jobType;
    public int workersInWork = 0;

    public Job(JobType type)
    {
        jobType = type;
    }

    public int GetPriority()
    {
        return JobManager.Instance.GetLivePriority(jobType);
    }

    public abstract Vector3 GetWorldPosition();
    public abstract bool IsValid();
    public abstract bool StillValid(SkeletonWorker worke);
    public abstract bool CanExecute();
    public abstract bool TryStart(SkeletonWorker worker);
}
