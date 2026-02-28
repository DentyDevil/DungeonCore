using UnityEngine;

public enum JobType
{
    Dig,
    Build,
    Haul
}

[System.Serializable]
public class Job 
{
    public JobType jobType;
    public Vector3Int position;
    public GameObject dropGameObject;
    public ConstructionSite constructionSite;
    public int workersInWork;
    public int workPriority;
    public WorldResource worldResource;

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
}
