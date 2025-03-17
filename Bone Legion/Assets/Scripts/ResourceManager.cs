using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    // Simplified version of resource manager for prototype
    public static ResourceManager Instance { get; private set; }

    // buildings
    private int totalGoldMineBuildings;

    // resources
    private float totalGoldCoins;

    [SerializeField]
    private ResourceCostData costData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }


    void Start()
    {
        totalGoldCoins = 50;
        totalGoldMineBuildings = 0;
        StartCoroutine(generateGold());
    }
    IEnumerator generateGold()
    {
        while (true)
        {
            yield return new WaitForSeconds(2.5f);
            totalGoldCoins += totalGoldMineBuildings * 1;
        }
    }

    private void Update()
    {
        UIManager.Instance.UpdateTotalGoldText(totalGoldCoins);
    }

    public void AddGoldMineBuilding()
    {
        totalGoldMineBuildings += 1;
    }
    public void RemoveGoldMineBuilding()
    {
        totalGoldMineBuildings -= 1;
    
    }

    public bool CanBuy(Type type)
    {
        //return costData.archerCost <= totalGoldCoins;
        switch (type)
        {
            case Type.Archer:
                return costData.archerCost <= totalGoldCoins;
            case Type.Wall:
                return costData.wallCost <= totalGoldCoins;
            case Type.Tower:
                return costData.towerCost <= totalGoldCoins;
            case Type.GoldMine:
                return costData.goldMineCost <= totalGoldCoins;
            default:
                throw new ArgumentOutOfRangeException();

        }
    }

    public void SpendGoldCoins(Type type)
    {
        //return costData.archerCost <= totalGoldCoins;
        switch (type)
        {
            case Type.Archer:
                totalGoldCoins -= costData.archerCost;
                break;
            case Type.Wall:
                totalGoldCoins -= costData.wallCost;
                break;
            case Type.Tower:
                totalGoldCoins -= costData.towerCost;
                break;
            case Type.GoldMine:
                totalGoldCoins -= costData.goldMineCost;
                break;
            default:
                throw new ArgumentOutOfRangeException();

        }
        Debug.Log(totalGoldCoins);
    }
}
