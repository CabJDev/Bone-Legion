// Author: Hope Oberez (H.O)
using UnityEngine;
using System;
using TMPro;

// Handles the overall flow and state of the game, central point for other managers, 
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private SoundFXManager soundFXManager;
    [SerializeField] AudioClip gameOverClip;

    [SerializeField]
    private GameObject loadingScreen;

    [SerializeField]
    private GameObject endingScreen;

    [SerializeField]
    private TimeManager timeManager;


    [SerializeField]
    private UnitController unitController;

    [SerializeField]
    private BuildingManager buildingManager;

    [SerializeField]
    private TMP_Text FinalDaysText;
    [SerializeField]
    private TMP_Text FinalHoursText;
    [SerializeField]
    private TMP_Text FinalMinutesText;

    public GameState state;

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
        endingScreen.SetActive(false);
        UpdateGameState(GameState.LoadGame);
        soundFXManager = SoundFXManager.Instance;
        Debug.Log("Game State: " + state);
    }

    public void UpdateGameState(GameState newState)
    {

        GameState previousState = state;
        state = newState;
        Debug.Log("Game State: " + state);

        switch (state)
        {
            case GameState.LoadGame:
                HandleLoadGame();
                break;
            case GameState.GameStart:
                HandleGameStart();
                break;
            case GameState.GamePaused:
                Time.timeScale = 0;
                break;
            case GameState.GameRun:
                Time.timeScale = 1;
                break;
            case GameState.GameEnd:
                HandleGameEnd();
                break;
            default:
                throw new ArgumentOutOfRangeException();

        }
    }

    private void HandleLoadGame()
    {
        loadingScreen.SetActive(true);

    }
    private void HandleGameStart()
    {
        loadingScreen.SetActive(false);
        timeManager.StartTime();
        UpdateGameState(GameState.GameRun);
    }

    private void HandleGameEnd()
    {
        UpdateGameState(GameState.GamePaused);
        FinalDaysText.text = timeManager.Days.ToString() + " Day(s)";
        FinalHoursText.text = timeManager.Hours.ToString() + " Hour(s)";
        FinalMinutesText.text = timeManager.Minutes.ToString() + " Minute(s)";
        timeManager.EndTime();
        endingScreen.SetActive(true);
        if (timeManager.Days >= 4)
			soundFXManager.PlaySound(gameOverClip, new Vector3(0, 0, 0), 1f);
		soundFXManager.playSounds = false;
    }
}



public enum GameState
{
    LoadGame,
    GameStart,
    GamePaused,
    GameRun,
    GameEnd
}
