using UnityEngine;

public class BaseEnemy : MonoBehaviour, IDamageable
{
    public EnemyData enemy;
    public EnemyStateMachine stateMachine;
    public float healthPoints;
    private Rigidbody2D rb;
    public Rigidbody2D Rb { get { return rb; } }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        healthPoints = enemy.healthPoints;
    }

    private void Start()
    {
        stateMachine.Initialize(new EnemyPathfindingState(this, stateMachine));
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
