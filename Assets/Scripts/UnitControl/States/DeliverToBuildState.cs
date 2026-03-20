using UnityEngine;
public class DeliverToBuildState : WorkerState
{
    ConstructionSite building;
    WorldResource resourceForBuilding;
    JobManager jobManager;
    Job job;
    public DeliverToBuildState(SkeletonWorker worker, ConstructionSite _building, WorldResource _resourceForBuilding, JobManager _jobManager, Job _job) : base(worker)
    {
        building = _building;
        resourceForBuilding = _resourceForBuilding;
        jobManager = _jobManager;
        job = _job;
    }

    public override void Enter()
    {
        if (building != null && resourceForBuilding != null)
        {
            building.RemoveIncomingResource(resourceForBuilding.resourceData);
            building.AddResource(resourceForBuilding.resourceData);
            Object.Destroy(resourceForBuilding.gameObject);
            jobManager.JobBecomeFree(job, 1);
            worker.ChangeState(new IdleState(worker));
        }
        else
        {
            jobManager.JobBecomeFree(job, 1);
            worker.ChangeState(new IdleState(worker));
        }
    }

    public override void Execute()
    {
       
    }

    public override void Exit()
    {

    }
}
