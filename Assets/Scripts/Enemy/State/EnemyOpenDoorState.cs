using System.Collections.Generic;
using UnityEngine;

public class EnemyOpenDoorState : EnemyBaseState
{
    float timer;
    float timeToOpenDoor;
    Vector3Int doorPosition;
    EnemyBaseState nexState;
    public EnemyOpenDoorState(WarriorEnemy enemy, EnemyStateMachine stateMachine, float _timeToOpenDoor,Vector3Int _dooorPosition, EnemyBaseState _nextState) : base(enemy, stateMachine)
    {
        timeToOpenDoor = _timeToOpenDoor;
        nexState = _nextState;
        doorPosition = _dooorPosition;
    }

    public override void Enter()
    {
        Debug.Log("Враг начал взламывать дверь!");
    }
    public override void Execute()
    {
        OpeningDoor();
    }
    public override void Exit()
    {

    }

    void OpeningDoor()
    {
        timer += Time.deltaTime;

        if (timer >= timeToOpenDoor)
        {
            PathfindingManager.Instance.Grid.UpdateNodeWalkability(doorPosition, true);

            List<Node> stepInsidePath = new List<Node>();
            stepInsidePath.Add(PathfindingManager.Instance.Grid.NodeFromWorldPoint(doorPosition));

            Vector3Int posBeforeStep = Vector3Int.FloorToInt(enemy.transform.position);

            stateMachine.ChangeState(new EnemyMoveState(enemy, stateMachine, stepInsidePath, enemy.enemy, new EnemyScanState(enemy, stateMachine, new EnemyPathfindingState(enemy, stateMachine), true, posBeforeStep)));

            Debug.Log("Враг взломал дверь, делает шаг внутрь и начинает сканирование!");
        }
    }
}
