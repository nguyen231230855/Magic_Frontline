using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

[System.Serializable]
public class WaveData
{
    public float duration = 10f;
    public int easyEnemies = 5;
    public int hardEnemies = 2;
}

public class WaveManager : MonoBehaviour
{
    public WaveData[] waves;
    public Button startWaveButton;

    public GameObject easyEnemyPrefab;
    public GameObject hardEnemyPrefab;

    public Transform[] wayPoints;

    public TextMeshProUGUI waveTxt;
    private int currentWaveIndex = 0;
    private bool waveRunning = false;
    static bool won;
    Grid grid;
    bool pathSnapped;

    void Start()
    {
        startWaveButton.onClick.AddListener(StartWave);
        grid = FindFirstObjectByType<Grid>();
        Debug.Assert(grid != null, "WaveManager missing Grid");
        won = false;
    }

    public void UseHud(Button button, TextMeshProUGUI text)
    {
        if (startWaveButton != null)
            startWaveButton.onClick.RemoveListener(StartWave);
        startWaveButton = button;
        waveTxt = text;
        startWaveButton.onClick.AddListener(StartWave);
    }

    public void StartWave()
    {
        if (waveRunning) return;
        if (currentWaveIndex >= waves.Length) return;
        
        StartCoroutine(RunWave());
    }
    
    IEnumerator RunWave()
    {
        waveRunning = true;
        startWaveButton.interactable = false;

        WaveData wave = waves[currentWaveIndex];

        for(int i = 0; i < wave.easyEnemies; i++)
        {
            SpawnEnemy(easyEnemyPrefab);
            yield return new WaitForSeconds((wave.duration / 3) / wave.easyEnemies);
        }

        for(int i = 0; i < wave.hardEnemies; i++)
        {
            SpawnEnemy(hardEnemyPrefab);
            yield return new WaitForSeconds((wave.duration / 3) / wave.hardEnemies);
        }

        yield return new WaitForSeconds(wave.duration / 3);
        waveRunning = false;
        startWaveButton.interactable = true;
        currentWaveIndex++;
        waveTxt.text = (currentWaveIndex + 1).ToString();
    }

    bool Cleared => currentWaveIndex >= waves.Length && !waveRunning;

    void Update()
    {
        if (won || !Cleared) return;
        if (HealthManager.Instance == null || HealthManager.Instance.health <= 0) return;
        foreach (var manager in FindObjectsByType<WaveManager>(FindObjectsSortMode.None))
            if (!manager.Cleared) return;
        if (FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length > 0) return;
        won = true;
        startWaveButton.interactable = false;
        Time.timeScale = 0f;
        var home = FindFirstObjectByType<LevelHome>();
        Debug.Assert(home != null, "LevelHome missing");
        home.ShowWin(HealthManager.Instance.health);
    }

    void SpawnEnemy(GameObject prefab)
    {
        if (!pathSnapped)
        {
            for (int i = 0; i < wayPoints.Length; i++)
            {
                Vector3 center = grid.GetCellCenterWorld(grid.WorldToCell(wayPoints[i].position));
                center.z = 0f;
                wayPoints[i].position = center;
            }
            pathSnapped = true;
        }

        GameObject e = Instantiate(prefab, wayPoints[0].position, Quaternion.identity);
        Enemy enemy = e.GetComponent<Enemy>();
        enemy.waypoints = wayPoints;
    }
}
