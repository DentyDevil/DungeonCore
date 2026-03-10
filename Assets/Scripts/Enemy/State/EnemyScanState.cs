    using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class EnemyScanState : EnemyBaseState
{
    private float scanDuration = 2.5f;
    private float scanTimer = 0f;

    public EnemyScanState(WarriorEnemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
    {

    }

    public override void Enter()
    {
        scanTimer = 0f;
    }
    public override void Execute()
    {
        scanTimer += Time.deltaTime;
        if (scanTimer >= scanDuration)
        {
            while (enemy.explorationMemory.Count > 0)
            {
                ExplorationTarget best = enemy.explorationMemory.RemoveFirst();
                if(enemy.visitedCells.Contains(best.position)) continue;

                List<Node> path = PathfindingManager.Instance.EnemyPathfindingInstance.FindPath(Vector3Int.FloorToInt(enemy.transform.position), best.position, false);

                if (path != null && path.Count > 0)
                {
                    stateMachine.ChangeState(new EnemyMoveState(enemy, stateMachine, path, enemy.enemy, this));
                    return;
                }
            }
            stateMachine.ChangeState(new EnemyPathfindingState(enemy, stateMachine));
        }

    }
    public override void Exit()
    {

    }
}
