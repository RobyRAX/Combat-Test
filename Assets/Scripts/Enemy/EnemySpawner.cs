using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    public static event Action<int, int> OnWaveChange;
    public static event Action OnLevelComplete;

    public GameObject[] enemyPrefabs;

    [Header("Wave Parameter")]
    public WaveStatsDatabase waveStatsDB;
    [SerializeField] WaveStats waveStats;
    [SerializeField] int _wave;
    [SerializeField] float delayPerWave;
    public int Wave
    {
        get { return _wave; }
        set { _wave = value; }
    }

    [Header("Enemy Pool")]
    public List<GameObject> currentEnemies;
    public List<GameObject> enemyPool;
    public Transform poolParent;

    [Header("Spawn Area")]
    [SerializeField] Vector3 center;
    [SerializeField] Vector3 area;

    private void Awake()
    {
        Instance = this;
        GameManager.OnGameStateChange += GameStateChangeHandler;
        EnemyBase.OnEnemyDead += EnemyDeadHandler;
    }

    private void Start()
    {
        SetWaveStat();
    }

    void SetWaveStat()
    {
        _wave++;
        waveStats = waveStatsDB.Stats[_wave - 1];

        OnWaveChange(_wave, waveStatsDB.Stats.Length);
                
    }

    void GameStateChangeHandler(GameState state)
    {
        if(state == GameState.GameInit)
        {
            InitPool();            
        }
        else if(state == GameState.Gameplay)
        {
            InitialSpawn();
        }
    }

    void EnemyDeadHandler(GameObject enemy, float exp)
    {
        if(currentEnemies.Contains(enemy))
        {
            currentEnemies.Remove(enemy);
            SpawnEnemy();

            if(currentEnemies.Count <= 0 && enemyPool.Count <= 0)
            {
                if (_wave < waveStatsDB.Stats.Length)
                    StartCoroutine(NextWaveCo());
                else
                    OnLevelComplete();
            }
        }
    }

    IEnumerator NextWaveCo()
    {
        SetWaveStat();
        InitPool();

        yield return new WaitForSeconds(delayPerWave);
        InitialSpawn();
    }

    void InitPool()
    {
        for(int i = 0; i < waveStats.enemyPerWave; i++)
        {
            Vector3 spawnPoint = new Vector3(UnityEngine.Random.Range(center.x + area.x, center.x - area.x),
                                center.y + area.y, 
                                UnityEngine.Random.Range(center.z + area.z, center.z - area.z));
            int random = UnityEngine.Random.Range(0, enemyPrefabs.Length);

            GameObject enemy = Instantiate(enemyPrefabs[random], poolParent);
            //enemy.name = $"{enemy.name} - Level {enemy.GetComponent<EnemyBase>().stat.Level} - Wave {_wave}";
            enemy.transform.position = spawnPoint;
            enemyPool.Add(enemy);
            enemy.SetActive(false);
        }
    }

    void InitialSpawn()
    {
        StartCoroutine(InitialSpawnCo());
    }

    IEnumerator InitialSpawnCo()
    {
        while (currentEnemies.Count <= waveStats.maxCurrentEnemy)
        {
            yield return new WaitForSeconds(waveStats.spawnDelay);

            if (enemyPool.Count != 0)
            {
                enemyPool[0].SetActive(true);
                currentEnemies.Add(enemyPool[0]);
                enemyPool.RemoveAt(0);
            }
        }
    }

    void SpawnEnemy()
    {
        StartCoroutine(SpawnEnemyCo());     
    }

    IEnumerator SpawnEnemyCo()
    {
        if(currentEnemies.Count <= waveStats.maxCurrentEnemy)
        {
            yield return new WaitForSeconds(waveStats.spawnDelay);

            if(enemyPool.Count != 0)
            {
                enemyPool[0].SetActive(true);
                currentEnemies.Add(enemyPool[0]);
                enemyPool.RemoveAt(0);
            }           
        }        
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(center, Vector3.one);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(center + area, center + new Vector3(-area.x, area.y, area.z));
        Gizmos.DrawLine(center + new Vector3(area.x, area.y, -area.z), center + new Vector3(-area.x, area.y, -area.z));

        Gizmos.color = Color.red;
        Gizmos.DrawLine(center + area, center + new Vector3(area.x, area.y, -area.z));
        Gizmos.DrawLine(center + new Vector3(-area.x, area.y, area.z), center + new Vector3(-area.x, area.y, -area.z));
    }
}
