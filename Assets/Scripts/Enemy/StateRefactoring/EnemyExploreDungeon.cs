using UnityEngine;
using System.Collections.Generic;

public class EnemyExploreDungeon : EnemyBaseState
{
    public EnemyExploreDungeon(BaseEnemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
    {

    }
    public override void Enter()
    {
        
    }
    public override void Execute()
    {
        if(enemy.explorationMemory.Count > 0)
        {
            Vector3 doorPos = enemy.explorationMemory.GetFirst().position;
            Vector3Int target = Vector3Int.FloorToInt(doorPos);

            Vector3Int behindDoor = Vector3Int.FloorToInt(GetPositionBehindDoor(doorPos, enemy.room));
           

            
            stateMachine.ChangeState(new EnemyMovingState(enemy, stateMachine, target, new EnemyScanAroundState(enemy,stateMachine, true, behindDoor, new EnemyExploreRoomState(enemy, stateMachine, 3))));
        }
        else
        {
            Vector3Int target = Vector3Int.FloorToInt(DungeonCore.Instance.transform.position);
            stateMachine.ChangeState(new EnemyMovingState(enemy, stateMachine, target, new EnemyIdleState(enemy, stateMachine)));
        }
    }
    public override void Exit()
    {

    }

    Vector3 GetPositionBehindDoor(Vector3 doorPos, List<Node> currentRoom)
    {
        Node doorNode = PathfindingManager.Instance.Grid.NodeFromWorldPoint(doorPos);
        if (doorNode == null || !doorNode.isDoor) return doorPos;

        foreach (Node neighbors in PathfindingManager.Instance.Grid.GetOrthogonalNeighbors(doorNode))
        {
            if(neighbors.isWalkable && !neighbors.isDoor)
            {
                if(currentRoom != null && !currentRoom.Contains(neighbors))
                {
                    return neighbors.worldPosition;
                }
                else if(currentRoom == null)
                {
                    return neighbors.worldPosition;
                }
            }
        }
        return doorPos;
    }
}
