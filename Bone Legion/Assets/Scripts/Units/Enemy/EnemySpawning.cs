using System.Collections.Generic;
using UnityEngine;

public class EnemySpawning : MonoBehaviour
{
    SoundFXManager soundFXManager;

    [SerializeField] AudioClip[] waveVoiceLines;

    List<GameObject> activeEnemies = new List<GameObject>();
    public List<Transform> enemyTransforms = new List<Transform>();
    List<GameObject> inactiveEnemies = new List<GameObject>();

    [SerializeField]
    TerrainGenerator terrain;

    [SerializeField] private EnemyPathfinding enemyPathfinding;

    [SerializeField]
    int maxEnemies;

    public int wave = 0;
    private int waveAmount;
    public int currentWaveCount = 0;

    [SerializeField]
    GameObject[] enemies;

    [SerializeField] TimeManager timeManager;

    Vector2 maxBounds;

    public bool instantiatingEnemies = true;
    public bool waveFinished = false;

    float attackSoundCooldown = 0.1f;

    private void Start()
    {
        soundFXManager = SoundFXManager.Instance;
        maxBounds = terrain.GetMapSize();
    }

    private void Update()
    {
        attackSoundCooldown -= Time.deltaTime;
        if (instantiatingEnemies)
        {
            for (int i = 0; i < 1000; ++i)
            {
                GameObject enemy = Instantiate(enemies[0]);
                enemy.transform.SetParent(transform);
                enemy.SetActive(false);
                inactiveEnemies.Add(enemy);

                if (inactiveEnemies.Count == maxEnemies)
                {
                    instantiatingEnemies = false;
                    break;
                }
            }
        } 
        else
        {
            if (EnemiesRaiding() && currentWaveCount < 10 * Mathf.Pow(10, wave))
            {
                for (int i = 0; i < 1000; ++i)
                {
                    GameObject enemy = GetEnemy();

                    if (enemy != null)
                        enemy.transform.position = GetPosition();

                    ++currentWaveCount;
                    if (currentWaveCount >= 10 * Mathf.Pow(10, wave)) break;
                }
            }
        }
    }

    private bool EnemiesRaiding()
    {
        if (timeManager.Days == 0 && timeManager.Hours >= 18)
        {
            if (!waveFinished)
            {
                wave++;
                soundFXManager.PlaySound(waveVoiceLines[wave - 1], new Vector3(0, 0, 0), 1f);
                waveFinished = true;
            }
            return true;
        }
        else if (timeManager.Days > 0 && (timeManager.Hours >= 18 || timeManager.Hours < 6))
        {
            if (!waveFinished)
            {
                wave++;
                if (wave - 1 < waveVoiceLines.Length)
					soundFXManager.PlaySound(waveVoiceLines[wave - 1], new Vector3(0, 0, 0), 1f);
				waveFinished = true;
            }
            return true;
        }

        waveFinished = false;
        return false;
    }

    public void KillEnemies()
    {
        if (!instantiatingEnemies)
        {
            for (int i = 0; i < 1000; ++i)
            {
                if (currentWaveCount <= 0) break;
                RemoveEnemy(activeEnemies[0]);
                --currentWaveCount;
            }
        }
    }

    private GameObject GetEnemy()
    {
        if (inactiveEnemies.Count > 0)
        {
            GameObject enemy = inactiveEnemies[0];
            inactiveEnemies.Remove(enemy);
            activeEnemies.Add(enemy);
            enemyPathfinding.AddEnemyUnit(enemy);

            enemyTransforms.Add(enemy.transform);

            enemy.SetActive(true);

            return enemy;
        }

        return null;
    }

    public void RemoveEnemy(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            if (attackSoundCooldown <= 0f)
            {
				soundFXManager.PlaySound(SoundType.EnemyDie, enemy.transform.position, 1f);
                attackSoundCooldown = 0.1f;
			}
			
			activeEnemies.Remove(enemy);
            inactiveEnemies.Add(enemy);
            enemyPathfinding.RemoveEnemyUnit(enemy);

            enemyTransforms.Remove(enemy.transform);

            enemy.SetActive(false);
        }
    }

    private Vector3 GetPosition()
    {
        float posX = Random.Range(0.25f, maxBounds.x - 0.25f);
        int up = Random.Range(0, 2);
        float posY = 0;
        if (up == 0)
            posY = Mathf.Abs((posX % maxBounds.x / 2) - (maxBounds.x / 4));
        else
            posY = Mathf.Abs(((posX + (maxBounds.x / 2)) % maxBounds.x / 2) - (maxBounds.x / 4)) + ((maxBounds.x / 4) - 0.25f);

            return new Vector3(posX - (maxBounds.x / 2), posY, posY);
    }
}
