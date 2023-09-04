using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Panels")]
    public GameObject menuPanel;
    public GameObject readyPanel;
    public GameObject gameplayPanel;
    public GameObject upgradePanel;
    public GameObject resultPanel;

    [Header("UI Status")]
    public Slider HpBar;
    public TextMeshProUGUI HpText;
    public Slider ExpBar;
    public TextMeshProUGUI ExpText;
    public TextMeshProUGUI LevelText;
    public TextMeshProUGUI WaveText;

    [Header("Upgrade")]
    public TextMeshProUGUI upgradeNotif;

    [Header("Result")]
    public TextMeshProUGUI resultText;

    private void Awake()
    {
        Instance = this;

        GameManager.OnGameStateChange += GameStateChangeHandler;
        CharController.OnHpChange += HpChangeHandler;
        CharController.OnExpChange += ExpChangeHandler;
        CharController.OnLevelUp += LevelChangeHandler;
        EnemySpawner.OnWaveChange += WaveChangeHandler;
        UpgradeManager.OnUpgradePointChange += UpgradePointChangeHandler;
    }

    void GameStateChangeHandler(GameState state)
    {
        menuPanel.SetActive(state == GameState.Menu);
        readyPanel.SetActive(state == GameState.GameInit);
        gameplayPanel.SetActive(state == GameState.GameInit || state == GameState.Gameplay);
        upgradePanel.SetActive(state == GameState.OpenStats);
        resultPanel.SetActive(state == GameState.Win || state == GameState.Lose);

        if (state == GameState.Win)
            resultText.text = "You Win !!";
        else if(state == GameState.Lose)
            resultText.text = "You Lose !!";
    }

    void HpChangeHandler(float currentHP, float maxHP)
    {
        HpBar.value = currentHP / maxHP;
        HpText.text = $"HP = {currentHP.ToString("F1")} / {maxHP.ToString("F1")}";
    }

    void ExpChangeHandler(float currentEXP, float maxEXP)
    {
        ExpBar.value = currentEXP / maxEXP;
        ExpText.text = $"EXP = {currentEXP.ToString("F1")} / {maxEXP.ToString("F1")}";
    }

    void LevelChangeHandler(int level, bool isMax)
    {
        if(isMax)
            LevelText.text = "MAX";
        else
            LevelText.text = level.ToString();
    }

    void WaveChangeHandler(int wave, int maxWave)
    {
        WaveText.text = "Wave\n" + wave.ToString() + " / " + maxWave.ToString();
    }

    void UpgradePointChangeHandler(int point)
    {
        upgradeNotif.gameObject.SetActive(point > 0);
    }
}
