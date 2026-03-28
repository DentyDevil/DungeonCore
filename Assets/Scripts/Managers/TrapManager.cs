using System.Collections.Generic;
using UnityEngine;

public class TrapManager : MonoBehaviour
{
    public static TrapManager instance;
    private readonly Dictionary<Vector3Int, ActiveTrapInstance> activeTraps = new();
    private readonly Dictionary<Vector3Int, ActiveTrapInstance> detectedTraps = new();
    private readonly Dictionary<Vector3Int, ActiveTrapInstance> brokenTraps = new();

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
                    if (trapInstance.currentDetectDifficulty < enemyData.mechanicalDetectedSkill) return trapInstance.trapData;
                    break;
                case TrapClass.Mechanical:
                    if (trapInstance.currentDetectDifficulty < enemyData.mechanicalDetectedSkill)
                    {
                        Debug.LogWarning($"Ловушка обнаружена. Незаметность ловушки - {trapInstance.currentDetectDifficulty}. Навык внимательности юнита - {enemyData.mechanicalDetectedSkill}");
                        return trapInstance.trapData;
                    }
                    break;
                case TrapClass.Magic:
                    if (trapInstance.currentDetectDifficulty < enemyData.magicDetectedSkill) return trapInstance.trapData;
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
        else if (brokenTraps.ContainsKey(trapPos)) return true;
     
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
            Collider2D collider2D = trapInterface.gameObject.GetComponent<Collider2D>();
            if (sprite != null && collider2D != null)
            {
                Color sColor = sprite.color;
                sColor.a = 0.5f;
                sprite.color = sColor;
                collider2D.enabled = false;
            }
            TrapsEvents.OnTrapDisarmedEvent(trapPos);
            brokenTraps.Add(trapPos, trapInterface);
            activeTraps.Remove(trapPos);
        }
    }
    public void AddTrap(Vector3Int trapPos, ActiveTrapInstance trapData)
    {
        if (trapData != null) 
        { 
            activeTraps.Add(trapPos, trapData);
            InitiateTrap(trapPos);
        }
    }

    void InitiateTrap(Vector3Int trapPos)
    {
        if(activeTraps.TryGetValue(trapPos, out ActiveTrapInstance trapInterface) && trapInterface != null)
        {
            trapInterface.currentDetectDifficulty = trapInterface.trapData.detectDifficulty;
        }
    }

    public void RemoveTrap(Vector3Int trapPos)
    {
        if(activeTraps.ContainsKey(trapPos)) activeTraps.Remove(trapPos);
        else if(brokenTraps.ContainsKey(trapPos)) brokenTraps.Remove(trapPos);
    }

    public void UpdateStateTrap(Vector3Int trapPos)
    {
        if (!brokenTraps.ContainsKey(trapPos) && activeTraps.TryGetValue(trapPos, out ActiveTrapInstance currTrapInterface) && currTrapInterface != null)
        {
            currTrapInterface.currentUsesBeforeBroken++;
            if(currTrapInterface.currentDetectDifficulty == currTrapInterface.trapData.detectDifficulty)
            {
                currTrapInterface.currentDetectDifficulty = Random.Range(1, 10);
                SpriteRenderer sprite = currTrapInterface.gameObject.GetComponent<SpriteRenderer>();
                sprite.color = Color.green;
                detectedTraps.Add(trapPos, currTrapInterface);
            }

            if (currTrapInterface.currentUsesBeforeBroken >= currTrapInterface.trapData.maxUsesBeforBroken)
            {
                DisarmTrap(trapPos);
            }
            Debug.LogWarning($"Ловушка обновилась срабатываний до поломки - {currTrapInterface.currentUsesBeforeBroken}/{currTrapInterface.trapData.maxUsesBeforBroken}");
        }
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
