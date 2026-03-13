using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    PathfindingGrid grid;
    Heap<Node> heap;

    void Awake()
    {
        grid = GetComponent<PathfindingGrid>();
        heap = new Heap<Node>(grid.gridWidth * grid.gridHeight);
    }

    public List<Node> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        grid.ResetNodeCosts();
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        startNode.gCost = 0;
        startNode.hCost = GetDistance(startNode, targetNode);

        heap.Clear();

        HashSet<Node> closedSet = new HashSet<Node>();

        heap.Add(startNode);

        while (heap.Count > 0)
        {
            Node currentNode = heap.RemoveFirst();
            closedSet.Add(currentNode);
            
            if(currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            foreach (Node neighbor in grid.GetOrthogonalNeighbors(currentNode))
            {

                if (closedSet.Contains(neighbor)) continue;

                if (!neighbor.isWalkable && neighbor != targetNode) continue;
                
                int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor) + neighbor.movementPenalty;

                if (newMovementCostToNeighbor < neighbor.gCost || !heap.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (heap.Contains(neighbor)) heap.UpdateItem(neighbor);
                    else heap.Add(neighbor);
                }
            }
        }
        return null;
    }

    List<Node> RetracePath(Node startNode, Node targetNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = targetNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();

        return path;
    }

    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        return dstX + dstY;
    }
}