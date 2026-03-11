using System.Collections.Generic;
using UnityEngine;

public class EnemyScanState : EnemyBaseState
{
    
    List<Node> dungeonMap = new List<Node>();
    EnemyBaseState nexState;

    bool exploreAfterScan;
    Vector3Int ignorePos;
    public EnemyScanState(WarriorEnemy enemy, EnemyStateMachine stateMachine, EnemyBaseState _nexState, bool _exploreAfterScan = false, Vector3Int _ignorePos = default) : base(enemy, stateMachine)
    {
        nexState = _nexState;
        exploreAfterScan = _exploreAfterScan;
        ignorePos = _ignorePos;
    }

    public override void Enter()
    {
        Node startPos = PathfindingManager.Instance.Grid.NodeFromWorldPoint(enemy.transform.position);
        dungeonMap = ScanDungeon(startPos, DungeonCore.Instance.maxRoomSize);

        enemy.room = dungeonMap;
        if (dungeonMap != null && dungeonMap.Count > 0 && exploreAfterScan)
        {
            dungeonMap.Remove(startPos);
            stateMachine.ChangeState(new EnemyExploreRoomState(enemy, stateMachine, dungeonMap, 3, nexState, true));
        }
        else
        {   
            stateMachine.ChangeState(nexState);
        }

    }
    public override void Execute()
    {
        
        
    }
    public override void Exit()
    {

    }

    List<Node> ScanDungeon(Node startNode, int maxSize)
    {
        Queue<Node> queue = new Queue<Node>();
        HashSet<Node> roomTiles = new HashSet<Node>();
        HashSet<Node> foundDoors = new HashSet<Node>();
        queue.Enqueue(startNode);
        roomTiles.Add(startNode);

        while (queue.Count > 0 && roomTiles.Count <= maxSize)
        {

            Node currentNode = queue.Dequeue();
            foreach (Node roomTileNeighbors in PathfindingManager.Instance.Grid.GetOrthogonalNeighbors(currentNode))
            {
                Vector3Int neighborPos = Vector3Int.FloorToInt(roomTileNeighbors.worldPosition);

                if (neighborPos == ignorePos) continue;
                if (enemy.visitedCells.Contains(neighborPos)) continue;

                if (!roomTiles.Contains(roomTileNeighbors) && roomTileNeighbors.isWalkable && !roomTileNeighbors.isDoor)
                {
                    roomTiles.Add(roomTileNeighbors);
                    queue.Enqueue(roomTileNeighbors);
                }
                else if (roomTileNeighbors.isDoor)
                {
                    if (foundDoors.Contains(roomTileNeighbors) || enemy.visitedCells.Contains(Vector3Int.FloorToInt(roomTileNeighbors.worldPosition))) continue;
                    foundDoors.Add(roomTileNeighbors);
                    Vector3 pos = roomTileNeighbors.worldPosition;
                    float distance = Vector3.Distance(enemy.transform.position, roomTileNeighbors.worldPosition);
                    float priority = distance + Random.Range(-0.5f, 0.5f);

                    enemy.explorationMemory.Add(new ExplorationTarget { position = pos, priority = priority });
                }


            }
        }
        Debug.LogWarning($"Дверей найдено - {foundDoors.Count}");
        Debug.LogWarning($"Проходимых тайлов найдено - {roomTiles.Count}");
        return new List<Node>(roomTiles);
    }
   
}
