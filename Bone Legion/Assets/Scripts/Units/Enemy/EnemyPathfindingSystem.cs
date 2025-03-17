using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateAfter(typeof(EnemySpawnerSystem))]
partial struct EnemyPathfindingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
		EntityCommandBuffer.ParallelWriter ecb = GetEntityCommandBuffer(ref state);

		float3 movementDirection = new float3(0, 1, 1);
        float deltaTime = SystemAPI.Time.DeltaTime;

        new MoveEnemyUnit
        {
            ecb = ecb,
            movementDirection = movementDirection,
            deltaTime = deltaTime,
        }.ScheduleParallel();
	}

	private EntityCommandBuffer.ParallelWriter GetEntityCommandBuffer(ref SystemState state)
	{
		var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
		EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
		return ecb.AsParallelWriter();
	}

    [BurstCompile]
    public partial struct MoveEnemyUnit : IJobEntity
    {
		public EntityCommandBuffer.ParallelWriter ecb;
		public float3 movementDirection;
		public float deltaTime;

        private void Execute([ChunkIndexInQuery] int chunkIndex, EnemyWalkAspect enemy, ref LocalTransform enemyTransform)
        {
            enemy.Walk(deltaTime, ref enemyTransform);
        }
    }
}