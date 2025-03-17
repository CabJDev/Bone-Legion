// Author: Hope Oberez (H.O)
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    [SerializeField]
    private HealthData healthData;

    [SerializeField]
    private Image healthBar;

    private float currentHealth;

    private GameObject selectedGameObject;
    private MovePositionDirect movePosition;
    private Transform transformPosition;

    private GameObject prefab;

    public Type unitType;

    private bool regeneratingHealth;

    private void Awake()
    {
        selectedGameObject = transform.Find("Selected").gameObject;
        movePosition = GetComponent<MovePositionDirect>();
        transformPosition = GetComponent<Transform>();
        SetSelectedVisible(false);
    }

    public void Start()
    {
        UnitController.Instance.allUnitsList.Add(this);
    }

    public void SetSelectedVisible(bool visible)
    {
        selectedGameObject.SetActive(visible);
    }

    public void MoveTo(Vector3 targetPosition)
    {
        movePosition.SetMovePosition(targetPosition);
    }

    public Vector3 GetCurrentPosition()
    {
        return transformPosition.position;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (regeneratingHealth == false)
        {
            StartCoroutine(RegenerateHealth());
            regeneratingHealth = true;
        }
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            healthBar.fillAmount = 0;
            Destroy(this);
            return;
        }
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

    private void OnDestroy()
    {
        UnitController.Instance.allUnitsList.Remove(this);
    }

}
