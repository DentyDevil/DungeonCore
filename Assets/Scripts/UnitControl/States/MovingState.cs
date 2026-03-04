using System.Collections.Generic;
using UnityEngine;

public class MovingState : WorkerState
{
    private List<Node> path;
    private WorkerState stateAfterMoving;
    private int targetIndex;
    private Job job;
    private JobManager jobManager;
    public MovingState(SkeletonWorker worker, List<Node> _path, WorkerState nextState) : base(worker)
    {
        path = _path;
        stateAfterMoving = nextState;
        job = worker.Job;
        jobManager = worker.JobManager;
    }

    public override void Enter()
    {

    }

    public override void Execute()
    {
        switch (job.jobType)
        {
            case JobType.Dig:
                if (!jobManager.digJobs.queue.HasJobAt(job.position))
                {
                    jobManager.JobBecomeFree(job, 1);
                    worker.ChangeState(new IdleState(worker));
                    return;
                }
                break;
            case JobType.Build:
                if (job.constructionSite == null)
                {
                    jobManager.JobBecomeFree(job, 1);
                    worker.DropResource();
                    worker.ChangeState(new IdleState(worker));
                    return;
                }
                break;
            case JobType.Haul:
                if (job.worldResource == null)
                {
                    jobManager.JobBecomeFree(job, 1);
                    worker.ChangeState(new IdleState(worker));
                    return;
                }
                break;
            case JobType.Deconstruct:
                if(job.building == null)
                {
                    jobManager.JobBecomeFree(job, 1);
                    worker.ChangeState(new IdleState(worker));
                    return;
                }
                break;
            default:
                break;
        }

        if (MoveToTarget(1))
        {
            worker.ChangeState(stateAfterMoving);
        }
    }

    public override void Exit()
    {

    }

    private bool MoveToTarget(float stopDistance)
    {
        if (targetIndex < path.Count)
        {
            Vector3 target = Vector3.MoveTowards(worker.transform.position, path[targetIndex].worldPosition, worker.WorkerSpeed * Time.deltaTime);
            worker.Rb.MovePosition(target);

            if (Vector3.Distance(worker.transform.position, path[targetIndex].worldPosition) <= stopDistance)
            {
                targetIndex++;
            }
        }
        else
        {
            return true;
        }
        return false;
    }
}
