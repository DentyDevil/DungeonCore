using System.Collections.Generic;
using UnityEngine;

public class TrapManager : MonoBehaviour
{
    public static TrapManager instance;
    private Dictionary<Vector3Int, ActiveTrapInstance> activeTraps = new();

    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
    }

    public TrapData TryDetect(Vector3Int trapPos, EnemyData enemyData)
    {
        if (activeTraps.TryGetValue(trapPos, out ActiveTrapInstance trapInstance) && trapInstance != null)
        {
            switch (trapInstance.trapData.trapClass)
            {
                case TrapClass.Static:
                    if (trapInstance.trapData.detectDifficulty < enemyData.mechanicalDetectedSkill) return trapInstance.trapData;
                    break;
                case TrapClass.Mechanical:
                    if (trapInstance.trapData.detectDifficulty < enemyData.mechanicalDetectedSkill) return trapInstance.trapData;
                    Debug.LogWarning($"Ловушка обнаружена. Незаметность ловушки - {trapInstance.trapData.detectDifficulty}. Навык внимательности юнита - {enemyData.mechanicalDetectedSkill}");
                    break;
                case TrapClass.Magic:
                    if (trapInstance.trapData.detectDifficulty < enemyData.magicDetectedSkill) return trapInstance.trapData;
                    break;
            }
        }
        return null;
    }

    public bool TryDisarm(Vector3Int trapPos, EnemyData enemyData)
    {
        if (activeTraps.TryGetValue(trapPos, out ActiveTrapInstance trapInstance) && trapInstance != null)
        {
            switch (trapInstance.trapData.trapClass)
            {
                case TrapClass.Static:
                    if (trapInstance.trapData.disarmDifficulty < enemyData.mechanicalDisarmSkill) { DisarmTrap(trapPos); return true; }
                    break;
                case TrapClass.Mechanical:
                    if (trapInstance.trapData.disarmDifficulty < enemyData.mechanicalDisarmSkill) { DisarmTrap(trapPos); return true; }
                    break;
                case TrapClass.Magic:
                    if (trapInstance.trapData.disarmDifficulty < enemyData.magicDisarmSkill) { DisarmTrap(trapPos); return true; }
                    break;
            }
        }
        return false;
    }

    public TrapData GetTrapData(Vector3Int trapPos)
    {
        if (activeTraps.ContainsKey(trapPos)) return activeTraps[trapPos].trapData;
        return null;
    }

    public void DisarmTrap(Vector3Int trapPos)
    {

        if (activeTraps.TryGetValue(trapPos, out ActiveTrapInstance trapInterface))
        {
            SpriteRenderer sprite = trapInterface.gameObject.GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                Color sColor = sprite.color;
                sColor.a = 0.5f;
                sprite.color = sColor;
            }
            activeTraps.Remove(trapPos);
        }
    }
    public void AddTrap(Vector3Int trapPos, ActiveTrapInstance trapData)
    {
        if (trapData != null) activeTraps.Add(trapPos, trapData);
    }
    public void RemoveTrap(Vector3Int trapPos)
    {
        activeTraps.Remove(trapPos);
    }

    private void OnDrawGizmos()
    {
        if (activeTraps == null || activeTraps.Count == 0) return;

        Gizmos.color = Color.red;

        foreach (var trap in activeTraps)
        {
            Gizmos.DrawCube(trap.Value.gameObject.transform.position, Vector3.one);
        }
    }
}
