using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class MovingState : WorkerState
{
    private List<Node> path;
    private WorkerState stateAfterMoving;
    private int targetIndex;
    public MovingState(SkeletonWorker worker, List<Node> _path, WorkerState nextState) : base(worker)
    {
        path = _path;
        stateAfterMoving = nextState;
    }

    public override void Enter()
    {

    }

    public override void Execute()
    {
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
