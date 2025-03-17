using UnityEngine;

public class UnitAttack : MonoBehaviour
{
    EnemySpawning spawner;

    float cooldown = 0.1f;
    float radius = 9f; // Always a squared number so that it works good with sqrmagnitude

	Transform currentPos;

	private void Start()
	{
		spawner = GameObject.Find("_EnemyUnits").GetComponent<EnemySpawning>();
		currentPos = transform;
	}

	private void Update()
	{
		if (cooldown > 0)
		{
			cooldown -= Time.deltaTime;
			return;
		}

		Transform closestUnit = null;
		float closestDistance = float.MaxValue;

		for (int i = 0; i < spawner.enemyTransforms.Count; ++i)
		{
			Transform enemyUnit = spawner.enemyTransforms[i];
			Vector2 pos = new Vector2(enemyUnit.position.x, enemyUnit.position.y);
			float distanceToBuilding = (new Vector2(currentPos.position.x, currentPos.position.y) - pos).sqrMagnitude;
			if (distanceToBuilding <= radius && distanceToBuilding < closestDistance)
			{
				closestUnit = enemyUnit;
				closestDistance = distanceToBuilding;
			}
		}

		if (closestUnit != null)
		{
			spawner.RemoveEnemy(closestUnit.gameObject);
			cooldown = 0.1f;
		}
	}
}
