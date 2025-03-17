using UnityEngine;

[CreateAssetMenu(fileName = "ResourceCostData", menuName = "ScriptableObjects/ResourceCostData")]

public class ResourceCostData : ScriptableObject
{
    public int archerCost;
    public int goldMineCost;
    public int towerCost;
    public int wallCost;
}
