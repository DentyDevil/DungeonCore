using UnityEngine;

public class EnemyExploreDungeon : EnemyBaseState
{
    public EnemyExploreDungeon(BaseEnemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
    {

    }
    public override void Enter()
    {
        
    }
    public override void Execute()
    {
        if(enemy.explorationMemory.Count > 0)
        {
            Vector3Int target = Vector3Int.FloorToInt(enemy.explorationMemory.GetFirst().position);
            stateMachine.ChangeState(new EnemyMovingState(enemy, stateMachine, target, new EnemyIdleState(enemy, stateMachine), 0f));
        }
        else
        {
            Vector3Int target = Vector3Int.FloorToInt(DungeonCore.Instance.transform.position);
            stateMachine.ChangeState(new EnemyMovingState(enemy, stateMachine, target, new EnemyIdleState(enemy, stateMachine), 0f));
        }
    }
    public override void Exit()
    {

    }
}
