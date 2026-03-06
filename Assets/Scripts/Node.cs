using UnityEngine;

public class Node 
{
    public bool isWalkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;

    public int gCost;
    public int hCost;

    public int movementPenalty;

    public int fCost {get { return gCost + hCost; } }

    public Node parent;

    public Node(bool _isWalcable, Vector3 _worldPos, int _gridX, int _gridY, int _movementPenalty)
    {
        isWalkable = _isWalcable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        movementPenalty = _movementPenalty;
    }
}
