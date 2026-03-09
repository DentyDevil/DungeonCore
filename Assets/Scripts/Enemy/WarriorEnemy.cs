using System.Collections.Generic;
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

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        healthPoints = enemy.healthPoints;
        explorationMemory = new Heap<ExplorationTarget>(100);
        visitedCells = new HashSet<Vector3Int>();
    }

    private void Start()
    {
        stateMachine.Initialize(new EnemyScanState(this, stateMachine));
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

    void Die()
    {
        Destroy(gameObject);
        Debug.Log("Враг умер! выпал дроп...");
    }
    
}
