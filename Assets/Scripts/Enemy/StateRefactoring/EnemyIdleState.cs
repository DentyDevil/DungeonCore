using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{
    public EnemyIdleState(BaseEnemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
    {

    }
    public override void Enter()
    {
        SpriteRenderer sprite = enemy.GetComponent<SpriteRenderer>();
        sprite.color = Color.darkOrange;
        Debug.Log($"Текущее состояние - {stateMachine.CurrentState}");
        stateMachine.ChangeState(new EnemyScanAroundState(enemy, stateMachine));
    }
    public override void Execute()
    {
        
    }
    public override void Exit()
    {

    }
}
