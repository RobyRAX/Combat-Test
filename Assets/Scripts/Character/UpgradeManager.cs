using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UpgradeManager : MonoBehaviour
{
    public static event Action<int> OnUpgradePointChange;

    public CharController player;

    [Header("Upgrade UI")]
    public TextMeshProUGUI upgradePointText;
    [SerializeField] int upgradePointPerLevel;
    int _highestUpgradePoint = 0;
    int _upgradePoint = 0;
    public int UpgradePoint
    {
        get { return _upgradePoint; }
        set 
        { 
            _upgradePoint = value;
            upgradePointText.text = UpgradePoint.ToString();
            OnUpgradePointChange(_upgradePoint);           
        }
    }

    [Header("HP Upgrade Point")]   
    [SerializeField] float maxHpPerUpgrade;
    [SerializeField] float hpToBeAdded;
    public Slider maxHpUI;
    public TextMeshProUGUI hpText;

    [Header("Damage Upgrade Point")]
    [SerializeField] float maxDamagePerUpgrade;
    public Slider maxDamageUI;
    public TextMeshProUGUI damageText;

    [Header("Critical Upgrade Point")]
    [SerializeField] float maxCritPerUpgrade;
    public Slider maxCritUI;
    public TextMeshProUGUI critText;

    [Header("Movement Upgrade Point")]
    [SerializeField] float maxMovePerUpgrade;
    public Slider maxMoveUI;
    public TextMeshProUGUI moveText;

    private void Awake()
    {
        GameManager.OnGameStateChange += GameStateChangeHandler;
        CharController.OnLevelUp += LevelUpHandler;
    }

    

    private void Start()
    {
        UpgradePoint = 0;
    }

    private void LevelUpHandler(int level, bool isMax)
    {
        UpgradePoint += upgradePointPerLevel;

        _highestUpgradePoint = UpgradePoint;
    }

    void GameStateChangeHandler(GameState state)
    {
        if(state == GameState.GameInit)
        {
            maxHpPerUpgrade = (player.statDB.MaxHPUpgrade -
                                player.statDB.stats[player.statDB.stats.Length - 1].MaxHp) /
                                (player.statDB.stats.Length * upgradePointPerLevel);

            maxDamagePerUpgrade = (player.statDB.MaxDamageUpgrade -
                                    player.statDB.stats[player.statDB.stats.Length - 1].Damage) /
                                    (player.statDB.stats.Length * upgradePointPerLevel);

            maxCritPerUpgrade = (player.statDB.MaxCriticalUpgrade -
                                    player.statDB.stats[player.statDB.stats.Length - 1].CritChance) /
                                    (player.statDB.stats.Length * upgradePointPerLevel);

            maxMovePerUpgrade = (player.statDB.MaxMoveSpeedUpgrade -
                                    player.statDB.stats[player.statDB.stats.Length - 1].MovementSpeed) /
                                    (player.statDB.stats.Length * upgradePointPerLevel);
        }
        else if(state == GameState.OpenStats)
        {
            SetHPUI();
            SetDamageUI();
            SetCritUI();
            SetMoveSpeedUI();
        }
    }

    void SetHPUI()
    {
        maxHpUI.value = (player.baseStat.MaxHp + player.upgradeStat.MaxHp) / player.statDB.MaxHPUpgrade;
        hpText.text = $"{player.baseStat.MaxHp.ToString("F1")} + {player.upgradeStat.MaxHp.ToString("F1")} = {(player.baseStat.MaxHp + player.upgradeStat.MaxHp).ToString("F1")}";
    }

    public void PlusHP()
    {
        if(UpgradePoint > 0)
        {
            UpgradePoint--;           

            player.upgradeStat.MaxHp += maxHpPerUpgrade;
            hpToBeAdded += maxHpPerUpgrade;
        }
        SetHPUI();
    }

    public void MinusHP()
    {
        if (UpgradePoint < _highestUpgradePoint)
        {
            UpgradePoint++;

            player.upgradeStat.MaxHp -= maxHpPerUpgrade;
            hpToBeAdded -= maxHpPerUpgrade;
        }
        SetHPUI();
    }

    void SetDamageUI()
    {
        maxDamageUI.value = (player.baseStat.Damage + player.upgradeStat.Damage) / player.statDB.MaxDamageUpgrade;
        damageText.text = $"{player.baseStat.Damage.ToString("F1")} + {player.upgradeStat.Damage.ToString("F1")} =  {(player.baseStat.Damage + player.upgradeStat.Damage).ToString("F1")}";
    }

    public void PlusDamage()
    {
        if (UpgradePoint > 0)
        {
            UpgradePoint--;

            player.upgradeStat.Damage += maxDamagePerUpgrade;
        }
        SetDamageUI();
    }

    public void MinusDamage()
    {
        if (UpgradePoint < _highestUpgradePoint)
        {
            UpgradePoint++;

            player.upgradeStat.Damage -= maxDamagePerUpgrade;
        }
        SetDamageUI();
    }

    void SetCritUI()
    {
        maxCritUI.value = (player.baseStat.CritChance + player.upgradeStat.CritChance) / player.statDB.MaxCriticalUpgrade;
        critText.text = $"{player.baseStat.CritChance.ToString("F1")} + {player.upgradeStat.CritChance.ToString("F1")} = {(player.baseStat.CritChance + player.upgradeStat.CritChance).ToString("F1")}";
    }

    public void PlusCritical()
    {
        if (UpgradePoint > 0)
        {
            UpgradePoint--;

            player.upgradeStat.CritChance += maxCritPerUpgrade;
        }
        SetCritUI();
    }

    public void MinusCritical()
    {
        if (UpgradePoint < _highestUpgradePoint)
        {
            UpgradePoint++;

            player.upgradeStat.CritChance -= maxCritPerUpgrade;
        }
        SetCritUI();
    }

    void SetMoveSpeedUI()
    {
        maxMoveUI.value = (player.baseStat.MovementSpeed + player.upgradeStat.MovementSpeed) / player.statDB.MaxMoveSpeedUpgrade;
        moveText.text = $"{player.baseStat.MovementSpeed.ToString("F1")} + {player.upgradeStat.MovementSpeed.ToString("F1")} = {(player.baseStat.MovementSpeed + player.upgradeStat.MovementSpeed).ToString("F1")}";
    }

    public void PlusMoveSpeed()
    {
        if (UpgradePoint > 0)
        {
            UpgradePoint--;

            player.upgradeStat.MovementSpeed += maxMovePerUpgrade;
        }
        SetMoveSpeedUI();
    }

    public void MinusMoveSpeed()
    {
        if (UpgradePoint < _highestUpgradePoint)
        {
            UpgradePoint++;

            player.upgradeStat.MovementSpeed -= maxMovePerUpgrade;
        }
        SetMoveSpeedUI();
    }

    public void ConfirmButton()
    {
        player.SetCurrentStats();
        player.CurrentHP += hpToBeAdded;
        _highestUpgradePoint = UpgradePoint;

        GameManager.Instance.UpdateGameState(GameState.Gameplay);
    }
}
