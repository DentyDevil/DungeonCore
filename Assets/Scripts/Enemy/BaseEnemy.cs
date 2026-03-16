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

    public virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        healthPoints = enemy.healthPoints;
        explorationMemory = new Heap<ExplorationTarget>(100);
        visitedCells = new HashSet<Vector3Int>();
        stateMachine = GetComponent<EnemyStateMachine>();
    }
}
