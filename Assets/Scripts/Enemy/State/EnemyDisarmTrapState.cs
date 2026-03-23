using UnityEngine;

public class EnemyDisarmTrapState : EnemyBaseState
{
    Vector3Int trapPos;
    float timer = 0;
    float timeToDisarm =>  TrapManager.instance.GetTrapData(trapPos).timeToDisarm;
    EnemyData enemyData => enemy.enemy;
    TrapManager TrapManager => TrapManager.instance;
    public EnemyDisarmTrapState(BaseEnemy enemy, EnemyStateMachine stateMachine, Vector3Int _trapPos) : base(enemy, stateMachine)
    {
        trapPos = _trapPos;
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
                stateMachine.ChangeState(new EnemyIdleState(enemy, stateMachine));
            }
            else
            {
                Debug.LogWarning("Неудача, враг получил урон");
                float trapDamage = TrapManager.GetTrapData(trapPos).damage;
                IDamageable enemyIdmg = enemy.GetComponent<IDamageable>();
                enemyIdmg.TakeDamage(trapDamage);
                if (enemy.healthPoints > 0)
                {
                    stateMachine.ChangeState(new EnemyIdleState(enemy, stateMachine));
                }

            }
            timer = 0;
        }
    }

    
}
