using System.Collections.Generic;
using UnityEngine;

public class HaulJob : Job
{
    public WorldResource worldResource;
    Vector3Int position;
   public HaulJob(WorldResource _resource, int priority) : base(priority)
    {
        worldResource = _resource;
        position = Vector3Int.FloorToInt(worldResource.transform.position);
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
    public override Vector3 GetWorldPosition()
    {
        return position;
    }
    public override int GetPriority()
    {
        return workPriority;
    }

    public override bool StillValid(SkeletonWorker skeletonWorker)
    {
        if (worldResource != null) return true;
        return false;
    }

    public override bool CanExecute()
    {
        if (worldResource.resourceData.isAllowedToHaul && worldResource != null)
        {
            return true;
        }
        return false;
    }

    public override bool IsValid()
    {
        if (worldResource != null) return true;
        return false;
    }
}
