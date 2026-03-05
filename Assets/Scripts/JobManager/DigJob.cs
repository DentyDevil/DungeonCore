using System.Collections.Generic;
using UnityEngine;

public class DigJob : Job
{
    public DigJob(Vector3Int pos, int priority) : base(JobType.Dig, pos, priority)
    {
       
    }
    public override bool TryStart(SkeletonWorker worker)
    {
        List<Node> path = worker.Pathfinding.FindPath(worker.transform.position, GetWorldPosition());
        if(path == null) return false;

        workersInWork++;
        worker.ChangeState(new MovingState(worker, path, new DiggingState(worker, worker.JobManager, position, worker.inputManager)));
        return true;
    }
}
        