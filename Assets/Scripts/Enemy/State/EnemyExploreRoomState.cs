using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class EnemyExploreRoomState : EnemyBaseState
{
    List<Node> availableTiles;
    int pointsLeft;
    EnemyBaseState nextState;

    float waitDuration;
    float timer;
    bool isWaiting;
    bool instantStart;

    public EnemyExploreRoomState(WarriorEnemy enemy, EnemyStateMachine stateMachine, List<Node> _availibleTiles, int _pointsLeft, EnemyBaseState _nextState, bool _instantStart = false) : base(enemy, stateMachine)
    {
        availableTiles = _availibleTiles;
        pointsLeft = _pointsLeft;
        nextState = _nextState;
        waitDuration = enemy.enemy.timeToScanRoom;
        
    }

    public override void Enter()
    {
        if(instantStart) timer = waitDuration;
        else timer = 0;

        isWaiting = true;
    }
    public override void Execute()
    {
        if (!isWaiting) return;

        timer += Time.deltaTime;

        if (timer > waitDuration)
        {
            isWaiting = false;
            if (pointsLeft <= 0 || availableTiles == null || availableTiles.Count == 0) stateMachine.ChangeState(nextState);
            else GoToRandomPoint();
        }
    }
    public override void Exit()
    {

    }

    void GoToRandomPoint()
    {
        Node randomTarget = availableTiles[Random.Range(0, availableTiles.Count)];
        availableTiles.Remove(randomTarget);

        Vector3Int targetPos = Vector3Int.FloorToInt(randomTarget.worldPosition);
        Vector3Int unitPos = Vector3Int.FloorToInt(enemy.transform.position);
        if(targetPos == unitPos)
        {
            stateMachine.ChangeState(new EnemyExploreRoomState(enemy, stateMachine, availableTiles, pointsLeft, nextState, true));
            return;
        }

        List<Node> path = PathfindingManager.Instance.EnemyPathfindingInstance.FindPath(unitPos, targetPos, false);

        if (path != null && path.Count > 0)
        {
            stateMachine.ChangeState(new EnemyMoveState(enemy, stateMachine, path, enemy.enemy, new EnemyExploreRoomState(enemy, stateMachine, availableTiles, pointsLeft - 1, nextState, false)));
            
        }
        else
        {
            stateMachine.ChangeState(new EnemyExploreRoomState(enemy, stateMachine, availableTiles, pointsLeft, nextState, true));
        }
    }

   
}
