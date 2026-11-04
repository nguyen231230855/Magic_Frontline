using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelCanvasLoader : MonoBehaviour
{
    static GameObject spawned;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Boot()
    {
        var go = new GameObject(nameof(LevelCanvasLoader));
        DontDestroyOnLoad(go);
        go.AddComponent<LevelCanvasLoader>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Main Menu")
        {
            if (spawned != null)
                Destroy(spawned);
            return;
        }

        if (scene.name == "Level_1")
        {
            OnLevelOne(scene);
            return;
        }

        var canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var canvas in canvases)
        {
            if (canvas.gameObject.scene == scene)
                Destroy(canvas.gameObject);
        }

        if (spawned == null)
        {
            var prefab = Resources.Load<GameObject>("LevelCanvas");
            Debug.Assert(prefab != null, "LevelCanvas prefab missing");
            spawned = Instantiate(prefab);
            DontDestroyOnLoad(spawned);
        }

        Button goButton = null;
        TextMeshProUGUI waveTxt = null;
        foreach (var button in spawned.GetComponentsInChildren<Button>(true))
        {
            if (button.name == "GoBTN")
                goButton = button;
        }
        foreach (var label in spawned.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (label.name == "WaveTxt")
                waveTxt = label;
        }
        Debug.Assert(goButton != null && waveTxt != null, "Level canvas missing GoBTN or WaveTxt");

        foreach (var wave in FindObjectsByType<WaveManager>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (wave.gameObject.scene == scene)
                wave.UseHud(goButton, waveTxt);
        }

        EnsureHome(spawned);
    }

    static void OnLevelOne(Scene scene)
    {
        if (spawned != null)
            Destroy(spawned);
        foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (canvas.gameObject.scene == scene && canvas.name == "Canvas")
                EnsureHome(canvas.gameObject);
        }
    }

    static void EnsureHome(GameObject canvas)
    {
        var home = canvas.GetComponent<LevelHome>();
        if (home == null)
            home = canvas.AddComponent<LevelHome>();
        else
            home.Hide();
    }
}

public class LevelHome : MonoBehaviour
{
    GameObject panel;
    GameObject dim;
    float previousScale = 1f;
    bool paused;

    void Awake()
    {
        panel = transform.Find("HomeBackPanel").gameObject;
        var existing = transform.Find("DimBackground");
        dim = existing != null ? existing.gameObject : CreateDim();
        dim.SetActive(false);
        panel.SetActive(false);
        FindButton("HomeBTN").onClick.AddListener(Open);
        FindButton("YesBTN").onClick.AddListener(Confirm);
        FindButton("NoBTN").onClick.AddListener(Hide);
        var optionsButton = FindButton("SettingBTN");
        if (optionsButton != null)
            optionsButton.onClick.AddListener(OpenOptions);
        foreach (var button in GetComponentsInChildren<Button>(true))
            button.onClick.AddListener(PlayClick);
        Debug.Assert(MenuManager.ClickSfx != null, "Click sfx missing");
    }

    Button FindButton(string name)
    {
        foreach (var button in GetComponentsInChildren<Button>(true))
        {
            if (button.name == name)
                return button;
        }
        Debug.Assert(false, name + " missing");
        return null;
    }

    GameObject CreateDim()
    {
        var go = new GameObject("DimBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(transform, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.5882353f);
        return go;
    }

    void PlayClick()
    {
        if (AudioManager.Instance == null || MenuManager.ClickSfx == null)
            return;
        AudioManager.Instance.PlaySFX(MenuManager.ClickSfx);
    }

    void OpenOptions()
    {
        Debug.Assert(OptionsHost.Instance != null, "OptionsHost missing");
        OptionsHost.Instance.OpenPaused();
    }

    void Open()
    {
        if (!paused)
        {
            previousScale = Time.timeScale;
            Time.timeScale = 0f;
            paused = true;
        }
        dim.SetActive(true);
        panel.SetActive(true);
        dim.transform.SetAsLastSibling();
        panel.transform.SetAsLastSibling();
    }

    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
        if (dim != null)
            dim.SetActive(false);
        if (!paused)
            return;
        Time.timeScale = previousScale;
        paused = false;
    }

    void Confirm()
    {
        Time.timeScale = 1f;
        paused = false;
        SceneManager.LoadScene("Main Menu");
    }
}
