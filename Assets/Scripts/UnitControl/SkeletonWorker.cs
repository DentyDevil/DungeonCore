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

    [SerializeField] InputManager inputManager;
    [SerializeField] DungeonCore dungeonCore;
    [SerializeField] Pathfinding pathfinder;
    [SerializeField] JobManager jobManager;
    [SerializeField] Job job;

    private float digTimer = 0f;
    private Vector3Int currentTargetToDigging;
    private WorldResource dropOnGround;
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
        if(currentState != null)
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

                    List<Node> path = pathfinder.FindPath(transform.position, currentTargetToDigging);
                    path = pathfinder.FindPath(transform.position, targetWorldPos);

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
                    if (!currentBuildingTask.isReadyToBuild)
                    {
                        foreach (var nedeedRes in currentBuildingTask.resourceCost)
                        {
                            var collected = currentBuildingTask.resourcesCollected.Find(r => r.resourceData == nedeedRes.resourceData);

                            int collectedCount = 0;
                            if (collected != null) collectedCount = collected.count;

                            if (collectedCount < nedeedRes.count)
                            {
                                ResourceData resType = nedeedRes.resourceData;

                                dropOnGround = jobManager.GetResourceForBuild(transform.position, resType);

                                if (dropOnGround != null)
                                {
                                    List<Node> pathToDropForBuilding = pathfinder.FindPath(transform.position, dropOnGround.transform.position);
                                    if (pathToDropForBuilding != null)
                                    {
                                        ChangeState(new MovingState(this, pathToDropForBuilding, new PickupState(this, dropOnGround, pathfinder, currentBuildingTask.transform, job, jobManager, new DeliverToBuildState(this, currentBuildingTask, dropOnGround, jobManager, job))));
                                        break;
                                    }
                                    else
                                    {
                                        Debug.Log("Путь не найден. Задача отложена.");
                                        jobManager.JobBecomeFree(job, 1);
                                    }
                                }
                                else
                                {
                                    Debug.Log("Не нашли ресурс на полу.");
                                    jobManager.JobBecomeFree(job, 1);
                                }

                            }
                        }
                    }
                    else
                    {
                        List<Node> pathToBuilding = pathfinder.FindPath(transform.position, currentBuildingTask.transform.position);
                        if (pathToBuilding == null)
                        {
                            Debug.Log("К стройке невозможно дойти");
                            ChangeState(new IdleState(this));
                            jobManager.JobBecomeFree(job, 1);
                        }
                        else
                        {
                            ChangeState(new MovingState(this, pathToBuilding, new BuildingState(this, currentBuildingTask)));
                        }
                    }
                    break;
                case JobType.Haul:
                    dropOnGround = job.worldResource;
                    if (dropOnGround == null) return;
                    List<Node> pathToDrop = pathfinder.FindPath(transform.position, dropOnGround.transform.position);
                    pathToDrop = pathfinder.FindPath(transform.position, dropOnGround.transform.position);

                    if (pathToDrop != null)
                    {
                        if (currentBuildingTask == null)
                        {
                            ChangeState(new MovingState(this, pathToDrop, new PickupState(this, dropOnGround, pathfinder, dungeonCore.transform, job, jobManager, new StoreResourceState(this, dropOnGround, dungeonCore))));
                        }
                        else
                        {
                            ChangeState(new MovingState(this, pathToDrop, new PickupState(this, dropOnGround, pathfinder, currentBuildingTask.transform, job, jobManager, new DeliverToBuildState(this, currentBuildingTask, dropOnGround, jobManager, job))));
                        }
                    }
                    else
                    {
                        jobManager.JobBecomeFree(job, 1);
                    }
                    break;
                //case JobType.Deconstruct:
                //    currentDecunstructBuilding = job.building;
                //    if (currentDecunstructBuilding == null) return;
                //    currentPath = pathfinder.FindPath(transform.position, currentDecunstructBuilding.transform.position);
                //    if (currentPath != null)
                //    {
                //        currentWorkerState = WorkerBaseState.GoToDecunstructBuilding;
                //    }
                //    else
                //    {
                //        Debug.Log("Пути к постройке которую надо разрушить на данный момент нет!");
                //        jobManager.JobBecomeFree(job, 1);
                //        currentWorkerState = WorkerBaseState.Idle;
                //    }
                //    break;
            }
        }

    }
    public void ChangeState(WorkerState newState)
    {
        if (currentState != null) currentState.Exit();
        currentState = newState;
        newState.Enter();
    }
}
