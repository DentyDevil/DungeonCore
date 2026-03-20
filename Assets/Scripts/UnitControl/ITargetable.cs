using Unity;
using UnityEngine;

public interface ITargetable
{
    Transform TargetTransform { get; }
    void TakeDamage(float damage);
    bool IsDead();
}
