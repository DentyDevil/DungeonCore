using UnityEngine;

public class BuildingState : WorkerState
{
    ConstructionSite building;
    public BuildingState(SkeletonWorker worker, ConstructionSite _building) : base(worker)
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
            building.Construct(Time.deltaTime);
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
