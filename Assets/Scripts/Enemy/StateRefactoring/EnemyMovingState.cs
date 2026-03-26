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

    float timerTolookAround = 0;
    float howoftenLookAround = 3;

    bool isWalkingToCore = false;

    private Vector3Int target;
    Vector3 EnemyPos => enemy.transform.position;
    EnemyData enemyData => enemy.enemy;

    private List<Node> path = new();

    private LineRenderer lr => enemy.pathLine;
    public EnemyMovingState(BaseEnemy enemy, EnemyStateMachine stateMachine, Vector3Int _target, EnemyBaseState nextStateAfterReachingTarger, float _stopDistance = 0, bool _isWalkingToCore = false) : base(enemy, stateMachine)
    {
        nextState = nextStateAfterReachingTarger;
        stopDistanceSqr = _stopDistance * _stopDistance;
        target = _target;
        isWalkingToCore = _isWalkingToCore;
    }
    public override void Enter()
    {
        Debug.Log($"“екущее состо€ние - {stateMachine.CurrentState}");
        SpriteRenderer sprite = enemy.GetComponent<SpriteRenderer>();
        sprite.color = Color.blue;
        if (!isOpeningDoor) isPathAvailible = TryGetPathToTarget();
        CheckForTrap();
        if (isPathAvailible && path != null && path.Count > 0)
        {
            lr.enabled = true;
            DrawPath();     
        }
        if (isPathAvailible) ChechkForDoorAndPause();
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
                CheckForTrap();
                steps = 0;
                if (isPathAvailible) { ChechkForDoorAndPause(); DrawPath(); }
            }

            if (MoveToTarget())
            {
                stateMachine.ChangeState(nextState);
            }
            else
            {
                DrawPath();
            }

            LookAround();
        }
    }
    public override void Exit()
    {
        lr.enabled = false;
    }

    bool MoveToTarget()
    {
        if (!isPathAvailible) { stateMachine.ChangeState(new EnemyIdleState(enemy, stateMachine)); return false; }

        if (pathIndex < path.Count)
        {
            Vector3 targetPos = GetCurrentWaypoint();
            Vector3 newPos = Vector3.MoveTowards(EnemyPos, targetPos, enemy.enemy.speed * Time.deltaTime);
            enemy.rb.MovePosition(newPos);
            if ((EnemyPos - targetPos).sqrMagnitude <= stopDistanceSqr)
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

    void LookAround()
    {
        if (isWalkingToCore)
        {
            timerTolookAround += Time.deltaTime;
            if (timerTolookAround >= howoftenLookAround && !isOpeningDoor)
            {
                timerTolookAround = 0;
                if (enemy.explorationMemory.Count <= 0)
                {
                    Debug.LogWarning("—лучилось сканирование");
                    stateMachine.ChangeState(new EnemyScanAroundState(enemy, stateMachine, this));
                }
            }
            if (enemy.explorationMemory.Count > 0)
            {
                Debug.LogWarning("ѕри сканировании нашлись двери ");
                stateMachine.ChangeState(new EnemyExploreDungeon(enemy, stateMachine));
            }
        }
    }

    private bool TryGetPathToTarget()
    {
        List<Node> oldpath = path;
        Vector3? oldWaypoint = null;

        if (oldpath != null && pathIndex >= 0 && pathIndex < oldpath.Count)
        {
            oldWaypoint = oldpath[pathIndex].worldPosition;
        }

        List<Node> newPath = PathfindingManager.Instance.EnemyPathfindingInstance.FindPath(Vector3Int.FloorToInt(EnemyPos), target, false);

        if (newPath == null || newPath.Count <= 0)
        {
            path = null;
            return false;
        }
        if (oldWaypoint.HasValue && newPath[0].worldPosition == oldWaypoint.Value)
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
        if (pathIndex < path.Count) return path[pathIndex].worldPosition;
        else return Vector3.zero;
    }

    void ChechkForDoorAndPause()
    {
        if (pathIndex < path.Count && path[pathIndex].isDoor)
        {
            if (!isOpeningDoor)
            {
                doorTimer = 0;
                isOpeningDoor = true;
            }
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
                    path = path.GetRange(0, checkIndex);
                    bool previousData_IsWalkingToCore = isWalkingToCore;
                    nextState = new EnemyDisarmTrapState(enemy, stateMachine, checkPos, target, previousData_IsWalkingToCore, nextState);
                    break;
                }


            }
        }
    }

    void DrawPath()
    {
        if (path == null || path.Count == 0 || pathIndex >= path.Count)
        {
            lr.positionCount = 0;
            return;
        }

        //  ол-во точек линии = кол-во оставшихс€ точек в пути + текуща€ позици€ врага
        int pointsCount = path.Count - pathIndex + 1;
        lr.positionCount = pointsCount;

        // ѕерва€ точка линии - сам враг (чтобы лини€ т€нулась от него)
        lr.SetPosition(0, EnemyPos);

        // «аполн€ем остальные точки из пути, начина€ с текущего pathIndex
        int linePointIndex = 1;
        for (int i = pathIndex; i < path.Count; i++)
        {
            // path[i].worldPosition - это центр клетки, берем его
            lr.SetPosition(linePointIndex, path[i].worldPosition);
            linePointIndex++;
        }
    }
}
