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
        Building
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

                        foreach (var nedeedRes in currentBuildingTask.resourceCost)
                        {
                            var collected = currentBuildingTask.resourcesCollected.Find(r => r.resourceData == nedeedRes.resourceData);

                            int collectedCount = 0;
                            if (collected != null) collectedCount = collected.count;

                            if(collectedCount < nedeedRes.count)
                            {
                                ResourceData resType = nedeedRes.resourceData;

                                dropOnGround = jobManager.GetResourceForBuild(transform.position, resType);

                                if(dropOnGround != null)
                                {
                                    currentPath = pathfinder.FindPath(transform.position, dropOnGround.transform.position);
                                    if(currentPath != null)
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
                        dropOnGround.transform.SetParent(transform);
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
                        targetIndex = 0;
                        currentWorkerState = WorkerState.TowardResourceToBuild;
                        jobManager.RemoveHaulJob(dropOnGround);
                    }
                    else
                    {
                        dropOnGround.transform.SetParent(transform);
                        currentPath = pathfinder.FindPath(transform.position, dungeonCore.transform.position);
                        if(currentPath == null)
                        {
                            dropOnGround.transform.SetParent(null);
                            jobManager.AddHaulJob(dropOnGround);
                            jobManager.JobBecomeFree(job, 1);
                            currentWorkerState = WorkerState.Idle;
                            return;
                        }
                        jobManager.RemoveHaulJob(dropOnGround);
                        targetIndex = 0;
                        currentWorkerState = WorkerState.TransferResourceToStorage;
                    }
                }
                break;
            case WorkerState.TransferResourceToStorage:
                if (MoveToTarget(stopDistance))
                {
                    dungeonCore.AddBone();
                    jobManager.RemoveHaulJob(dropOnGround);
                    Destroy(dropOnGround.gameObject);
                    currentWorkerState = WorkerState.Idle;
                }
                break;
            case WorkerState.TowardResourceToBuild:
                if (MoveToTarget(stopDistance))
                {
                    currentBuildingTask.AddResource(dropOnGround.resourceData);

                    Destroy(dropOnGround.gameObject);
                    jobManager.RemoveHaulJob(dropOnGround);
                    currentBuildingTask = null;
                    jobManager.JobBecomeFree(job, 1);
                    currentWorkerState = WorkerState.Idle;
                }
                break;
            case WorkerState.Building:
                break;
            default:
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
