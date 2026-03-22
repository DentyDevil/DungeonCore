using UnityEngine;

public class EnemyPathfindingState : EnemyBaseState
{
   public EnemyPathfindingState(BaseEnemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
    {

    }

    public override void Enter()
    {
        //if (!enemy.isLeader) stateMachine.ChangeState(new EnemyFollowingState(enemy, stateMachine));
    }
    public override void Execute()
    {
        if (enemy.explorationMemory.Count > 0)
        {
            ExplorationTarget door = enemy.explorationMemory.GetFirst();
            Vector3Int unitPos = Vector3Int.FloorToInt(enemy.transform.position);
            Vector3Int doorPos = Vector3Int.FloorToInt(door.position);
            if (enemy.visitedCells.Contains(doorPos)) return;

            enemy.path = PathfindingManager.Instance.EnemyPathfindingInstance.FindPath(unitPos, doorPos, false);
            enemy.currentPathIndex = 0;
            if (enemy.path != null && enemy.path.Count > 0)
            {
                Vector3Int tileBeforeDoor = unitPos;
                if (enemy.path.Count > 1) tileBeforeDoor = Vector3Int.FloorToInt(enemy.path[enemy.path.Count - 2].worldPosition);

                stateMachine.ChangeState(new EnemyMoveState(enemy, stateMachine, enemy.path, enemy.enemy, new EnemyScanState(enemy, stateMachine, new EnemyPathfindingState(enemy, stateMachine), true, tileBeforeDoor)));
            }
            else { Debug.LogWarning("Опс! пути к двери нет"); enemy.explorationMemory.RemoveFirst(); }
        }
        else
        {
            enemy.path = PathfindingManager.Instance.EnemyPathfindingInstance.FindPath(Vector3Int.FloorToInt(enemy.transform.position), Vector3Int.FloorToInt(DungeonCore.Instance.transform.position), true);
            enemy.currentPathIndex = 0;
            stateMachine.ChangeState(new EnemyMoveState(enemy, stateMachine, enemy.path, enemy.enemy, new EnemyAttackCoreState(enemy, stateMachine)));
            Debug.LogWarning("Дверей не осталось! пора к ядру))");
        }
    }
    public override void Exit()
    {
        
    }
}
