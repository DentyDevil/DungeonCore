using UnityEngine;

public class EnemyInteractDoorState : EnemyBaseState
{
    float delayTime;
    float timer;
    Vector3Int doorPos;
    EnemyBaseState nextState;
    public EnemyInteractDoorState(WarriorEnemy enemy, EnemyStateMachine stateMachine, Vector3Int _doorPos, EnemyBaseState _nextState) : base(enemy, stateMachine)
    {
        delayTime = enemy.enemy.timeToOpenDoor;
        doorPos = _doorPos;
        nextState = _nextState;
    }
    public override void Enter()
    {
        timer = 0;
    }

    public override void Execute()
    {
        timer += Time.deltaTime;

        if (timer >= delayTime)
        {
            Vector3 worldPos = PathfindingManager.Instance.Grid.NodeFromWorldPoint(doorPos).worldPosition;
            Collider2D doorCollider = Physics2D.OverlapPoint(worldPos);
            if (doorCollider != null)
            {
                AutoDoor door = doorCollider.GetComponent<AutoDoor>();
                if (door != null)
                {
                    door.OpenDoor();
                }
            }
            stateMachine.ChangeState(nextState);
        }
    }

    public override void Exit()
    {

    }
}
