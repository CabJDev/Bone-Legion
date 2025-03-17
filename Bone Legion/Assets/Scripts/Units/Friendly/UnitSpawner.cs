// Author: Hope Oberez (H.O)
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;


public class UnitSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject spawnedUnits;

    [SerializeField]
    private TerrainGenerator terrainGenerator;

    private InputAction pointer;

    private Unit selectedUnit;

    private Unit spawnedUnit;

    private void Start()
    {
        enabled = false;
        pointer = InputSystem.actions.FindAction("Pointer");

    }

    private void Update()
    {
        if (!ResourceManager.Instance.CanBuy(Type.Archer))
        {
            UIManager.Instance.NotEnoughGoldNotification();
            enabled = false;
            return;
        }

        if (Input.GetMouseButtonDown(0) && !UIManager.Instance.IsMouseOverUI())
        {
            Vector2 pointerPos = pointer.ReadValue<Vector2>();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(pointerPos.x, pointerPos.y, 0));

            SpawnUnit();
            spawnedUnit.MoveTo(worldPos);

            UIManager.Instance.RecruitmentSuccess();
            ResourceManager.Instance.SpendGoldCoins(selectedUnit.unitType);
            UIManager.Instance.PlayGame();
        }

        if (Input.GetMouseButtonDown(0)&& UIManager.Instance.IsMouseOverUI())
        {
            enabled = false;
            UIManager.Instance.RecruitmentCanceled();
        }

        if (Input.GetMouseButtonDown(1))
        {
            enabled = false;
            UIManager.Instance.RecruitmentCanceled();
        }
    }
    public void SpawnUnit() // Simple spawner for now to test out ui button
    {

        Vector3 castlePosition = terrainGenerator.GetCastlePosition();

        Vector2 spawnPosition = new Vector2(castlePosition.x + 1, castlePosition.y +1);

        Vector3 spawnPoint = new Vector3(spawnPosition.x, spawnPosition.y, 0);
        spawnedUnit = Instantiate(selectedUnit, spawnPoint, Quaternion.identity);
        spawnedUnit.transform.SetParent(spawnedUnits.transform);
    }

    public void SetSelectedUnit(Unit unit)
    {
        selectedUnit = unit;
        enabled = true;

    }

}
