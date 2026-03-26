using UnityEngine;

public class EnemyDisarmTrapState : EnemyBaseState
{
    Vector3Int trapPos;
    float timer = 0;
    float timeToDisarm =>  TrapManager.instance.GetTrapData(trapPos).timeToDisarm;
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

    }
    public override void Execute()
    {
        DisarmTrap();
    }
    public override void Exit()
    {

    }

    void DisarmTrap()
    {
        timer += Time.deltaTime;
        if (timer >= timeToDisarm)
        {
            if (TrapManager.TryDisarm(trapPos, enemyData))
            {
                Debug.LogWarning("Ловушка успешно обезврежена");
                stateMachine.ChangeState(new EnemyMovingState(enemy, stateMachine, target, returnState, _isWalkingToCore: isWalkingToCoreReturData));
            }
            else
            {
                Debug.LogWarning("Неудача, враг получил урон");
                float trapDamage = TrapManager.GetTrapData(trapPos).damage;
                IDamageable enemyIdmg = enemy.GetComponent<IDamageable>();
                enemyIdmg.TakeDamage(trapDamage);
                if (enemy.healthPoints > 0)
                {
                    stateMachine.ChangeState(new EnemyMovingState(enemy, stateMachine, target, returnState, _isWalkingToCore: isWalkingToCoreReturData));
                }

            }
            timer = 0;
        }
    }

    
}
