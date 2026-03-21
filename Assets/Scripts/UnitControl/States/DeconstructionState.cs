using UnityEngine;

public class DeconstructionState : WorkerState
{
    Building building;
    public DeconstructionState(SkeletonWorker worker, Building _building) : base(worker)
    {
        building = _building;
    }

    public override void Enter()
    {
       
    }

    public override void Execute()
    {
        Deconstructing();
    }

    public override void Exit()
    {

    }

    void Deconstructing()
    {
        if (building != null)
        {
            if (!JobManager.Instance.jobQueues[JobType.Deconstruct].HasJobAt(Vector3Int.FloorToInt(building.transform.position))) { worker.ChangeState(new IdleState(worker)); return; }
            if (building.Deconstruct())
            {
                worker.ChangeState(new IdleState(worker));
            }
        }
        else
        {
            worker.ChangeState(new IdleState(worker));
        }
    }
}
