using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EnemyMoveState : EnemyBaseState
{
    EnemyData enemyData;
    private int targetIndex;
    List<Node> path;
    EnemyBaseState nextStateAfterMoving;

    bool isOpenigDoor = false;
    float doorTimer = 0f;
    float timeToOpenDoor;


    public EnemyMoveState(BaseEnemy enemy, EnemyStateMachine stateMachine, List<Node> _path, EnemyData _enemyData , EnemyBaseState nextState) : base(enemy, stateMachine)
    {
        path = _path;
        enemyData = _enemyData;
        nextStateAfterMoving = nextState;
        timeToOpenDoor = enemyData.timeToOpenDoor;   
    }

    public override void Enter()
    {
        CheckForDoorAndPause();
    }
    public override void Execute()
    {
        if (isOpenigDoor)
        {
            doorTimer += Time.deltaTime;
            if(doorTimer >= timeToOpenDoor)
            {
                isOpenigDoor = false;
                OpenDoorAt(path[targetIndex].worldPosition);
            }
            return;
        }
        if (MoveToTarget(0.1f))
        {
            stateMachine.ChangeState(nextStateAfterMoving);
        }
    }
    public override void Exit()
    {

    }
    private bool MoveToTarget(float stopDistance)
    {
        if (targetIndex < path.Count)
        {
            Vector3 target = Vector3.MoveTowards(enemy.transform.position, path[targetIndex].worldPosition, enemyData.speed * Time.deltaTime);
            enemy.rb.MovePosition(target);

            if (Vector3.Distance(enemy.transform.position, path[targetIndex].worldPosition) <= stopDistance)
            {
                targetIndex++;
                if(enemy.isLeader) enemy.currentPathIndex = targetIndex;
                CheckForDoorAndPause();
                
                if (targetIndex < path.Count && path[targetIndex].isWalkable == false && path[targetIndex].isDoor == false) { stateMachine.ChangeState(new EnemyDiggingState(enemy, stateMachine, Vector3Int.FloorToInt(path[targetIndex].worldPosition), new EnemyPathfindingState(enemy, stateMachine))); return false; }
            }
        }
        else
        {
            return true;
        }
        return false;
    }

    void CheckForDoorAndPause()
    {
        if(targetIndex < path.Count && path[targetIndex].isDoor)
        {
            doorTimer = 0;
            isOpenigDoor = true;
        }
    }
    void OpenDoorAt(Vector3 pos)
    {
        Collider2D doorCollider = Physics2D.OverlapPoint(pos);
        if (doorCollider != null)
        {
            AutoDoor door = doorCollider.GetComponent<AutoDoor>();
            if (door != null) door.OpenDoor();  
        }
    }

    public void Updatepath(List<Node> newPath)
    {
        path = newPath;
        if(newPath.Count > 0) targetIndex = 1;
    }
}
