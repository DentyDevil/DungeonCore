using UnityEngine;

[System.Serializable]
public abstract class Job
{
    public int workPriority;
    public int workersInWork = 0;

    public Job(int priority)
    {
        workPriority = priority;
    }

    public abstract Vector3 GetWorldPosition();
    public abstract int GetPriority();
    public abstract bool IsValid();
    public abstract bool StillValid(SkeletonWorker worke);
    public abstract bool CanExecute();
    public abstract bool TryStart(SkeletonWorker worker);
}
