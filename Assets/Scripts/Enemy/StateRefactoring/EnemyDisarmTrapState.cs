using UnityEngine;

public class EnemyDisarmTrapState : EnemyBaseState
{
    Vector3Int trapPos;
    float timer = 0;
    float timeToDisarm;
    bool isTrapDisarmed = false;
    EnemyData enemyData => enemy.enemy;
    TrapManager TrapManager => TrapManager.instance;

    EnemyBaseState returnState;
    Vector3Int target;
    bool isWalkingToCoreReturData;
    public EnemyDisarmTrapState(BaseEnemy enemy, EnemyStateMachine stateMachine, Vector3Int _trapPos, Vector3Int target, bool isWalkingToCoreReturData, EnemyBaseState returnState) : base(enemy, stateMachine)
    {
        trapPos = _trapPos;
        this.returnState = returnState;
        this.target = target;
        this.isWalkingToCoreReturData = isWalkingToCoreReturData;
    }

    public override void Enter()
    {
        timeToDisarm = TrapManager.instance.GetTrapData(trapPos).timeToDisarm;
        TrapsEvents.OnTrapDisarmed += HandleOnTrapDisarmed;
    }

    private void HandleOnTrapDisarmed(Vector3Int trapPos)
    {
        if (this.trapPos == trapPos) isTrapDisarmed = true;
    }

    public override void Execute()
    {
        DisarmTrap();
    }
    public override void Exit()
    {
        TrapsEvents.OnTrapDisarmed -= HandleOnTrapDisarmed;
    }

    void DisarmTrap()
    {
        if (!isTrapDisarmed)
        {
            timer += Time.deltaTime;
            if (timer >= timeToDisarm)
            {
                if (TrapManager.TryDisarm(trapPos, enemyData))
                {
                    isTrapDisarmed = true;
                    Debug.LogWarning("Ловушка успешно обезврежена");
                    stateMachine.ChangeState(new EnemyMovingState(enemy, stateMachine, target, returnState, _isWalkingToCore: isWalkingToCoreReturData));
                }
                else
                {
                    Debug.LogWarning("Неудача, враг получил урон");
                    float trapDamage = TrapManager.GetTrapData(trapPos).damage;
                    IDamageable enemyIdmg = enemy.GetComponent<IDamageable>();
                    enemyIdmg.TakeDamage(trapDamage);
                    TrapManager.UpdateStateTrap(trapPos);
                    if (enemy.healthPoints > 0)
                    {
                        stateMachine.ChangeState(new EnemyMovingState(enemy, stateMachine, target, returnState, _isWalkingToCore: isWalkingToCoreReturData));
                    }

                }
                timer = 0;
            }
        }
        else
        {
            stateMachine.ChangeState(new EnemyMovingState(enemy, stateMachine, target, returnState, _isWalkingToCore: isWalkingToCoreReturData));
        }
    }

    
}
