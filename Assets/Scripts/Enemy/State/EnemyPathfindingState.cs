using System.Collections.Generic;
using UnityEngine;

public class EnemyPathfindingState : EnemyBaseState
{
    List<Node> path;
    float timer;
    float howOftenCheckPath = 1;
   public EnemyPathfindingState(BaseEnemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
    {

    }

    public override void Enter()
    {
        path = AnalyzePath();
    }
    public override void Execute()
    {
        timer += Time.deltaTime;

        if(timer >= howOftenCheckPath)
        {
            path = AnalyzePath();
            timer = 0;
        }
        if (path == null || path.Count <= 0) return;

        if (path[0].isWalkable)
        {
            stateMachine.ChangeState(new EnemyMoveState(enemy, stateMachine, path, enemy.enemy, new EnemyAttackCoreState(enemy, stateMachine)));
        }
        else
        {
            Vector3Int cellPos = Vector3Int.FloorToInt(path[0].worldPosition);
            stateMachine.ChangeState(new EnemyDiggingState(enemy, stateMachine, cellPos, this));
        }

    }
    public override void Exit()
    {
        
    }

    public List<Node> AnalyzePath()
    {
        List<Node> path = PathfindingManager.Instance.EnemyPathfindingInstance.FindPath(enemy.transform.position, DungeonCore.Instance.transform.position);
        if (path == null || path.Count <= 0) return null;

        return path;
    }
}
