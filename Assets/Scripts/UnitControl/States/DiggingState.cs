using UnityEngine;
using static SkeletonWorker;

public class DiggingState : WorkerState
{
    JobManager jobManager;
    Vector3Int currentDiggingTile;
    private float digTimer = 0f;

    public DiggingState(SkeletonWorker worker, JobManager _jobManager, Vector3Int _currentDiggingTile) : base(worker)
    {
        jobManager = _jobManager;
        currentDiggingTile = _currentDiggingTile;
    }

    public override void Enter()
    {

    }

    public override void Execute()
    {
        Digging();
    }

    public override void Exit()
    {

    }

    void Digging()
    {
        if (!jobManager.jobQueues[JobType.Dig].HasJobAt(currentDiggingTile)) { worker.ChangeState(new IdleState(worker)); return; }

        digTimer += Time.deltaTime;

        if (digTimer >= worker.WorkerTimeBetweenDigging)
        {
            DiggingManager.Instance.DamageTile(currentDiggingTile, worker.WorkerDiggingDamage);
            digTimer = 0;
        }
    }
}
