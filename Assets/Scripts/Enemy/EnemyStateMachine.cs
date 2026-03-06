using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    public EnemyBaseState CurrentState {  get; private set; }

    public void Initialize(EnemyBaseState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    private void Update()
    {
        CurrentState?.Execute();
    }

    public void ChangeState(EnemyBaseState newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();

    }
}
