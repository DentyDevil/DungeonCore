using UnityEngine;

public class BaseEnemy : MonoBehaviour, IDamageable
{
    public EnemyData enemy;
    public float healthPoints;

    Vector3 target;

    private void Awake()
    {
        healthPoints = enemy.healthPoints;
    }

    private void Start()
    {
        target = DungeonCore.Instance.transform.position;
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
