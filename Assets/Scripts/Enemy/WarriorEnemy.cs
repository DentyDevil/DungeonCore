using UnityEngine;

public class WarriorEnemy : BaseEnemy, IDamageable
{

    public override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        stateMachine.Initialize(new EnemyIdleState(this, stateMachine));
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

    public void Die()
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
