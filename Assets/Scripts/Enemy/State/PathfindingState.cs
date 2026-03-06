using System.Collections.Generic;
using UnityEngine;

public class PathfindingState : EnemyBaseState
{

   public PathfindingState(BaseEnemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
    {

    }

    public override void Enter()
    {
        
    }
    public override void Execute()
    {
        
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
