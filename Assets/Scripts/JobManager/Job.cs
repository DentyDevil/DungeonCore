using UnityEngine;

public enum JobType
{
    Dig,
    Build,
    Haul,
    Deconstruct
}

[System.Serializable]
public class Job : JobBase
{
    public JobType jobType;
    public Vector3Int position;
    //public GameObject dropGameObject;
    public ConstructionSite constructionSite;
    public WorldResource worldResource;
    public Building building;
    public int workPriority;

    public Job(JobType job, Vector3Int pos, int priority = 3)
    {
        jobType = job;
        position = pos;
        workPriority = priority;
    }

    public Job(JobType job, ConstructionSite site, int priority = 3)
    {
        jobType = job;
        constructionSite = site;
        workPriority = priority;

        position = Vector3Int.FloorToInt(site.transform.position);
    }
    public Job(JobType job, WorldResource _worldResource, int priority = 3)
    {
        jobType = job;
        worldResource = _worldResource;
        workPriority = priority;
    }
    public Job(JobType job, Building _building, int priority = 3)
    {
        jobType = job;
        building = _building;
        workPriority= priority;

        position = Vector3Int.FloorToInt(_building.transform.position);
    }

    public override Vector3 GetWorldPosition()
    {
        return position;
    }

    public override int GetPriority()
    {
        return workPriority;
    }

    public override bool IsValid()
    {
        switch (jobType)
        {
            case JobType.Build:
                if(constructionSite == null)
                    return false;
                break;
            case JobType.Haul:
                if(worldResource == null)
                    return false;
                break;
            case JobType.Deconstruct:
                if (building == null)
                    return false;
                break;
        }
        return true;
    }
    public override bool CanExecute()
    {
        switch (jobType)
        {
            case JobType.Haul:
                return worldResource.resourceData.isAllowedToHaul;
        }
        return true;
    }
}
