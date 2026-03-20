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
        if (building != null)
        {
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

    public override void Exit()
    {

    }
}
