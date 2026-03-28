using UnityEngine;

public abstract class BaseTrap : MonoBehaviour
{
    public TrapData trapData;
    protected float nextUseTime;
    [SerializeField] protected string targetTag = "Enemy";
    public virtual void Initialize(TrapData data)
    {
        trapData = data;
        nextUseTime = 0;
    }

    public abstract void TryActivate(Collider2D target);

    public virtual void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag(targetTag) && CanActivate())
        {
            nextUseTime = Time.time + trapData.coolDownTime;
            TryActivate(collision);
        }
    }

    protected bool CanActivate()
    {
        return Time.time >= nextUseTime;
    }
}
