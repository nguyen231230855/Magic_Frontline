using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

[System.Serializable]
public class WaveData
{
    public float duration = 10f;
    public int easyEnemies = 5;
    public int normalEnemies = 3;
    public int hardEnemies = 2;
    public int sHardEnemies = 1;
    public int orcEnemies;
}

public class WaveManager : MonoBehaviour
{
    public WaveData[] waves;
    public Button startWaveButton;

    public GameObject easyEnemyPrefab;
    public GameObject normalEnemyPrefab;
    public GameObject hardEnemyPrefab;
    public GameObject sHardEnemyPrefab;
    public GameObject orcEnemyPrefab;

    public Transform[] wayPoints;

    public TextMeshProUGUI waveTxt;
    const float autoDelay = 5f;
    private int currentWaveIndex = 0;
    private bool waveRunning = false;
    Coroutine auto;
    static bool won;
    Grid grid;
    bool pathSnapped;

    void Start()
    {
        startWaveButton.onClick.RemoveListener(StartWave);
        startWaveButton.onClick.AddListener(StartWave);
        startWaveButton.gameObject.SetActive(true);
        startWaveButton.interactable = true;
        grid = FindFirstObjectByType<Grid>();
        Debug.Assert(grid != null, "WaveManager missing Grid");
        Debug.Assert(waves != null && waves.Length > 0, "WaveManager has no waves");
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

    IEnumerator AutoStart()
    {
        yield return new WaitForSeconds(autoDelay);
        auto = null;
        StartWave();
    }

    public void StartWave()
    {
        if (waveRunning) return;
        if (currentWaveIndex >= waves.Length) return;
        if (auto != null)
        {
            StopCoroutine(auto);
            auto = null;
        }

        StartCoroutine(RunWave());
    }
    
    IEnumerator RunWave()
    {
        waveRunning = true;
        startWaveButton.interactable = false;

        int later = 0;
        foreach (var manager in FindObjectsByType<WaveManager>(FindObjectsSortMode.None))
        {
            if (manager.gameObject.scene != gameObject.scene) continue;
            if (string.CompareOrdinal(manager.name, name) < 0)
                later++;
        }
        if (later > 0)
            yield return new WaitForSeconds(later * 2.5f);

        WaveData wave = waves[currentWaveIndex];

        for(int i = 0; i < wave.easyEnemies; i++)
        {
            SpawnEnemy(easyEnemyPrefab);
            yield return new WaitForSeconds((wave.duration / 3) / wave.easyEnemies);
        }

        for(int i = 0; i < wave.normalEnemies; i++)
        {
            SpawnEnemy(normalEnemyPrefab);
            yield return new WaitForSeconds((wave.duration / 3) / wave.normalEnemies);
        }

        for(int i = 0; i < wave.hardEnemies; i++)
        {
            SpawnEnemy(hardEnemyPrefab);
            yield return new WaitForSeconds((wave.duration / 3) / wave.hardEnemies);
        }

        for(int i = 0; i < wave.sHardEnemies; i++)
        {
            SpawnEnemy(sHardEnemyPrefab);
            yield return new WaitForSeconds((wave.duration / 3) / wave.sHardEnemies);
        }

        if (wave.orcEnemies > 0)
        {
            Debug.Assert(orcEnemyPrefab != null, "Wave has orcs but no prefab");
            for (int i = 0; i < wave.orcEnemies; i++)
            {
                SpawnEnemy(orcEnemyPrefab);
                yield return new WaitForSeconds((wave.duration / 3) / wave.orcEnemies);
            }
        }

        yield return new WaitForSeconds(wave.duration / 3);
        waveRunning = false;
        currentWaveIndex++;
        if (currentWaveIndex >= waves.Length)
        {
            startWaveButton.gameObject.SetActive(false);
            yield break;
        }
        waveTxt.text = (currentWaveIndex + 1).ToString();
        // shortcut: one shared GoBTN, the first lane that reaches its last wave hides it
        if (currentWaveIndex == waves.Length - 1)
            startWaveButton.gameObject.SetActive(false);
        else
            startWaveButton.interactable = true;
        auto = StartCoroutine(AutoStart());
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
