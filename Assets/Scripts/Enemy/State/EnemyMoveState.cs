using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EnemyMoveState : EnemyBaseState
{
    float howOftenCheckPath = 1f;
    float timer;
    EnemyData enemyData;
    private int targetIndex;
    List<Node> path;
    EnemyBaseState nextStateAfterMoving;

    public EnemyMoveState(BaseEnemy enemy, EnemyStateMachine stateMachine, List<Node> _path, EnemyData _enemyData , EnemyBaseState nextState) : base(enemy, stateMachine)
    {
        path = _path;
        enemyData = _enemyData;
        nextStateAfterMoving = nextState;
    }

    public override void Enter()
    {

    }
    public override void Execute()
    {

        if (MoveToTarget(1))
        {
            stateMachine.ChangeState(nextStateAfterMoving);
        }
        timer += Time.deltaTime;
        int a = path.Count;
        if (timer >= howOftenCheckPath) { path = AnalyzePath(); targetIndex = 1; timer = 0; Debug.Log($"Построен новый путь! был - {a} стал - {path.Count}"); }

    }
    public override void Exit()
    {

    }

    private bool MoveToTarget(float stopDistance)
    {
        if (targetIndex < path.Count)
        {
            Vector3 target = Vector3.MoveTowards(enemy.transform.position, path[targetIndex].worldPosition, enemyData.speed * Time.deltaTime);
            enemy.Rb.MovePosition(target);

            if (Vector3.Distance(enemy.transform.position, path[targetIndex].worldPosition) <= stopDistance)
            {
                targetIndex++;
                if (targetIndex < path.Count && path[targetIndex].isWalkable == false) { stateMachine.ChangeState(new EnemyDiggingState(enemy, stateMachine,Vector3Int.FloorToInt(path[targetIndex].worldPosition), new EnemyPathfindingState(enemy, stateMachine))); return false; }
            }
        }
        else
        {
            return true;
        }
        return false;
    }

    public List<Node> AnalyzePath()
    {
        List<Node> path = PathfindingManager.Instance.EnemyPathfindingInstance.FindPath(enemy.transform.position, DungeonCore.Instance.transform.position);
        if (path == null || path.Count <= 0) return null;

        return path;
    }
}
