using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathfindingGrid : MonoBehaviour
{
    public Tilemap wallsTilemap;
    public InputManager inputManager;
    public int gridWidth = 50;
    public int gridHeight = 50;
    public Vector3 startPosition;

    Node[,] grid;

    private void Start()
    {
        CreateGrid();
    }

    public void CreateGrid()
    {
        grid = new Node[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 worldPoint = startPosition + new Vector3(x + 0.5f, y + 0.5f, 0);

                Vector3Int cellPosition = wallsTilemap.WorldToCell(worldPoint);

                bool walkable = false;
                int movementPenalty = 100;
                if (!wallsTilemap.HasTile(cellPosition) && !inputManager.occupiedCells.Contains(cellPosition))
                {
                    walkable = true;
                    movementPenalty = 1;
                }


                grid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty);
            }
        }
    }

    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        int[] dx = { 1, -1, 0, 0, 1, -1, 1, -1 };
        int[] dy = { 0, 0, 1, -1, 1, -1, -1, 1 };

        for (int i = 0; i < 8; i++)
        {
            int checkX = node.gridX + dx[i];
            int checkY = node.gridY + dy[i];

            if(checkX >= 0 && checkX < gridWidth && checkY >= 0 && checkY < gridHeight)
            {
                neighbors.Add(grid[checkX, checkY]);
            }
        }
        return neighbors;
    }

    public List<Node> GetOrthogonalNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        int[] dx = { 1, -1, 0, 0, 1, -1, 1, -1 };
        int[] dy = { 0, 0, 1, -1, 1, -1, -1, 1 };

        for (int i = 0; i < 4; i++)
        {
            int checkX = node.gridX + dx[i];
            int checkY = node.gridY + dy[i];

            if (checkX >= 0 && checkX < gridWidth && checkY >= 0 && checkY < gridHeight)
            {
                neighbors.Add(grid[checkX, checkY]);
            }
        }
        return neighbors;
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x - startPosition.x);
        int y = Mathf.FloorToInt(worldPosition.y - startPosition.y);

        x = Mathf.Clamp(x, 0, gridWidth - 1);
        y = Mathf.Clamp(y, 0, gridHeight - 1);

        return grid[x, y];
    }

    public void UpdateNodeWalkability(Vector3 worldPosition, bool isWalkable)
    {
        Node nodeToUpdate = NodeFromWorldPoint(worldPosition);

        nodeToUpdate.isWalkable = isWalkable;
    }
}
