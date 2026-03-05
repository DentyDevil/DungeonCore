using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class DeconstructJob : Job
{
    public DeconstructJob(Building building, int priority) : base(JobType.Deconstruct, building, priority)
    {

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
}
