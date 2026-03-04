using UnityEngine;
using static SkeletonWorker;

public class DiggingState : WorkerState
{
    JobManager jobManager;
    InputManager inputManager;
    Vector3Int currentDiggingTile;
    private float digTimer = 0f;

    public DiggingState(SkeletonWorker worker, JobManager _jobManager, Vector3Int _currentDiggingTile, InputManager _inputManager) : base(worker)
    {
        jobManager = _jobManager;
        currentDiggingTile = _currentDiggingTile;
        inputManager = _inputManager;
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
        if (!jobManager.digJobs.queue.HasJobAt(currentDiggingTile)) { worker.ChangeState(new IdleState(worker)); return; }

        digTimer += Time.deltaTime;

        if (digTimer >= worker.WorkerTimeBetweenDigging)
        {
            inputManager.DamageTile(currentDiggingTile, worker.WorkerDiggingDamage);
            digTimer = 0;
        }
    }
}
