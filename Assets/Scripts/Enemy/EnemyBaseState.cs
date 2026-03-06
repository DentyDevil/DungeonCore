using UnityEngine;

public abstract class EnemyBaseState 
{
    protected BaseEnemy enemy;
    protected EnemyStateMachine stateMachine;

    public EnemyBaseState(BaseEnemy enemy, EnemyStateMachine stateMachine)
    {
        this.enemy = enemy;
        this.stateMachine = stateMachine;
    }

    public abstract void Enter();
    public abstract void Execute();
    public abstract void Exit();
}
