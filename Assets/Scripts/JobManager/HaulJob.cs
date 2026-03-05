using System.Collections.Generic;
using UnityEngine;

public class HaulJob : Job
{
   public HaulJob(WorldResource resource, int priority) : base(JobType.Haul, resource, priority)
    {

    }

    public override bool TryStart(SkeletonWorker worker)
    {
        WorldResource drop = worldResource;
        if (drop == null) { return false; }
        List<Node> pathToDrop = worker.pathfinder.FindPath(worker.transform.position, drop.transform.position);
        if (pathToDrop != null)
        {
            workersInWork++;
            worker.ChangeState(new MovingState(worker, pathToDrop, new PickupState(worker, drop, worker.pathfinder, worker.dungeonCore.transform, this, worker.JobManager, new StoreResourceState(worker, drop, worker.dungeonCore))));
            return true;
        }
        else
        {
            worker.JobManager.JobBecomeFree(this, 1);
            return false;
        }
    }
}
