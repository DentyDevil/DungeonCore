using UnityEngine;
using System.Collections.Generic;

public class EnemyExploreDungeon : EnemyBaseState
{
    public EnemyExploreDungeon(BaseEnemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
    {

    }
    public override void Enter()
    {
        Debug.Log($"Текущее состояние - {stateMachine.CurrentState}");
        SpriteRenderer sprite = enemy.GetComponent<SpriteRenderer>();
        sprite.color = Color.green;
    }
    public override void Execute()
    {
        if(enemy.explorationMemory.Count > 0)
        {
            Vector3Int doorPos = Vector3Int.FloorToInt(enemy.explorationMemory.GetFirst().position);
            Vector3Int target = Vector3Int.FloorToInt(doorPos);

            Vector3Int behindDoor = GetPositionBehindDoor(doorPos);

            if (behindDoor != doorPos && RoomManager.Instance.tileRoomMap.TryGetValue(behindDoor, out RoomData room))
            {
                stateMachine.ChangeState(new EnemyMovingState(enemy, stateMachine, target, new EnemyExploreRoomState(enemy, stateMachine, room, 3)));
            }
            else
            {
                stateMachine.ChangeState(new EnemyMovingState(enemy, stateMachine, target, new EnemyIdleState(enemy, stateMachine)));
            }
            
        }
        else
        {
            Vector3Int target = Vector3Int.FloorToInt(DungeonCore.Instance.transform.position);
            stateMachine.ChangeState(new EnemyMovingState(enemy, stateMachine, target, new EnemyIdleState(enemy, stateMachine), _isWalkingToCore: true));
        }
    }
    public override void Exit()
    {

    }

    Vector3Int GetPositionBehindDoor(Vector3Int doorPos)
    {
        Node doorNode = PathfindingManager.Instance.Grid.NodeFromWorldPoint(doorPos);

        foreach (Node neighbors in PathfindingManager.Instance.Grid.GetOrthogonalNeighbors(doorNode))
        {
            if (RoomManager.Instance.tileRoomMap.TryGetValue(Vector3Int.FloorToInt(neighbors.worldPosition), out RoomData room))
            {
                if (neighbors.isWalkable && !neighbors.isDoor && !enemy.exploredRooms.Contains(room))
                {
                    return Vector3Int.FloorToInt(neighbors.worldPosition);
                }
            }

        }
        return doorPos;
    }
}
