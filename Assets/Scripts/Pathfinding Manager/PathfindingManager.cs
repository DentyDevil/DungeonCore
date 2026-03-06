using UnityEngine;

public class PathfindingManager : MonoBehaviour
{
    public Pathfinding PathfindingInstance;
    public EnemyPathfinding EnemyPathfindingInstance;

    public static PathfindingManager Instance;

    private void Awake()
    {
        Instance = this;
    }
}
