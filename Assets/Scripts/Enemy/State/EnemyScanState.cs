using System.Collections.Generic;
using UnityEngine;

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
        ScanForChokepoints();
        Debug.Log($"{enemy.name} осматриваетс€ в комнате...");
    }
    public override void Execute()
    {
        scanTimer += Time.deltaTime;
        if (scanTimer >= scanDuration)
        {
            if (enemy.explorationMemory.Count > 0)
            {
                ExplorationTarget best = enemy.explorationMemory.RemoveFirst();
                Vector3Int targetPos = best.position;

                List<Node> path = PathfindingManager.Instance.EnemyPathfindingInstance.FindPath(Vector3Int.FloorToInt(enemy.transform.position), targetPos, false);

                if (path != null && path.Count > 0)
                {
                    stateMachine.ChangeState(new EnemyMoveState(enemy, stateMachine, path, enemy.enemy, this));
                }
                else Debug.Log("ѕуть к chokepoint не найден, пробуем другой...");
            }
            else stateMachine.ChangeState(new EnemyPathfindingState(enemy, stateMachine));
        }
        
    }
    public override void Exit()
    {

    }

    void ScanForChokepoints()
    {
        List<Node> area = PathfindingManager.Instance.Grid.GetNodesInRadius(PathfindingManager.Instance.Grid.NodeFromWorldPoint(enemy.transform.position), 5);

        foreach (var node in area)
        {
            if (!node.isWalkable) continue;

            Vector3Int pos = Vector3Int.FloorToInt(node.worldPosition);

            if (PathfindingManager.Instance.Grid.IsChokepoint(node))
            {
                float distance = Vector3.Distance(enemy.transform.position, node.worldPosition);
                float priority = distance + Random.Range(-0.5f, 0.5f);

                enemy.explorationMemory.Add(new ExplorationTarget { position = pos, priority = priority });

                Debug.Log($"Ќайден chokepoint на {pos}, приоритет {priority}");
            }
    }
}
}
