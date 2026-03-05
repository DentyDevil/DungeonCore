using System.Collections.Generic;
using UnityEngine;

public class BuildJob : Job
{
    public ConstructionSite constructionSite;
    private Vector3Int position;
    public BuildJob(ConstructionSite _constructionSite, int priority) : base(priority)
    {
        constructionSite = _constructionSite;
        position = Vector3Int.FloorToInt(constructionSite.transform.position);
    }

    public override bool TryStart(SkeletonWorker worker)
    {
        if (constructionSite == null) { worker.DropResource(); worker.ChangeState(new IdleState(worker)); return false; }

        List<ResourceData> neededRes = constructionSite.GetNextAllRequiredResource();
        if (neededRes != null)
        {
            Job currenResourceJob = worker.JobManager.GetResourceForBuild(worker.transform.position, neededRes);
            if (currenResourceJob is HaulJob haultask)
            {
                WorldResource drop = haultask.worldResource;
                List<Node> pathToDropForBuilding = worker.pathfinder.FindPath(worker.transform.position, drop.transform.position);
                if(pathToDropForBuilding != null)
                {
                    workersInWork++;
                    constructionSite.AddIncomingResource(drop.resourceData);
                    worker.ChangeState(new MovingState(worker, pathToDropForBuilding, new PickupState(worker, drop, worker.pathfinder, constructionSite.transform, this, worker.JobManager, new DeliverToBuildState(worker, constructionSite, drop, worker.JobManager, this))));
                    return true;
                }
                else
                {
                    Debug.Log("Путь не найден. Задача отложена.");
                    worker.ChangeState(new IdleState(worker));
                    worker.DropResource();
                    worker.JobManager.JobBecomeFree(this, 1);
                    return false;
                }
            }
            else
            {
                Debug.Log("Не нашли ресурс на полу.");
                worker.ChangeState(new IdleState(worker));
                worker.JobManager.JobBecomeFree(this, 1);
                return false;
            }
        }
        else
        {
            if (constructionSite.isReadyToBuild)
            {
                List<Node> pathToBuilding = worker.pathfinder.FindPath(worker.transform.position, constructionSite.transform.position);
                if (pathToBuilding == null)
                {
                    Debug.Log("К стройке невозможно дойти");
                    worker.ChangeState(new IdleState(worker));
                    worker.JobManager.JobBecomeFree(this, 1);
                    return false;
                }
                else
                {
                    worker.ChangeState(new MovingState(worker, pathToBuilding, new BuildingState(worker, constructionSite)));
                    workersInWork++;
                    return true;
                }
            }
            else
            {
                worker.ChangeState(new IdleState(worker));
                worker.JobManager.JobBecomeFree(this, 1);
                return false;
            }
        }
    }
    public override Vector3 GetWorldPosition()
    {
        return position;
    }
    public override int GetPriority()
    {
        return workPriority;
    }

    public override bool StillValid(SkeletonWorker skeletonWorker)
    {
        if(constructionSite != null) return true;
        return false;
    }

    public override bool CanExecute()
    {
        return true;
    }

    public override bool IsValid()
    {
        if (constructionSite != null) return true;
        return false;
    }
}
