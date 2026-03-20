using UnityEngine;

public class EnemyDiggingState : EnemyBaseState
{
    float digTimer;
    float timeBetweenDigging;
    EnemyBaseState nexState;
    int damageTile;
    Vector3Int diggingTile;
    public EnemyDiggingState(BaseEnemy enemy, EnemyStateMachine stateMachine, Vector3Int _diggingTile, EnemyBaseState _nextState) : base(enemy, stateMachine)
    {
        diggingTile = _diggingTile;
        damageTile = enemy.enemy.damageTile;
        timeBetweenDigging = enemy.enemy.timeBetweenDigging;
        nexState = _nextState;
    }

    public override void Enter()
    {

    }
    public override void Execute()
    {
        Digging();
    }
    public override void Exit()
    {
        
    }

    void Digging()
    {
        digTimer += Time.deltaTime;

        if (digTimer >= timeBetweenDigging)
        {
            if (DiggingManager.Instance.DamageTile(diggingTile, damageTile)) stateMachine.ChangeState(nexState);
            else digTimer = 0;
        }
    }
}
