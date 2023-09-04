using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState
{
    Menu,
    GameInit,
    Gameplay,
    WaveUp,
    OpenStats,
    GameOver,
    Win,
    Lose,
}

public class GameManager : MonoBehaviour
{
    public static event Action<GameState> OnGameStateChange;

    public static GameManager Instance;
    PlayerInputAction playerInput;

    public GameState currentState;

    [SerializeField] float delayToGameplay;
    [SerializeField] float delayToResult;

    bool isWin;

    private void Awake()
    {
        Instance = this;

        CharController.OnPlayerDead += PlayerDeadHandler;
        //CharController.OnLevelUp += LevelUpHandler;
        EnemySpawner.OnLevelComplete += LevelCompleteHandler;

        playerInput = new PlayerInputAction();
        playerInput.Enable();
        playerInput.Char.OpenStats.performed += OpenStatsHandler;
    }

    private void Start()
    {
        UpdateGameState(GameState.Menu);
    }

    public void UpdateGameState(GameState newState)
    {
        currentState = newState;

        switch(currentState)
        {
            case GameState.Menu:
                MenuHandler();
                break;
            case GameState.GameInit:
                GameInitHandler();
                break;
            case GameState.Gameplay:
                GameplayHandler();
                break;
            case GameState.GameOver:
                GameOverHandler();
                break;

        }
        OnGameStateChange(currentState);
    }

    void OpenStatsHandler(InputAction.CallbackContext context)
    {
        Time.timeScale = 0;
        UpdateGameState(GameState.OpenStats);
    }

    /*void LevelUpHandler(int level, bool isMax)
    {
        Time.timeScale = 0;
        UpdateGameState(GameState.LevelUp);
    }*/

    void MenuHandler()
    {
        Time.timeScale = 0;
    }

    void GameInitHandler()
    {
        StartCoroutine(GameInitCo());
    }

    IEnumerator GameInitCo()
    {
        Time.timeScale = 1;
        yield return new WaitForSeconds(delayToGameplay);

        UpdateGameState(GameState.Gameplay);
    }

    void GameplayHandler()
    {
        Time.timeScale = 1;
    }

    void GameOverHandler()
    {
        StartCoroutine(GameOverCo());
    }

    IEnumerator GameOverCo()
    {
        yield return new WaitForSeconds(delayToResult);

        if (isWin)        
            UpdateGameState(GameState.Win);       
        else
            UpdateGameState(GameState.Lose);

    }

    public void PlayButton()
    {
        UpdateGameState(GameState.GameInit);

    }

    void PlayerDeadHandler()
    {
        UpdateGameState(GameState.GameOver);

        isWin = false;
    }

    void LevelCompleteHandler()
    {
        UpdateGameState(GameState.GameOver);

        isWin = true;
    }
}
