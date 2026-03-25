using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyExploreRoomState : EnemyBaseState
{
    List<Vector3Int> roomTiles;
    List<Vector3Int> doors;

    int pointsLeft;

    float waitDuration => enemy.enemy.timeToScanRoom;
    float timer;

    public EnemyExploreRoomState(BaseEnemy enemy, EnemyStateMachine stateMachine, RoomData roomData, int _pointsLeft) : base(enemy, stateMachine)
    {
        pointsLeft = _pointsLeft;
        roomTiles = new(roomData.RoomTiles);
        doors = roomData.DoorsInRoom;
    }

    public override void Enter()
    {
        timer = 0;
        GetAllDoorsInRoom();    
    }
    public override void Execute()
    {
        timer += Time.deltaTime;
        if (timer >= waitDuration)
        {
            if (roomTiles != null && roomTiles.Count > 0)
            {
                GoToRandomPoint();
                timer = 0;
            }
            else
            {
                stateMachine.ChangeState(new EnemyExploreDungeon(enemy, stateMachine));
            }
        }
    }
    public override void Exit()
    {

    }

    void GoToRandomPoint()
    {
        Vector3Int randomTarget = roomTiles[Random.Range(0, roomTiles.Count)];
        roomTiles.Remove(randomTarget);


        if (pointsLeft > 0)
        {
            stateMachine.ChangeState(new EnemyMovingState(enemy, stateMachine, randomTarget, this));
            Debug.LogWarning($"ѕосещаю клетку комнаты... ќсталось посетить - {pointsLeft}");
            pointsLeft--;
        }
        else
        {
            Debug.LogWarning($"¬се клетки комнаты посещены...");
            stateMachine.ChangeState(new EnemyExploreDungeon(enemy, stateMachine));
        }
    }

    void GetAllDoorsInRoom()
    {
        foreach (Vector3Int doorPosInt in doors)
        {
            if (!enemy.visitedCells.Contains(doorPosInt))
            {
                float priority = Random.Range(-0.5f, 0.5f);
                ExplorationTarget newDoor = new ExplorationTarget { position = doorPosInt + new Vector3(0.5f, 0.5f, 0), priority = priority };
                enemy.explorationMemory.Add(newDoor);
                Debug.LogWarning($"дверь - {doorPosInt} приоритет - {priority} добавлена в пам€ть исследовани€");
            }
        }
    }
}
