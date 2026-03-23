using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{
    public EnemyIdleState(BaseEnemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
    {

    }
    public override void Enter()
    {
        stateMachine.ChangeState(new EnemyScanAroundState(enemy, stateMachine));
    }
    public override void Execute()
    {
        
    }
    public override void Exit()
    {

    }
}
