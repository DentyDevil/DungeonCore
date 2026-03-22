using System.Collections.Generic;
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

    int steps = 0;


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
        steps = 0;
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
                steps++;
                CheckForTrap();
                if(enemy.isLeader) enemy.currentPathIndex = targetIndex;
                CheckForDoorAndPause();
                if (steps >= 2 && enemy.explorationMemory.Count <= 0) { Debug.Log("Осматриваюсь..."); steps = 0; stateMachine.ChangeState(new EnemyScanState(enemy, stateMachine, new EnemyPathfindingState(enemy, stateMachine))); return false; }
                
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
            AutoDoor currDoor = doorCollider.GetComponent<AutoDoor>();
            if (currDoor != null) { currDoor.OpenDoor(); ExplorationTarget door = enemy.explorationMemory.RemoveFirst(); Vector3Int doorPos = Vector3Int.FloorToInt(door.position); enemy.visitedCells.Add(doorPos); }  
        }
    }

    public void Updatepath(List<Node> newPath)
    {
        path = newPath;
        if(newPath.Count > 0) targetIndex = 1;
    }

    void CheckForTrap()
    {
        for(int i = 0; i < enemyData.detectTrapDistance; i++)
        {
            int checkIndex = targetIndex + i;
            if(checkIndex <  path.Count)
            {
                Vector3Int checkPos = Vector3Int.FloorToInt(path[checkIndex].worldPosition);
                TrapData trap = TrapManager.instance.TryDetect(checkPos, enemyData);
                if (trap != null)
                {
                    Debug.LogWarning("Ловушка обнаружена");
                    path = path.GetRange(0, checkIndex);
                    nextStateAfterMoving = new EnemyDisarmTrapState(enemy, stateMachine, checkPos);
                    break;
                }
                

            }
        }
    }
}
