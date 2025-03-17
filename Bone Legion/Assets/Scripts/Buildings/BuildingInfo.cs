using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class BuildingInfo : MonoBehaviour
{
    [SerializeField]
    private HealthData healthData;

    [SerializeField]
    private Image healthBar;

    private Grid grid;
    private Tilemap placedBuildingsTilemap;
    private FlowfieldPathfinding flowfieldPathfinding;

    public Vector2Int size;

    public Type buildingType;

    private float currentHealth;

    private bool destroyed = false;

    private bool regeneratingHealth = false;

    private void Start()
    {
        currentHealth = healthData.maxHealth;
        placedBuildingsTilemap = GameObject.Find("Grid/Placed Buildings").GetComponent<Tilemap>();
        flowfieldPathfinding = GameObject.Find("_Pathfinding").GetComponent<FlowfieldPathfinding>();
    }
    public void TakeDamage(float damage)
    {
        if (destroyed) return;
        currentHealth -= damage;
        if (regeneratingHealth == false) 
        {
            StartCoroutine(RegenerateHealth());
            regeneratingHealth = true; 
        }
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            if (buildingType != Type.Wall)
                healthBar.fillAmount = 0;
            destroyed = true;
            BuildingDestroyed();
            return;
        }

        if (buildingType != Type.Wall)
            healthBar.fillAmount = currentHealth / healthData.maxHealth;
    }

    IEnumerator RegenerateHealth()
    {
        while (regeneratingHealth)
        {
            currentHealth += healthData.regeneration;

            if (currentHealth > healthData.maxHealth)
            {
                currentHealth = healthData.maxHealth;
                StopCoroutine(RegenerateHealth());
                regeneratingHealth = false;
            }
            yield return new WaitForSeconds(1);

        }
    }
	public void BuildingDestroyed()
    {
        foreach (Vector3Int cell in flowfieldPathfinding.buildingCells[gameObject])
        {
			placedBuildingsTilemap.SetTile(cell, null);
            flowfieldPathfinding.buildingMap.Remove(cell);
		}

		flowfieldPathfinding.buildingCells.Remove(gameObject);

        flowfieldPathfinding.mapChanged++;

        Destroy(gameObject);

        if (buildingType == Type.Castle)
        {
            GameManager.Instance.UpdateGameState(GameState.GameEnd);
        }
        else if (buildingType == Type.GoldMine)
        {
            ResourceManager.Instance.RemoveGoldMineBuilding();
        }
    }
}

public enum Type
{
    Castle,
    GoldMine,
    Tower,
    Wall,
    Archer
}