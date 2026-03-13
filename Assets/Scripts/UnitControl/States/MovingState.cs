using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class MovingState : WorkerState
{
    private List<Node> path;
    private WorkerState stateAfterMoving;
    private int targetIndex;
    private Job job;
    private JobManager jobManager;

    bool isOpeningDoor = false;
    float doorTimer = 0;
    float timeToOpenDoor = 1f;
    public MovingState(SkeletonWorker worker, List<Node> _path, WorkerState nextState) : base(worker)
    {
        path = _path;
        stateAfterMoving = nextState;
        job = worker.Job;
        jobManager = worker.JobManager;
    }

    public override void Enter()
    {
        CheckForDoorAndPause();
    }

    public override void Execute()
    {
        if (!job.StillValid(worker))
        {
            jobManager.JobBecomeFree(job, 1);
            worker.DropResource();
            worker.ChangeState(new IdleState(worker));
            return;
        }

        if (isOpeningDoor)
        {
            doorTimer += Time.deltaTime;
            if(doorTimer > timeToOpenDoor)
            {
                isOpeningDoor = false;
                OpenDoorAt(path[targetIndex].worldPosition);
            }
            return;
        }
        if (MoveToTarget(0.1f))
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
            if (!path[targetIndex].isWalkable && !path[targetIndex].isDoor)
            {
                return true;
            }
            Vector3 target = Vector3.MoveTowards(worker.transform.position, path[targetIndex].worldPosition, worker.WorkerSpeed * Time.deltaTime);
            worker.Rb.MovePosition(target);

            if (Vector3.Distance(worker.transform.position, path[targetIndex].worldPosition) <= stopDistance)
            {
                targetIndex++;

                CheckForDoorAndPause();
            }
        }
        else
        {
            return true;
        }
        return false;
    }

    void CheckForDoorAndPause()
    {
        if (targetIndex < path.Count && path[targetIndex].isDoor)
        {
            Collider2D doorCollider = Physics2D.OverlapPoint(path[targetIndex].worldPosition);
            if (doorCollider != null)
            {
                AutoDoor door = doorCollider.GetComponent<AutoDoor>();
                if (door != null && door.isOpen)
                {
                    isOpeningDoor = false;
                    return;
                }
            }
            isOpeningDoor =  true;
            doorTimer = 0;
        }
    }

    void OpenDoorAt(Vector3 pos)
    {
        Collider2D doorCollider = Physics2D.OverlapPoint(pos);
        if(doorCollider != null)
        {
            AutoDoor door = doorCollider.GetComponent<AutoDoor>();
            if(door != null) door.OpenDoor();
        }
    }
}
