using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class SkeletonWorker : MonoBehaviour
{
    [Header("Skeleton characteristics")]
    [SerializeField] private float workerSpeed = 2f;
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

    private WorldResource dropOnGround;
    private Rigidbody2D rb;
    public Rigidbody2D Rb { get { return rb; } }
    private WorkerState currentState;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ChangeState(new IdleState(this));
    }
    void FixedUpdate()
    {
        if (currentState != null)
        {
            currentState.Execute();
        }
    }
    public void GetAnyJob()
    {
        job = jobManager.DelegateWork(transform.position);
        if (job == null) return;

        if (job.TryStart(this) == false)
        {
            jobManager.JobBecomeFree(job, 1);
            jobManager.unreachebleTasks.Add(job);
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
        if (dropOnGround != null)
        {
            dropOnGround.transform.SetParent(null);
            jobManager.AddHaulJob(dropOnGround);
            dropOnGround = null;
        }
    }
}
