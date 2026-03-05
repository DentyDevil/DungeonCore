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

    private Vector3Int currentTargetToDigging;
    private WorldResource dropOnGround;
    private Job currentResourceJob;
    public ConstructionSite currentBuildingTask;
    public Building currentDecunstructBuilding;
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
    public void ContinueBuildingWork()
    {
        if (currentBuildingTask == null) { DropResource(); ChangeState(new IdleState(this)); return; }
        List<ResourceData> neededRes = currentBuildingTask.GetNextAllRequiredResource();
        if (neededRes != null)
        {
            currentResourceJob = jobManager.GetResourceForBuild(transform.position, neededRes);

            if (currentResourceJob != null)
            {
                dropOnGround = currentResourceJob.worldResource;
                List<Node> pathToDropForBuilding = pathfinder.FindPath(transform.position, dropOnGround.transform.position);
                if (pathToDropForBuilding != null)
                {
                    currentBuildingTask.AddIncomingResource(dropOnGround.resourceData);
                    ChangeState(new MovingState(this, pathToDropForBuilding, new PickupState(this, dropOnGround, pathfinder, currentBuildingTask.transform, job, jobManager, new DeliverToBuildState(this, currentBuildingTask, dropOnGround, jobManager, job))));
                }
                else
                {
                    Debug.Log("Путь не найден. Задача отложена.");
                    currentBuildingTask = null;
                    ChangeState(new IdleState(this));
                    jobManager.JobBecomeFree(job, 1);
                    if (currentResourceJob.IsValid()) jobManager.JobBecomeFree(currentResourceJob, 1);
                    DropResource();
                    dropOnGround = null;
                    
                }
            }
            else
            {
                Debug.Log("Не нашли ресурс на полу.");
                currentBuildingTask = null;
                ChangeState(new IdleState(this));
                jobManager.JobBecomeFree(job, 1);
            }
        }
        else
        {
            if (currentBuildingTask.isReadyToBuild)
            {
                List<Node> pathToBuilding = pathfinder.FindPath(transform.position, currentBuildingTask.transform.position);
                if (pathToBuilding == null)
                {
                    Debug.Log("К стройке невозможно дойти");
                    currentBuildingTask = null;
                    ChangeState(new IdleState(this));
                    jobManager.JobBecomeFree(job, 1);
                }
                else
                {
                    ChangeState(new MovingState(this, pathToBuilding, new BuildingState(this, currentBuildingTask)));
                }
            }
            else
            {
                dropOnGround = null;
                currentBuildingTask = null;
                jobManager.JobBecomeFree(job, 1);
                ChangeState(new IdleState(this));
            }
        }
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
