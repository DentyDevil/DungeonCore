using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class WarriorEnemy : MonoBehaviour, IDamageable
{
    public EnemyData enemy;
    public EnemyStateMachine stateMachine;
    float healthPoints;
    private Rigidbody2D rb;
    public Rigidbody2D Rb { get { return rb; } }

    public Heap<ExplorationTarget> explorationMemory;
    public HashSet<Vector3Int> visitedCells;

    public List<Node> room;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        healthPoints = enemy.healthPoints;
        explorationMemory = new Heap<ExplorationTarget>(100);
        visitedCells = new HashSet<Vector3Int>();
    }

    private void Start()
    {
        stateMachine.Initialize(new EnemyScanState(this, stateMachine, new EnemyPathfindingState(this, stateMachine)));
    }

    public void TakeDamage(float damage)
    {
        healthPoints -= damage;
        Debug.Log($"Враг получил урон - {damage} Осталось HP: {healthPoints}" );
        if(healthPoints <= 0)
        {
            Die();
        }
    }
    private void Update()
    {

    }

    void Die()
    {
        Destroy(gameObject);
        Debug.Log("Враг умер! выпал дроп...");
    }

    private void OnDrawGizmos()
    {
        if (room == null || room.Count == 0) return;

        Gizmos.color = new Color(0f, 1f, 0f, 0.5f);

        foreach (Node node in room)
        {
            Gizmos.DrawCube(node.worldPosition, Vector3.one);
        }

        if (explorationMemory == null || explorationMemory.Count == 0) return;

        Gizmos.color = Color.purple;

        foreach (var doors in explorationMemory.GetItems())
        {
            Gizmos.DrawCube(doors.position, Vector3.one);
        }
    }

}
