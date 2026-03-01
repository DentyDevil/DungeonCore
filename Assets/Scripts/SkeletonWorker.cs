using System.Collections.Generic;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class SkeletonWorker : MonoBehaviour
{
    public enum WorkerState
    {
        Idle,
        GotoDig,
        Digging,
        GoToGetResource,
        TransferResourceToStorage,
        TowardResourceToBuild,
        GoToBuild,
        Building,
        GoToDecunstructBuilding,
        DecunstructBuilding
    }
    private WorkerState currentWorkerState = WorkerState.Idle;

    [Header("Skeleton characteristics")]
    [SerializeField] float workerSpeed = 2f;
    [SerializeField] int workerDiggingDamage = 10;
    [SerializeField] int timeBetweenDigging = 4;

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

    private List<Node> currentPath;
    private int targetIndex;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        StartCoroutine(FindJobRoutine());
    }
    void FixedUpdate()
    {
        HandleMovement(1);
    }
    IEnumerator FindJobRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            if (currentWorkerState == WorkerState.Idle)
            {
                GetAnyJob();
            }
        }
    }
    private void GetAnyJob()
    {
        if (currentWorkerState == WorkerState.Idle)
        {
            job = jobManager.DelegateWork(transform.position);
            if (job != null)
            {
                switch (job.jobType)
                {
                    case JobType.Dig:
                        currentTargetToDigging = job.position;
                        Vector3 targetWorldPos = new Vector3(currentTargetToDigging.x + 0.5f, currentTargetToDigging.y + 0.5f, 0);
                        currentPath = pathfinder.FindPath(transform.position, targetWorldPos);
                        if (currentPath != null)
                        {
                            targetIndex = 0;
                            currentWorkerState = WorkerState.GotoDig;
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
                                        currentPath = pathfinder.FindPath(transform.position, dropOnGround.transform.position);
                                        if (currentPath != null)
                                        {
                                            targetIndex = 0;
                                            currentWorkerState = WorkerState.GoToGetResource;
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
                            currentPath = pathfinder.FindPath(transform.position, currentBuildingTask.transform.position);
                            if (currentPath == null)
                            {
                                Debug.Log("К стройке невозможно дойти");
                                currentWorkerState = WorkerState.Idle;
                                jobManager.JobBecomeFree(job, 1);
                            }
                            else
                            {
                                currentWorkerState = WorkerState.GoToBuild;
                                targetIndex = 0;
                            }
                        }
                        break;
                    case JobType.Haul:
                        dropOnGround = job.worldResource;
                        if (dropOnGround == null) return;
                        currentPath = pathfinder.FindPath(transform.position, dropOnGround.transform.position);

                        if (currentPath != null)
                        {
                            targetIndex = 0;
                            currentWorkerState = WorkerState.GoToGetResource;
                        }
                        else
                        {
                            jobManager.JobBecomeFree(job, 1);
                        }
                        break;
                        case JobType.Deconstruct:
                        currentDecunstructBuilding = job.building;
                        if (currentDecunstructBuilding == null) return;
                        currentPath = pathfinder.FindPath(transform.position, currentDecunstructBuilding.transform.position);
                        if (currentPath != null)
                        {
                            targetIndex = 0;
                            currentWorkerState= WorkerState.GoToDecunstructBuilding;
                        }
                        else
                        {
                            Debug.Log("Пути к постройке которую надо разрушить на данный момент нет!");
                            jobManager.JobBecomeFree(job, 1);
                            currentWorkerState = WorkerState.Idle;
                        }
                        break;
                }
            }
        }
    }
    private bool MoveToTarget(float stopDistance)
    {
        if (targetIndex < currentPath.Count)
        {
            Vector3 target = Vector3.MoveTowards(transform.position, currentPath[targetIndex].worldPosition, workerSpeed * Time.deltaTime);
            rb.MovePosition(target);

            if (Vector3.Distance(transform.position, currentPath[targetIndex].worldPosition) <= stopDistance)
            {
                targetIndex++;
            }
        }
        else
        {
            return true;
        }
        return false;
    }
    void HandleMovement(float stopDistance)
    {
        if (currentPath == null) return;

        switch (currentWorkerState)
        {
            case WorkerState.Idle:
                break;
            case WorkerState.GotoDig:
                MoveToTarget(stopDistance);
                if (currentPath.Count == targetIndex) { currentWorkerState = WorkerState.Digging; }
                break;
            case WorkerState.Digging:
                Digging();
                break;
            case WorkerState.GoToGetResource:
                if (MoveToTarget(stopDistance))
                {
                    if (currentBuildingTask != null)
                    {
                        currentPath = pathfinder.FindPath(transform.position, currentBuildingTask.transform.position);
                        if (currentPath == null)
                        {
                            dropOnGround.transform.SetParent(null);
                            jobManager.AddHaulJob(dropOnGround);
                            jobManager.JobBecomeFree(job, 1);
                            currentWorkerState = WorkerState.Idle;
                            currentBuildingTask = null;
                            return;
                        }
                        else if (currentPath != null)
                        {
                            dropOnGround.transform.SetParent(transform);
                            targetIndex = 0;
                            currentWorkerState = WorkerState.TowardResourceToBuild;
                            jobManager.RemoveHaulJob(dropOnGround);
                        }
                    }
                    else
                    {
                        currentPath = pathfinder.FindPath(transform.position, dungeonCore.transform.position);
                        if (currentPath == null)
                        {
                            currentWorkerState = WorkerState.Idle;
                            dropOnGround.transform.SetParent(null);
                            jobManager.AddHaulJob(dropOnGround);
                            jobManager.JobBecomeFree(job, 1);
                            return;
                        }
                        else if (currentPath != null)
                        {
                            dropOnGround.transform.SetParent(transform);
                            targetIndex = 0;
                            jobManager.RemoveHaulJob(dropOnGround);
                            currentWorkerState = WorkerState.TransferResourceToStorage;

                        }
                    }

                }
                break;
            case WorkerState.TransferResourceToStorage:
                if (MoveToTarget(stopDistance))
                {
                    dungeonCore.AddBone();
                    Destroy(dropOnGround.gameObject);
                    currentWorkerState = WorkerState.Idle;
                }
                break;
            case WorkerState.TowardResourceToBuild:
                if (currentBuildingTask != null)
                {
                    if (MoveToTarget(stopDistance))
                    {
                        if (currentBuildingTask != null)
                        {
                            currentBuildingTask.AddResource(dropOnGround.resourceData);
                            Destroy(dropOnGround.gameObject);
                            currentBuildingTask = null;
                            jobManager.JobBecomeFree(job, 1);
                            currentWorkerState = WorkerState.Idle;
                        }
                        else
                        {

                        }
                    }
                }
                else
                {
                    dropOnGround.transform.SetParent(null);
                    jobManager.AddHaulJob(dropOnGround);
                    currentWorkerState = WorkerState.Idle;
                }
                break;
            case WorkerState.GoToBuild:
                if (MoveToTarget(stopDistance))
                {
                    currentWorkerState = WorkerState.Building;
                }
                break;
            case WorkerState.Building:
                if (currentBuildingTask != null)
                {
                    currentBuildingTask.Construct(Time.deltaTime);
                }
                else
                {
                    currentWorkerState = WorkerState.Idle;
                }
                break;
            case WorkerState.GoToDecunstructBuilding:
                if(currentDecunstructBuilding != null)
                {
                    if (MoveToTarget(stopDistance))
                    {
                        currentWorkerState = WorkerState.DecunstructBuilding;
                    }
                }
                else
                {
                    currentWorkerState = WorkerState.Idle;
                }
                break;
            case WorkerState.DecunstructBuilding:
                if(currentDecunstructBuilding != null)
                {
                    jobManager.RemoveDeconstruct(currentDecunstructBuilding);
                    currentDecunstructBuilding.Deconstruct();
                    currentWorkerState = WorkerState.Idle;
                }
                else
                {
                    currentWorkerState = WorkerState.Idle;
                }
                break;
        }
    }
    void Digging()
    {
        if (!jobManager.digJobs.ContainsKey(currentTargetToDigging)) { currentWorkerState = WorkerState.Idle; return; }

        digTimer += Time.deltaTime;

        if (digTimer >= timeBetweenDigging)
        {
            currentWorkerState = WorkerState.Digging;
            inputManager.DamageTile(currentTargetToDigging, workerDiggingDamage);
            digTimer = 0;
        }
    }
}
