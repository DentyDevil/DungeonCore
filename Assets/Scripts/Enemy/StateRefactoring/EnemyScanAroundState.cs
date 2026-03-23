using System.Collections.Generic;
using UnityEngine;

public class EnemyScanAroundState : EnemyBaseState
{

    List<Node> scannedTiles;
    public EnemyScanAroundState(BaseEnemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
    {

    }
    public override void Enter()
    {
        Node enemyPos = PathfindingManager.Instance.Grid.NodeFromWorldPoint(enemy.transform.position);
        scannedTiles = ScanDungeon(enemyPos, DungeonCore.Instance.maxRoomSize);
        if (scannedTiles != null) enemy.room = scannedTiles;
        stateMachine.ChangeState(new EnemyExploreDungeon(enemy, stateMachine));
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

                if (enemy.visitedCells.Contains(neighborPos)) continue;

                if (!roomTiles.Contains(roomTileNeighbors) && roomTileNeighbors.isWalkable && !roomTileNeighbors.isDoor)
                {
                    roomTiles.Add(roomTileNeighbors);
                    queue.Enqueue(roomTileNeighbors);
                }
                else if (roomTileNeighbors.isDoor)
                {
                    if (foundDoors.Contains(roomTileNeighbors) || enemy.visitedCells.Contains(Vector3Int.FloorToInt(roomTileNeighbors.worldPosition))) continue;

                    bool isAlreadyInMemory = false;

                    foreach (ExplorationTarget doorsInMemory in enemy.explorationMemory.GetItems())
                    {
                        if (Vector3Int.FloorToInt(doorsInMemory.position) == Vector3Int.FloorToInt(roomTileNeighbors.worldPosition)) { isAlreadyInMemory = true; break; }
                    }

                    if (!isAlreadyInMemory)
                    {
                        foundDoors.Add(roomTileNeighbors);
                        Vector3 pos = roomTileNeighbors.worldPosition;
                        float distance = Vector3.Distance(enemy.transform.position, roomTileNeighbors.worldPosition);
                        float priority = distance + Random.Range(-0.5f, 0.5f);

                        enemy.explorationMemory.Add(new ExplorationTarget { position = pos, priority = priority });
                    }
                }


            }
        }
        Debug.LogWarning($"Дверей найдено - {foundDoors.Count}");
        Debug.LogWarning($"Проходимых тайлов найдено - {roomTiles.Count}");
        return new List<Node>(roomTiles);
    }
}
