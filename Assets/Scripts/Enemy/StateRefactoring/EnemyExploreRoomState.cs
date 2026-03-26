using System.Collections.Generic;
using UnityEngine;

public class EnemyExploreRoomState : EnemyBaseState
{
    List<Vector3Int> roomTiles;
    List<Vector3Int> doors;
    RoomData roomData;

    int pointsLeft;

    float waitDuration => enemy.enemy.timeToScanRoom;
    float timer;

    public EnemyExploreRoomState(BaseEnemy enemy, EnemyStateMachine stateMachine, RoomData _roomData, int _pointsLeft) : base(enemy, stateMachine)
    {
        roomData = _roomData;
        pointsLeft = _pointsLeft;
        roomTiles = new(roomData.RoomTiles);
        doors = roomData.DoorsInRoom;
    }

    public override void Enter()
    {
        Debug.Log($"Текущее состояние - {stateMachine.CurrentState}");
        SpriteRenderer sprite = enemy.GetComponent<SpriteRenderer>();
        sprite.color = Color.red;
        timer = 0;
        GetAllDoorsInRoom();    
        roomData.IsCleared = true;
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
            Debug.LogWarning($"Посещаю клетку комнаты... Осталось посетить - {pointsLeft}");
            pointsLeft--;
        }
        else
        {
            Debug.LogWarning($"Все клетки комнаты посещены...");
            stateMachine.ChangeState(new EnemyExploreDungeon(enemy, stateMachine));
        }
    }

    void GetAllDoorsInRoom()
    {
        foreach (Vector3Int doorPosInt in doors)
        {
            if (enemy.visitedCells.Contains(doorPosInt)) continue;
            bool isAlreadyInMemory = false;

            foreach (ExplorationTarget doorsInMemory in enemy.explorationMemory.GetItems())
            {
                if (Vector3Int.FloorToInt(doorsInMemory.position) == doorPosInt) { isAlreadyInMemory = true; break; }
            }

            if (!isAlreadyInMemory)
            {
                Vector3 doorPos = doorPosInt + new Vector3(0.5f, 0.5f, 0);
                float distance = Vector3.Distance(enemy.transform.position, doorPos);
                float priority = distance + Random.Range(-0.5f, 0.5f);
                enemy.explorationMemory.Add(new ExplorationTarget { position = doorPos, priority = priority });

                Debug.LogWarning($"дверь - {doorPosInt} приоритет - {priority} добавлена в память исследования");
            }
        }
    }
}
