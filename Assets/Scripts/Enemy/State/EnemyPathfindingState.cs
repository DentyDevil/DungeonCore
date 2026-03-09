using System.Collections.Generic;
using UnityEngine;

public class EnemyPathfindingState : EnemyBaseState
{
    List<Node> currPath;

   public EnemyPathfindingState(WarriorEnemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
    {

    }

    public override void Enter()
    {
        bool allowDigging;
        currPath = AnalyzePath(out allowDigging);
        if (currPath == null) { Debug.LogWarning("Путь не найден в скрипте EnemyPathfindingState в методе Enter()"); return; }

        if (allowDigging == false) stateMachine.ChangeState(new EnemyMoveState(enemy, stateMachine, currPath, enemy.enemy, new EnemyAttackCoreState(enemy, stateMachine)));
        else 
        {
            if (currPath[0].isWalkable == false) stateMachine.ChangeState(new EnemyDiggingState(enemy, stateMachine, Vector3Int.FloorToInt(currPath[0].worldPosition), this));
            else stateMachine.ChangeState(new EnemyMoveState(enemy, stateMachine, currPath, enemy.enemy, new EnemyAttackCoreState(enemy, stateMachine)));
        }
    }
    public override void Execute()
    {
       

    }
    public override void Exit()
    {
        
    }

    public List<Node> AnalyzePath(out bool allowDigging)
    {
        List<Node> path = PathfindingManager.Instance.EnemyPathfindingInstance.FindPath(Vector3Int.FloorToInt(enemy.transform.position), Vector3Int.FloorToInt(DungeonCore.Instance.transform.position), false);
        if (path != null && path.Count > 0) { allowDigging = false; return path; }
        else if (path == null) { path = PathfindingManager.Instance.EnemyPathfindingInstance.FindPath(Vector3Int.FloorToInt(enemy.transform.position), Vector3Int.FloorToInt(DungeonCore.Instance.transform.position), true); if (path != null && path.Count > 0) { allowDigging = true; return path; } }
        allowDigging = false;
        return null;
    }
}
