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

    [SerializeField] InputManager inputManager;
    [SerializeField] DungeonCore dungeonCore;
    [SerializeField] Pathfinding pathfinder;

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
        if (job != null)
        {
            switch (job.jobType)
            {
                case JobType.Dig:
                    currentTargetToDigging = job.position;
                    Vector3 targetWorldPos = new Vector3(currentTargetToDigging.x + 0.5f, currentTargetToDigging.y + 0.5f, 0);

                    List<Node> path = pathfinder.FindPath(transform.position, targetWorldPos);

                    if (path != null)
                    {
                        ChangeState(new MovingState(this, path, new DiggingState(this, jobManager, currentTargetToDigging, inputManager)));
                    }
                    else
                    {
                        Debug.Log("Сейчас это невозможно сделать. Задача переложена в режим ожидания");
                        jobManager.JobBecomeFree(job, 1);
                    }
                    break;
                case JobType.Build:
                    currentBuildingTask = job.constructionSite;
                    ContinueBuildingWork();
                    break;
                case JobType.Haul:
                    dropOnGround = job.worldResource;
                    if (dropOnGround == null) return;
                    List<Node> pathToDrop = pathfinder.FindPath(transform.position, dropOnGround.transform.position);
                    pathToDrop = pathfinder.FindPath(transform.position, dropOnGround.transform.position);

                    if (pathToDrop != null)
                    {
                        ChangeState(new MovingState(this, pathToDrop, new PickupState(this, dropOnGround, pathfinder, dungeonCore.transform, job, jobManager, new StoreResourceState(this, dropOnGround, dungeonCore))));
                    }
                    else
                    {
                        jobManager.JobBecomeFree(job, 1);
                    }
                    break;
                case JobType.Deconstruct:
                    currentDecunstructBuilding = job.building;
                    if (currentDecunstructBuilding == null) return;
                    List<Node> pathToDeconcsruct = pathfinder.FindPath(transform.position, currentDecunstructBuilding.transform.position);
                    if (pathToDeconcsruct != null)
                    {
                        ChangeState(new MovingState(this, pathToDeconcsruct, new DeconstructionState(this, currentDecunstructBuilding)));
                    }
                    else
                    {
                        Debug.Log("Пути к постройке которую надо разрушить на данный момент нет!");
                        jobManager.JobBecomeFree(job, 1);
                        ChangeState(new IdleState(this));
                    }
                    break;
            }
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
