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

            Vector3Int behindDoor = Vector3Int.FloorToInt(GetPositionBehindDoor(doorPos));

            if (RoomManager.Instance.tileRoomMap.TryGetValue(behindDoor, out RoomData room))
            {
                stateMachine.ChangeState(new EnemyMovingState(enemy, stateMachine, target, new EnemyExploreRoomState(enemy, stateMachine, room, 3)));
            }
            else
            {
                stateMachine.ChangeState(new EnemyIdleState(enemy, stateMachine));
            }
            
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

    Vector3 GetPositionBehindDoor(Vector3 doorPos)
    {
        Node doorNode = PathfindingManager.Instance.Grid.NodeFromWorldPoint(doorPos);
        if (doorNode == null || !doorNode.isDoor) return doorPos;

        foreach (Node neighbors in PathfindingManager.Instance.Grid.GetOrthogonalNeighbors(doorNode))
        {
            if(neighbors.isWalkable && !neighbors.isDoor && Vector3Int.FloorToInt(neighbors.worldPosition) != Vector3Int.FloorToInt(enemy.transform.position))
            {
               return neighbors.worldPosition;
            }
        }
        return doorPos;
    }
}
