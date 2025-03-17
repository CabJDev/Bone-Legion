using UnityEngine;

[CreateAssetMenu(fileName = "HealthData", menuName = "ScriptableObjects/HealthData")]
public class HealthData : ScriptableObject
{
    public float maxHealth;
    public float regeneration;
}
