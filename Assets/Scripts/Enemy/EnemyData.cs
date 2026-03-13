using UnityEngine;
public enum EnemyClass
{
    Wizard,
    Archer,
    Warrior,
}

[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemy/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Базовые параметры")]
    public float healthPoints;
    public float speed;
    public float timeBetweenDigging;
    public int damageTile;
    public float timeToOpenDoor;
    public float timeToScanRoom;
    public EnemyClass enemyClass;

    [Header("Боевые параметры")]
    public float damage;
    public float attackCoolDown;

    [Header("Навыки работы с ловушками")]
    public float magicSkill;
    public float mechanicalSkill;

    [Header("Навыки обнаружения ловушек")]
    public float magicDetectedSkill;
    public float mechanicalDetectedSkill;


    [Header("Ссылки")]
    public GameObject enemyPrefab;
    public Sprite previewSprite;
}
