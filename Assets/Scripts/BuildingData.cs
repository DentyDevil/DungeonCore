using System.Collections.Generic;
using System;
using UnityEngine;

[System.Serializable]
public class ResourceCost
{
    public int count;
    public ResourceData resourceData;
}

[CreateAssetMenu(fileName = "New Building", menuName = "Buildings/Building Data")]
public class BuildingData : ScriptableObject
{
    public Sprite previewSprite;
    public GameObject actualPrefab;
    public float timeToBuild;

    public List<ResourceCost> resourceCost = new List<ResourceCost>();
}
