using UnityEngine;

public abstract class WorkerState
{
    protected SkeletonWorker worker;

    public WorkerState(SkeletonWorker worker)
    {
        this.worker = worker;
    }

    public abstract void Enter();
    public abstract void Execute();
    public abstract void Exit();

}
