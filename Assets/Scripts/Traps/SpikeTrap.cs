using UnityEngine;

public class SpikeTrap : BaseTrap
{
    public override void TryActivate(Collider2D target)
    {
        IDamageable enemy = target.GetComponent<IDamageable>();
        if (enemy != null) enemy.TakeDamage(trapData.damage);
        else Debug.LogWarning($"Объект {target.name} не имеет IDamageable", target.gameObject);
    }
}
