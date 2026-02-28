using UnityEngine;

[CreateAssetMenu(fileName = "New Resource", menuName = "Resources/Resource Data")]
public class ResourceData : ScriptableObject
{
    [Header("Основное")]
    public string displayName;
    public Sprite icon;
    public GameObject prefab;
}
