using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PickupState : WorkerState
{
    WorldResource drop;
    WorkerState nextStateAfterPickUp;
    Pathfinding pathfinding;
    Transform targetDestination;
    JobManager jobManager;
    Job job;
    public PickupState(SkeletonWorker worker, WorldResource _drop, Pathfinding _pathfinding, Transform _targetDestination, Job _job, JobManager _jobManager, WorkerState _nextStateAfterPickUp) : base(worker)
    {
        drop = _drop;  
        pathfinding = _pathfinding;
        targetDestination = _targetDestination;
        nextStateAfterPickUp = _nextStateAfterPickUp;
        job = _job;
        jobManager = _jobManager;
    }

    public override void Enter()
    {
        if (drop != null)
        {
            List<Node> path = pathfinding.FindPath(worker.transform.position, targetDestination.position);
            if (path != null)
            {
                drop.transform.SetParent(worker.transform);
                worker.ChangeState(new MovingState(worker, path, nextStateAfterPickUp));
            }
            else
            {
                worker.unreachableJobs.Add(job);

                jobManager.JobBecomeFree(job, 1);
                worker.ChangeState(new IdleState(worker));
            }
        }
        else
        {
            jobManager.JobBecomeFree(job, 1);
            worker.ChangeState(new IdleState(worker));
        }
    }

    public override void Execute()
    {
        
    }

    public override void Exit()
    {

    }
}
