using System.Collections.Generic;
using UnityEngine;

public class DeconstructJob : Job
{
    public Building building;
    Vector3Int position;
    public DeconstructJob(Building _building) : base(JobType.Deconstruct)
    {
        building = _building;
        position = Vector3Int.FloorToInt(building.transform.position);
    }

    public override bool TryStart(SkeletonWorker worker)
    {
        Building decunstructBuilding = building;
        if (decunstructBuilding == null) { return false; }
        List<Node> pathToDeconcsruct = worker.pathfinder.FindPath(worker.transform.position, decunstructBuilding.transform.position);
        if (pathToDeconcsruct != null)
        {
            workersInWork++;
            worker.ChangeState(new MovingState(worker, pathToDeconcsruct, new DeconstructionState(worker, building)));
            return true;
        }
        else
        {
            Debug.Log("Пути к постройке которую надо разрушить на данный момент нет!");
            worker.JobManager.JobBecomeFree(this, 1);
            worker.ChangeState(new IdleState(worker));
            return false;
        }
    }
    public override Vector3 GetWorldPosition()
    {
        return position;
    }

    public override bool StillValid(SkeletonWorker skeletonWorker)
    {
        if (building != null) return true;
        return false;
    }

    public override bool CanExecute()
    {
        return true;
    }

    public override bool IsValid()
    {
        if (building != null) return true;
        return false;
    }
}
