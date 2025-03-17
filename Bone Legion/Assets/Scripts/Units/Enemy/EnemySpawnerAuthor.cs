using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class EnemySpawnerAuthor : MonoBehaviour
{
    public GameObject enemyPrefab;
	public int spawnAmount;
	public Vector2Int mapBounds;
}

public class EnemySpawnerBaker : Baker<EnemySpawnerAuthor>
{
	public override void Bake(EnemySpawnerAuthor authoring)
	{
		Entity entity = GetEntity(TransformUsageFlags.None);
		AddComponent(entity, new EnemySpawner
		{
			enemyEntity = GetEntity(authoring.enemyPrefab, TransformUsageFlags.Dynamic),
			spawnAmount = authoring.spawnAmount,
			currentAmount = 0,
			mapBounds = new int2(authoring.mapBounds.x, authoring.mapBounds.y)
		});
	}
}

public struct EnemySpawner : IComponentData
{
    public Entity enemyEntity;
    public int spawnAmount;
    public int currentAmount;
	public int2 mapBounds;
}