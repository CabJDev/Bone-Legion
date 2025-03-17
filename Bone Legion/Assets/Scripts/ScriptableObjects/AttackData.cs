using UnityEngine;

[CreateAssetMenu(fileName = "AttackData", menuName = "ScriptableObjects/AttackData")]

public class AttackData : ScriptableObject
{
    public float attackDamage;
    public float attackSpeed;
    public float attackRange;
    public float watchRange;
}
