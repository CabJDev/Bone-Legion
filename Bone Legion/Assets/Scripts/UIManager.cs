// Author: Hope Oberez (H.O)
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }


    [SerializeField]
    private GameObject[] hideAtStartObjects;

    [SerializeField]
    private Unit archerPrefab;

    [SerializeField]
    private UnitSpawner unitSpawner;

    [SerializeField]
    private UnitController unitController;

    [SerializeField]
    private TMP_Text notificationText;

    [SerializeField]
    private TMP_Text totalGoldText;

    public bool newNotification;

    [SerializeField]
    private GameObject pauseButton;

    [SerializeField]
    private GameObject playButton;

    [SerializeField]
    private InfoPanel infoPanel;

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

    public void Start()
    {
        Debug.Log("Hiding certain UI components...");       

        foreach (GameObject obj in hideAtStartObjects)
        {
            obj.SetActive(false);
        }
        newNotification = false;
    }

    public void ExitGame() 
    {
        GameManager.Instance.UpdateGameState(GameState.GameEnd);
        SceneManager.LoadScene(1);
    }

    public void BuyArcherUnit()
    {

        newNotification = true;
        notificationText.text = "Select a position";
        unitSpawner.SetSelectedUnit(archerPrefab);
    }

    public void RecruitmentCanceled()
    {
        notificationText.text = "Canceled";
        newNotification = false;
        StartCoroutine(RemoveNotification());
    }

    public void RecruitmentSuccess()
    {
        notificationText.text = "Unit Recruited!";
        newNotification = false;
        StartCoroutine(RemoveNotification());
    }

    IEnumerator RemoveNotification()
    {
        yield return new WaitForSeconds(1.5f);

        if (!newNotification)
        {
            notificationText.text = "";
            newNotification = false;

        }

        StopCoroutine(RemoveNotification());
        newNotification = false;
    }

    public bool IsMouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    public void NotEnoughGoldNotification()
    {
        notificationText.text = "Not enough gold!";
        newNotification = false;
        StartCoroutine(RemoveNotification());
    }
    public void UpdateTotalGoldText(float value) 
    {
        totalGoldText.text = value.ToString();
    }

    public void PauseGame()
    {
        playButton.SetActive(true);
        pauseButton.SetActive(false);

        GameManager.Instance.UpdateGameState(GameState.GamePaused);
    }

    public void PlayGame()
    {
        pauseButton.SetActive(true);
        playButton.SetActive(false);

        GameManager.Instance.UpdateGameState(GameState.GameRun);

    }
}
