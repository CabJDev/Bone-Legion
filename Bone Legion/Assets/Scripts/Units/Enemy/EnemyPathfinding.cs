using System.Collections.Generic;
using UnityEngine;

public class EnemyPathfinding : MonoBehaviour
{
	SoundFXManager soundFXManager;

    [SerializeField] private FlowfieldPathfinding pathFinding;
	[SerializeField] private EnemySpawning enemySpawning;

	public float moveModifier;

	private Queue<Transform> enemyUnits;
	private Dictionary<Transform, float> unitDamageCooldown;
	private List<GameObject> enemyToBeRemoved;

	[SerializeField] private float unitDamage;
	[SerializeField] private float unitCooldown;

	private float walkSoundCooldown = 0.1f;
	private float attackSoundCooldown = 0.1f;

	private void Start()
	{
		soundFXManager = SoundFXManager.Instance;

		pathFinding = GameObject.Find("_Pathfinding").GetComponent<FlowfieldPathfinding>();
		enemyUnits = new Queue<Transform>();
		unitDamageCooldown = new Dictionary<Transform, float>();
		enemyToBeRemoved = new List<GameObject>();
	}

	private void Update()
	{
		walkSoundCooldown -= Time.deltaTime;
		attackSoundCooldown -= Time.deltaTime;

		if (pathFinding.gridInitialized)
		{
			for (int i = 0; i < 10000; ++i)
			{
				if (enemyUnits.Count == 0) continue;
				Transform enemy = enemyUnits.Dequeue();
				if (enemyToBeRemoved.Contains(enemy.gameObject))
				{
					enemyToBeRemoved.Remove(enemy.gameObject);
					unitDamageCooldown.Remove(enemy);
					continue;
				}

				Vector3 enemyPos = enemy.position;
				Vector3 xyPos = new Vector3(enemyPos.x, enemyPos.y, 0);
				Vector3 newPos = pathFinding.GetDirection(xyPos) * (moveModifier * enemySpawning.enemyTransforms.Count) * Time.deltaTime;
				enemy.Translate(newPos);
				if (walkSoundCooldown <= 0f)
				{
					soundFXManager.PlaySound(SoundType.EnemyWalk, enemy.position, 1f);
					walkSoundCooldown = 0.1f;
				}

				GameObject attackingObject = pathFinding.Attacking(xyPos);
				if (attackingObject != null && attackingObject.GetComponent<BuildingInfo>() != null)
				{
					if (unitDamageCooldown[enemy] <= 0)
					{
						attackingObject.GetComponent<BuildingInfo>().TakeDamage(unitDamage * enemySpawning.enemyTransforms.Count);
						unitDamageCooldown[enemy] = unitCooldown;
						if (attackSoundCooldown <= 0f)
						{
							soundFXManager.PlaySound(SoundType.EnemyAttack, enemy.position, 1f);
							attackSoundCooldown = 0.1f;
						}
					}
					else unitDamageCooldown[enemy] -= Time.deltaTime;
				}
					
				enemyUnits.Enqueue(enemy);
			}
		}
	}

	public void AddEnemyUnit(GameObject enemyUnit)
	{
		enemyUnits.Enqueue(enemyUnit.transform);
		unitDamageCooldown.Add(enemyUnit.transform, unitCooldown);
	}
	public void RemoveEnemyUnit(GameObject enemyUnit) { enemyToBeRemoved.Add(enemyUnit); }
}
