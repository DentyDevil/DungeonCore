using UnityEngine;

public class IdleState : WorkerState
{
    float checkNewJobTimer = 0.5f;
    float timer = 0;

    public IdleState(SkeletonWorker worker) : base(worker)
    {

    }

    public override void Enter()
    {

    }

    public override void Execute()
    {
        timer += Time.deltaTime;
        if (timer >= checkNewJobTimer)
        {
            worker.GetAnyJob();
            timer = 0;
        }
    }

    public override void Exit()
    {

    }
}
