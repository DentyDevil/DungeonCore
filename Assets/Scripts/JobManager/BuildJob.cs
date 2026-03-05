using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class BuildJob : Job
{
    public BuildJob(ConstructionSite constructionSite, int priority) : base(JobType.Build, constructionSite, priority)
    {

    }

    public override bool TryStart(SkeletonWorker worker)
    {
        if (constructionSite == null) { worker.DropResource(); worker.ChangeState(new IdleState(worker)); return false; }

        List<ResourceData> neededRes = constructionSite.GetNextAllRequiredResource();
        if (neededRes != null)
        {
            Job currenResourceJob = worker.JobManager.GetResourceForBuild(worker.transform.position, neededRes);
            if (currenResourceJob != null)
            {
                WorldResource drop = currenResourceJob.worldResource;
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
}
