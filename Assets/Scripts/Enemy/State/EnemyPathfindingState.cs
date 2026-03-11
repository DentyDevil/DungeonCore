using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class EnemyPathfindingState : EnemyBaseState
{
    List<Node> path;
   public EnemyPathfindingState(WarriorEnemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
    {

    }

    public override void Enter()
    {
        
    }
    public override void Execute()
    {
        if (enemy.explorationMemory.Count > 0)
        {
            ExplorationTarget door = enemy.explorationMemory.RemoveFirst();
            Vector3Int unitPos = Vector3Int.FloorToInt(enemy.transform.position);
            Vector3Int doorPos = Vector3Int.FloorToInt(door.position);
            if (enemy.visitedCells.Contains(doorPos)) return;
            enemy.visitedCells.Add(doorPos);
            path = PathfindingManager.Instance.EnemyPathfindingInstance.FindPath(unitPos, doorPos, false);
            if (path != null && path.Count > 0)
            {
                path.RemoveAt(path.Count - 1);
                stateMachine.ChangeState(new EnemyMoveState(enemy, stateMachine, path, enemy.enemy, new EnemyOpenDoorState(enemy, stateMachine, enemy.enemy.timeToOpenDoor, doorPos, new EnemyScanState(enemy, stateMachine, new EnemyPathfindingState(enemy, stateMachine)))));
            }
            else { Debug.LogWarning("Опс! пути к двери нет"); }
        }
        else
        {
            path = PathfindingManager.Instance.EnemyPathfindingInstance.FindPath(Vector3Int.FloorToInt(enemy.transform.position), Vector3Int.FloorToInt(DungeonCore.Instance.transform.position), true);
            stateMachine.ChangeState(new EnemyMoveState(enemy, stateMachine, path, enemy.enemy, new EnemyAttackCoreState(enemy, stateMachine)));
            Debug.LogWarning("Дверей не осталось! пора к ядру))");
        }
    }
    public override void Exit()
    {
        
    }
}
