using System.Collections.Generic;
using UnityEngine;

public class DigJob : Job
{
    Vector3Int position;
    public DigJob(Vector3Int pos) : base(JobType.Dig)
    {
       position = pos;
    }
    public override bool TryStart(SkeletonWorker worker)
    {
        List<Node> path = worker.Pathfinding.FindPath(worker.transform.position, GetWorldPosition());
        if(path == null) return false;

        workersInWork++;
        worker.ChangeState(new MovingState(worker, path, new DiggingState(worker, worker.JobManager, position)));
        return true;
    }

    public override Vector3 GetWorldPosition()
    {
        return position;
    }

    public override bool StillValid(SkeletonWorker skeletonWorker)
    {
        return skeletonWorker.JobManager.jobQueues[JobType.Dig].HasJobAt(position);
    }

    public override bool CanExecute()
    {
        return true;
    }

    public override bool IsValid()
    {
        return true;
    }
}
        