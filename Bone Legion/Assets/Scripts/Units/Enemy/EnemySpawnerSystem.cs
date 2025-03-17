using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

[BurstCompile]
partial struct EnemySpawnerSystem : ISystem
{
	Random rand;

    public void OnCreate(ref SystemState state)
    {
		rand = new Random(1000);
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		EntityCommandBuffer.ParallelWriter ecb = GetEntityCommandBuffer(ref state);

		new ProcessEnemySpawnerJob
		{
			ecb = ecb,
			rand = rand
		}.ScheduleParallel();
	}

    private EntityCommandBuffer.ParallelWriter GetEntityCommandBuffer(ref SystemState state)
	{
		var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
		EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
		return ecb.AsParallelWriter();
	}

	[BurstCompile]
	public partial struct ProcessEnemySpawnerJob : IJobEntity
	{
		public EntityCommandBuffer.ParallelWriter ecb;
		public Random rand;

		private void Execute([ChunkIndexInQuery] int chunkIndex, ref EnemySpawner enemySpawner)
		{
			for (int i = 0; i < enemySpawner.spawnAmount; ++i)
			{
				if (enemySpawner.currentAmount < enemySpawner.spawnAmount)
				{
					Entity newEntity = ecb.Instantiate(chunkIndex, enemySpawner.enemyEntity);
					ecb.SetComponent(chunkIndex, newEntity, LocalTransform.FromPosition(GetPosition(enemySpawner.mapBounds)));
					ecb.AddComponent<EnemyInfo>(chunkIndex, newEntity);
					enemySpawner.currentAmount++;
				}
				else break;
			}
		}

		private float3 GetPosition(int2 mapBounds)
		{
			float posX = rand.NextFloat(0.25f, mapBounds.x - 0.25f);
			bool up = rand.NextBool();
			float posY = 0;
			if (!up)
				posY = math.abs((posX % mapBounds.x / 2) - (mapBounds.x / 4));
			else
				posY = math.abs(((posX + (mapBounds.x / 2)) % mapBounds.x / 2) - (mapBounds.x / 4)) + ((mapBounds.x / 4) - 0.25f);

			return new float3(posX - (mapBounds.x / 2), posY, posY);
		}
	}
}
