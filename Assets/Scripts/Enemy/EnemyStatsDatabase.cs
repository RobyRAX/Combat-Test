using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterStats", menuName = "Stats/Enemy Stats Database")]
public class EnemyStatsDatabase : ScriptableObject
{
    public string enemyName;
    public EnemyStats[] Stats;
}
