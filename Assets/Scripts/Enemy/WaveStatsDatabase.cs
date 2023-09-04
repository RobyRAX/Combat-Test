using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterStats", menuName = "Stats/Wave Stats Database")]
public class WaveStatsDatabase : ScriptableObject
{
    public WaveStats[] Stats;
}
