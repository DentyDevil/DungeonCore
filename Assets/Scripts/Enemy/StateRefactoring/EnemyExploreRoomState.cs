using System.Collections.Generic;
using UnityEngine;

public class EnemyExploreRoomState : EnemyBaseState
{
    List<Node> roomTiles;

    int pointsLeft;

    float waitDuration => enemy.enemy.timeToScanRoom;
    float timer;

    public EnemyExploreRoomState(BaseEnemy enemy, EnemyStateMachine stateMachine, int _pointsLeft) : base(enemy, stateMachine)
    {
        pointsLeft = _pointsLeft;
        
    }

    public override void Enter()
    {
        roomTiles = new List<Node>(enemy.room);

        timer = 0;
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
        Node randomTarget = roomTiles[Random.Range(0, roomTiles.Count)];
        roomTiles.Remove(randomTarget);

        Vector3Int targetPos = Vector3Int.FloorToInt(randomTarget.worldPosition);

        if (pointsLeft > 0)
        {
            stateMachine.ChangeState(new EnemyMovingState(enemy, stateMachine, targetPos, this));
            Debug.LogWarning($"Посещаю клетку комнаты... Осталось посетить - {pointsLeft}");
            pointsLeft--;
        }
        else
        {
            Debug.LogWarning($"Все клетки комнаты посещены...");
            stateMachine.ChangeState(new EnemyExploreDungeon(enemy, stateMachine));
        }
    }

   
}
