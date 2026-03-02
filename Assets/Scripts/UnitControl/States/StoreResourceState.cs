using UnityEngine;

public class StoreResourceState : WorkerState
{
    WorldResource resource;
    DungeonCore dungeonCore;
    public StoreResourceState(SkeletonWorker worker, WorldResource _resource, DungeonCore _dungeonCore) : base(worker)
    {
        resource = _resource;
        dungeonCore = _dungeonCore;
    }

    public override void Enter()
    {
        if (resource != null)
        {
            Object.Destroy(resource.gameObject);
            dungeonCore.AddBone();
        }
        worker.ChangeState(new IdleState(worker));
    }

    public override void Execute()
    {
       
    }

    public override void Exit()
    {

    }
}
