using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EnemyMoveState : EnemyBaseState
{
    EnemyData enemyData;
    private int targetIndex;
    List<Node> path;
    EnemyBaseState nextStateAfterMoving;

    public EnemyMoveState(WarriorEnemy enemy, EnemyStateMachine stateMachine, List<Node> _path, EnemyData _enemyData , EnemyBaseState nextState) : base(enemy, stateMachine)
    {
        path = _path;
        enemyData = _enemyData;
        nextStateAfterMoving = nextState;
    }

    public override void Enter()
    {

    }
    public override void Execute()
    {

        if (MoveToTarget(0.1f))
        {
            stateMachine.ChangeState(nextStateAfterMoving);
        }
    }
    public override void Exit()
    {

    }

    private bool MoveToTarget(float stopDistance)
    {
        if (targetIndex < path.Count)
        {
            Vector3 target = Vector3.MoveTowards(enemy.transform.position, path[targetIndex].worldPosition, enemyData.speed * Time.deltaTime);
            enemy.Rb.MovePosition(target);

            if (Vector3.Distance(enemy.transform.position, path[targetIndex].worldPosition) <= stopDistance)
            {
                targetIndex++;
                
                if (targetIndex < path.Count && path[targetIndex].isWalkable == false && path[targetIndex].isDoor == false) { stateMachine.ChangeState(new EnemyDiggingState(enemy, stateMachine, Vector3Int.FloorToInt(path[targetIndex].worldPosition), new EnemyPathfindingState(enemy, stateMachine))); return false; }
            }
        }
        else
        {
            return true;
        }
        return false;
    }
}
