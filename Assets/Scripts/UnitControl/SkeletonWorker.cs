using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class SkeletonWorker : MonoBehaviour, ITargetable
{
    [Header("Skeleton characteristics")]
    public float workerSpeed = 2f;
    public float health = 100;

    public Transform TargetTransform => transform;
    public bool IsDead() => health <= 0;

    public float WorkerSpeed { get { return workerSpeed; } }
    [SerializeField] int workerDiggingDamage = 10;
    public int WorkerDiggingDamage { get { return workerDiggingDamage; } }
    [SerializeField] int timeBetweenDigging = 4;
    public int WorkerTimeBetweenDigging { get { return timeBetweenDigging; } }

    [SerializeField] JobManager jobManager;
    public JobManager JobManager { get { return jobManager; } }
    [SerializeField] Job job;
    public Job Job { get { return job; } }

    public InputManager inputManager;
    public DungeonCore dungeonCore;
    public Pathfinding pathfinder;
    public Pathfinding Pathfinding { get { return pathfinder; } }

    private Rigidbody2D rb;
    public Rigidbody2D Rb { get { return rb; } }
    private WorkerState currentState;

    public HashSet<Job> unreachableJobs = new HashSet<Job>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        jobManager.allWorkers.Add(this);

        ChangeState(new IdleState(this));
    }
    void FixedUpdate()
    {
        if (currentState != null)
        {
            currentState.Execute();
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        Debug.LogWarning($"Скелет получил - {damage} урона. Осталось - {health}HP");

        if (IsDead()) Die();
        DropResource();
        Destroy(gameObject);
    }

    public void Die()
    {
        if (job != null) jobManager.JobBecomeFree(job, 1);

    }

    public void GetAnyJob()
    {
        int maxAttempts = 5;
        int attempts = 0;

        while (attempts < maxAttempts)
        {

            job = jobManager.DelegateWork(this);

            if (job == null) return;

            if (job.TryStart(this))
            {
                return;
            }
            else
            {

                jobManager.JobBecomeFree(job, 1);
                unreachableJobs.Add(job);
                attempts++;
            }
        }
    }
    public void ChangeState(WorkerState newState)
    {
        if (currentState != null) currentState.Exit();
        currentState = newState;
        newState.Enter();
    }

    public void DropResource()
    {
        Debug.Log("1. Метод DropResource был вызван у скелета: " + gameObject.name);
        WorldResource recourceInHands = GetComponentInChildren<WorldResource>();

        if (recourceInHands != null)
        {
            Debug.Log("2. Ресурс найден в руках: " + recourceInHands.gameObject.name + ". Открепляем!");
            recourceInHands.transform.SetParent(null);
            jobManager.AddHaulJob(recourceInHands);
        }
        else
        {
            Debug.LogWarning("2. Скелет хотел бросить ресурс, но GetComponentInChildren вернул NULL!");
        }
    }

    private void OnDrawGizmos()
    {
        if (unreachableJobs == null) return;
        Gizmos.color = Color.yellow;
        
       foreach (var job in unreachableJobs)
        {
            Vector3 centPos = job.GetWorldPosition() + new Vector3(0.5f, 0.5f, 0f);
            Gizmos.DrawCube(centPos, Vector3.one);
        }
    }
}
