using System;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    public bool isLeader;
    public BaseEnemy leader;
    public List<Node> path = new();

    [SerializeField] public Rigidbody2D rb;
    [SerializeField] public float healthPoints;
    public Heap<ExplorationTarget> explorationMemory;
    public HashSet<Vector3Int> visitedCells;
    public List<Node> room;
    public EnemyData enemy;
    public int queuePosition;
    public int currentPathIndex;
    public EnemyStateMachine stateMachine;

    [Header("Агро система")]
    public LayerMask targetLayer; 
    public ITargetable currentTarget;

    float aggroRadius;
    float attackRange;
    float pathUpdateTimer;
    float pathUpdateInterval = 0.5f;

    public virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        healthPoints = enemy.healthPoints;
        explorationMemory = new Heap<ExplorationTarget>(100);
        visitedCells = new HashSet<Vector3Int>();
        stateMachine = GetComponent<EnemyStateMachine>();
        aggroRadius = enemy.aggroRadius;
        attackRange = enemy.attackRange;
    }
    private void FixedUpdate()
    {
        //if (currentTarget == null || currentTarget.IsDead()) FindTarget();

        //if(currentTarget != null && !currentTarget.IsDead())
        //{
        //    float distance = Vector3.Distance(transform.position, currentTarget.TargetTransform.position);
        //    if (distance <= attackRange)
        //    {
        //        //if(!(stateMachine.CurrentState is EnemyMeleeAttackState))
        //        //{
        //        //    EnemyBaseState returnState = stateMachine.CurrentState;
        //        //    stateMachine.ChangeState(new EnemyMeleeAttackState(this, stateMachine, currentTarget, returnState));
        //        //}
        //    }
        //    else
        //    {
        //        if(stateMachine.CurrentState is EnemyMoveState moveState)
        //        {
        //            pathUpdateTimer += Time.deltaTime;
        //            if(pathUpdateTimer >= pathUpdateInterval)
        //            {
        //                pathUpdateTimer = 0;
        //                Vector3Int startPos = Vector3Int.FloorToInt(transform.position);
        //                Vector3Int targetPos = Vector3Int.FloorToInt(currentTarget.TargetTransform.position);

        //                List<Node> newPath = PathfindingManager.Instance.EnemyPathfindingInstance.FindPath(startPos, targetPos, false);
        //                if (newPath != null) moveState.Updatepath(newPath);
        //            }
        //        }
        //        else
        //        {
        //            pathUpdateTimer = 0;
        //            Vector3Int startPos = Vector3Int.FloorToInt(transform.position);
        //            Vector3Int targetPos = Vector3Int.FloorToInt(currentTarget.TargetTransform.position);

        //            List<Node> newPath = PathfindingManager.Instance.EnemyPathfindingInstance.FindPath(startPos, targetPos, false);
        //            if (newPath != null)
        //            {
        //                EnemyBaseState returnState = stateMachine.CurrentState;
        //                stateMachine.ChangeState(new EnemyMoveState(this, stateMachine, newPath, enemy, returnState));
        //            }
        //        }
        //    }
        //} 
    }

    //private void FindTarget()
    //{
    //    Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aggroRadius, targetLayer);
    //    float closestDistance = Mathf.Infinity;
    //    ITargetable closestTarget = null;
    //    foreach (Collider2D hit in hits)
    //    {
    //        ITargetable target = hit.GetComponent<ITargetable>();
    //        if(target != null && !target.IsDead())
    //        {
    //            float dist = Vector3.Distance(transform.position, hit.transform.position);
    //            if(dist < closestDistance)
    //            {
    //                closestDistance = dist;
    //                closestTarget = target;
    //            }
    //        }
    //    }
    //    currentTarget = closestTarget;
    //}
}
