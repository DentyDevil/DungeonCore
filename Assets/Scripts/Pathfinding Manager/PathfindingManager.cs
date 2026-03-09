using UnityEngine;

public class PathfindingManager : MonoBehaviour
{
    public Pathfinding PathfindingInstance;
    public EnemyPathfinding EnemyPathfindingInstance;
    public PathfindingGrid Grid;

    public static PathfindingManager Instance;

    private void Awake()
    {
        Instance = this;
    }
}
