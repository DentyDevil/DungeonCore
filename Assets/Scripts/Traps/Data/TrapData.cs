using System.Collections.Generic;
using UnityEngine;

public enum TrapClass
{
    Static,
    Mechanical,
    Magic
}

[CreateAssetMenu(fileName = "New Trap", menuName = "Traps/Traps Data")]
public class TrapData : ScriptableObject
{
    [Header("Базовые значения")]
    public float damage;
    public float coolDownTime;
    public float detectDifficulty;
    public float disarmDifficulty;
    public float timeToBuild;
    public float timeToDisarm;
    public TrapClass trapClass;

    [Header("Ссылки")]
    public GameObject trapPrefab;

    public List<ResourceCost> resourceCost = new List<ResourceCost>();
    public Sprite previewSprite;
}
