using Unity.Entities;

public struct EnemyInfo : IComponentData
{
    public float maxHealth;
    public float health;
    public float speed;
    public float strength;
    public int range;
}
