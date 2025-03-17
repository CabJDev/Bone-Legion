using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public readonly partial struct EnemyWalkAspect : IAspect
{
    private readonly RefRO<EnemyInfo> enemyInfo;

    public void Walk(float deltaTime, ref LocalTransform enemyTransform)
    {
        enemyTransform.Position += new float3(0, 1, 1) * 1 * deltaTime;
    }
}
