using System;
using UnityEngine;

public static class TrapsEvents
{
    public static event Action<Vector3Int> OnTrapDisarmed;

    public static void OnTrapDisarmedEvent(Vector3Int pos)
    {
        OnTrapDisarmed?.Invoke(pos);
    }
}
