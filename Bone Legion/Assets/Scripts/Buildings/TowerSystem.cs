using UnityEngine;

public class TowerSystem : MonoBehaviour
{
    SoundFXManager soundFXManager;
    EnemySpawning spawner;

    float cooldown = 0.1f;
    float radius = 4f;

    Vector2 currentPos;

    private void Start()
    {
        soundFXManager = SoundFXManager.Instance;

        spawner = GameObject.Find("_EnemyUnits").GetComponent<EnemySpawning>();
    }

    private void Update()
    {
        if (cooldown > 0)
        {
            cooldown -= Time.deltaTime;
            if (currentPos == null || (currentPos.x != transform.position.x && currentPos.y != transform.position.y))
                currentPos = new Vector2(transform.position.x, transform.position.y);
            return;
        }

        Transform closestUnit = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < spawner.enemyTransforms.Count; ++i)
        {
            Transform enemyUnit = spawner.enemyTransforms[i];
            Vector2 pos = new Vector2(enemyUnit.position.x, enemyUnit.position.y);
            float distanceToBuilding = (currentPos - pos).sqrMagnitude;
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
