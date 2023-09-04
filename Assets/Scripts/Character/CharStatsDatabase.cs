using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterStats", menuName = "Stats/Character Stats Database")]
public class CharStatsDatabase : ScriptableObject
{
    [Header("Maximum Stats After Upgrade")]
    public float MaxHPUpgrade;
    public float MaxDamageUpgrade;
    public float MaxCriticalUpgrade;
    public float MaxMoveSpeedUpgrade;

    public CharStats[] stats;
}
