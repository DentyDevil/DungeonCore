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
            stateMachine.ChangeState(nexState);
            Debug.Log($"Враг взламал дверь дверь! и переходит в состояние - {nexState}");
        }
    }
}
