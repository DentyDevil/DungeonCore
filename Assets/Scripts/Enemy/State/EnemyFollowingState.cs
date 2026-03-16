using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyFollowingState : EnemyBaseState
{
    int currIndex;
    List<Node> path;
    public EnemyFollowingState(BaseEnemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
    {

    }
    public override void Enter()
    {
        
    }
    public override void Execute()
    {
        
        if(enemy.leader.stateMachine.CurrentState is EnemyExploreRoomState)
        {
            Node startPos = PathfindingManager.Instance.Grid.NodeFromWorldPoint(enemy.transform.position);
            List<Node> room = enemy.leader.room.ToList();
            room.Remove(startPos);

            Vector3Int tileBeforeDoor = Vector3Int.FloorToInt(enemy.leader.path[enemy.leader.path.Count - 2].worldPosition);

            stateMachine.ChangeState(new EnemyScanState(enemy, stateMachine, new EnemyPathfindingState(enemy, stateMachine), true, tileBeforeDoor));
        }
        else
        {
            if (enemy.leader.path != null && enemy.leader.path.Count > 0)
            {
                path = enemy.leader.path.ToList();


                if (enemy.leader.currentPathIndex >= enemy.queuePosition + 1)
                {
                    path = path.GetRange(0, Mathf.Max(0, path.Count - enemy.queuePosition));
                    if (path.Count > 0 && Vector3.Distance(enemy.transform.position, path[path.Count - 1].worldPosition) > 0.1f) stateMachine.ChangeState(new EnemyMoveState(enemy, stateMachine, path, enemy.enemy, new EnemyPathfindingState(enemy, stateMachine)));
                }
            }
        }
    }
    public override void Exit()
    {

    }

}
