using UnityEngine;
using System.Collections.Generic;
public class EnemyMovingState : EnemyBaseState
{
    EnemyBaseState nextState;
    private float stopDistanceSqr;
    private int pathIndex;
    private bool isPathAvailible;

    int steps = 0;
    int howOftenToCheckPath = 10;

    bool isOpeningDoor = false;
    float doorTimer = 0;
    float timeToOpenDoor => enemy.enemy.timeToOpenDoor;

    private Vector3Int target;
    Vector3 EnemyPos => enemy.transform.position;
    EnemyData enemyData => enemy.enemy;

    private List<Node> path = new();
    public EnemyMovingState(BaseEnemy enemy, EnemyStateMachine stateMachine, Vector3Int _target, EnemyBaseState nextStateAfterReachingTarger, float _stopDistance) : base(enemy, stateMachine)
    {
        nextState = nextStateAfterReachingTarger;
        stopDistanceSqr = _stopDistance * _stopDistance;
        target = _target;
    }
    public override void Enter()
    {
        isPathAvailible = TryGetPathToTarget();
        if (isPathAvailible) ChechkForDoorAndPause();
        steps = 0;
        doorTimer = 0;
    }
    public override void Execute()
    {
        if (isOpeningDoor)
        {
            doorTimer += Time.deltaTime;
            if (doorTimer >= timeToOpenDoor)
            {
                isOpeningDoor = false;
                OpenDoorAt(path[pathIndex].worldPosition);
            }
            return;
        }
        if (!isOpeningDoor)
        {
            if (steps >= howOftenToCheckPath && enemy.explorationMemory.Count <= 0)
            {
                isPathAvailible = TryGetPathToTarget();
                steps = 0;
                if (isPathAvailible) ChechkForDoorAndPause();
            }

            if (MoveToTarget())
            {
                stateMachine.ChangeState(nextState);
            }
        }
    }
    public override void Exit()
    {

    }

    bool MoveToTarget()
    {
        if (!isPathAvailible) { stateMachine.ChangeState(new EnemyIdleState(enemy, stateMachine)); return false; }

        if (pathIndex < path.Count)
        {
            Vector3 targetPos = GetCurrentWaypoint();
            Vector3 newPos = Vector3.MoveTowards(EnemyPos, targetPos, enemy.enemy.speed * Time.deltaTime);
            enemy.rb.MovePosition(newPos);
            if((EnemyPos - targetPos).sqrMagnitude <= stopDistanceSqr)
            {
                pathIndex++;
                ChechkForDoorAndPause();
                CheckForTrap();
                steps++;
            }
        } 
        else
        {
            return true;
        }
        return false;
    }

    private bool TryGetPathToTarget()
    {
        List<Node> oldpath = path;
        Vector3? oldWaypoint = null;

        if(oldpath != null && pathIndex >= 0 && pathIndex < oldpath.Count)
        {
            oldWaypoint = oldpath[pathIndex].worldPosition;
        }

        List<Node> newPath = PathfindingManager.Instance.EnemyPathfindingInstance.FindPath(Vector3Int.FloorToInt(EnemyPos), target, false);

        if(newPath == null || newPath.Count <= 0)
        {
            path = null;
            return false;
        }
        if(oldWaypoint.HasValue && newPath[0].worldPosition == oldWaypoint.Value)
        {
            path = newPath;
            return true;
        }
        else
        {
            path = newPath;
            pathIndex = 0;
            return true;
        }
    }
    
    Vector3 GetCurrentWaypoint()
    {
        if(pathIndex < path.Count) return path[pathIndex].worldPosition;
        else return Vector3.zero;
    }

    void ChechkForDoorAndPause()
    {
        if(pathIndex < path.Count && path[pathIndex].isDoor)
        {
            doorTimer = 0;
            isOpeningDoor = true;
        }
    }
    void OpenDoorAt(Vector3 pos)
    {
        Collider2D doorCollider = Physics2D.OverlapPoint(pos);
        if (doorCollider != null)
        {
            AutoDoor currDoor = doorCollider.GetComponent<AutoDoor>();
            if (currDoor != null)
            {
                currDoor.OpenDoor();
                Vector3Int doorPos = Vector3Int.FloorToInt(pos);

                if (enemy.explorationMemory.Count > 0)
                {
                    ExplorationTarget plannedDoor = enemy.explorationMemory.GetFirst();

                    if (Vector3Int.FloorToInt(plannedDoor.position) == doorPos) enemy.explorationMemory.RemoveFirst();
                }
                
                enemy.visitedCells.Add(doorPos);
            }
        }
    }
    void CheckForTrap()
    {
        for (int i = 0; i < enemyData.detectTrapDistance; i++)
        {
            int checkIndex = pathIndex + i;
            if (checkIndex < path.Count)
            {
                Vector3Int checkPos = Vector3Int.FloorToInt(path[checkIndex].worldPosition);
                TrapData trap = TrapManager.instance.TryDetect(checkPos, enemyData);
                if (trap != null)
                {
                    Debug.LogWarning("Ловушка обнаружена");
                    path = path.GetRange(0, checkIndex);
                    nextState = new EnemyDisarmTrapState(enemy, stateMachine, checkPos);
                    break;
                }


            }
        }
    }
}
