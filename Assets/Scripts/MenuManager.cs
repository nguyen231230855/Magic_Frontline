using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] Button capButton;
    [SerializeField] Animator gameTitle;
    [SerializeField] GameObject menuScreen;
    [SerializeField] GameObject optionsPanel;
    [SerializeField] GameObject playPanel;
    [SerializeField] GameObject dimBackground;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider soundSlider;
    [SerializeField] AudioClip clickSfx;
    public static AudioClip ClickSfx;
    bool closingPlay;

    static bool revealed;
    public static bool opened;
    static GameObject keptOptions;
    static GameObject keptDim;
    static GameObject keptPlay;
    static OptionsHost host;
    CanvasGroup canvasGroup;

    void Awake()
    {
        if (capButton != null)
            capButton.onClick.AddListener(OnCapClicked);
        ClickSfx = clickSfx;

        if (keptOptions == null)
        {
            var layer = new GameObject("OptionsLayer", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            DontDestroyOnLoad(layer);
            var canvas = layer.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1
                | AdditionalCanvasShaderChannels.Normal
                | AdditionalCanvasShaderChannels.Tangent;
            var scaler = layer.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            host = layer.AddComponent<OptionsHost>();
            keptOptions = optionsPanel;
            keptDim = dimBackground;
            foreach (var button in optionsPanel.GetComponentsInChildren<Button>(true))
            {
                if (button.name == "CloseBTN")
                    button.onClick.AddListener(host.Close);
            }
        }
        else
        {
            Destroy(optionsPanel);
            Destroy(dimBackground);
            optionsPanel = keptOptions;
            dimBackground = keptDim;
        }

        keptPlay = playPanel;
        host.panel = optionsPanel;
        host.dim = dimBackground;
        optionsPanel.transform.SetParent(host.transform, false);
        dimBackground.transform.SetParent(transform, false);
        optionsPanel.SetActive(false);
        playPanel.SetActive(false);
        dimBackground.SetActive(false);

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (opened)
        {
            if (gameTitle != null)
            {
                gameTitle.enabled = true;
                gameTitle.Play("GameTitleUp", 0, 1f);
            }
            if (capButton != null)
                capButton.interactable = false;
            menuScreen.SetActive(true);
            revealed = true;
            return;
        }

        if (gameTitle != null)
            gameTitle.enabled = false;
        if (revealed)
            return;

        revealed = true;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        StartCoroutine(FadeInOnce());
    }

    IEnumerator FadeInOnce()
    {
        const float fadeSeconds = 1f;
        float t = 0f;
        while (t < fadeSeconds)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = t / fadeSeconds;
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    void Start()
    {
        foreach (Button button in GetComponentsInChildren<Button>(true))
        {
            if (button.transform.IsChildOf(optionsPanel.transform))
                continue;
            button.onClick.AddListener(PlayClick);
        }

        if (host.wired || AudioManager.Instance == null)
            return;
        host.wired = true;
        foreach (var button in optionsPanel.GetComponentsInChildren<Button>(true))
            button.onClick.AddListener(host.PlayClick);
        musicSlider.SetValueWithoutNotify(AudioManager.Instance.MusicLevel);
        soundSlider.SetValueWithoutNotify(AudioManager.Instance.SfxLevel);
        musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
        soundSlider.onValueChanged.AddListener(AudioManager.Instance.SetSfxVolume);
    }

    void PlayClick()
    {
        AudioManager.Instance.PlaySFX(clickSfx);
    }

    public void OnCapClicked()
    {
        if (capButton != null)
            capButton.interactable = false;
        StartCoroutine(PlayTitleThenOpenMenu());
    }

    IEnumerator PlayTitleThenOpenMenu()
    {
        gameTitle.enabled = true;
        gameTitle.Play("GameTitleUp", 0, 0f);
        yield return null;
        yield return new WaitForSeconds(gameTitle.GetCurrentAnimatorStateInfo(0).length);
        menuScreen.SetActive(true);
        opened = true;
    }

    public void LoadScene(string sceneName)
    {
        KeepDim();
        SceneManager.LoadScene(sceneName);
    }

    void OnDisable()
    {
        if (gameObject.scene.isLoaded)
            return;
        KeepDim();
    }

    void KeepDim()
    {
        if (host == null || dimBackground != keptDim)
            return;
        dimBackground.transform.SetParent(host.transform, false);
        dimBackground.SetActive(false);
        optionsPanel.SetActive(false);
    }

    public void OpenOptions()
    {
        host.Open();
    }

    public void CloseOptions()
    {
        host.Close();
    }

    public void OpenPlay()
    {
        playPanel.SetActive(true);
        playPanel.GetComponent<Animator>().Play("PlaySlideIn", 0, 0f);
        RefreshDim();
    }

    public void ClosePlay()
    {
        if (closingPlay)
            return;
        StartCoroutine(ClosePlayRoutine());
    }

    IEnumerator ClosePlayRoutine()
    {
        closingPlay = true;
        Animator animator = playPanel.GetComponent<Animator>();
        animator.Play("PlaySlideOut", 0, 0f);
        yield return null;
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        playPanel.SetActive(false);
        closingPlay = false;
        RefreshDim();
    }

    void RefreshDim()
    {
        dimBackground.SetActive(optionsPanel.activeSelf || playPanel.activeSelf);
        if (!playPanel.activeSelf)
            return;
        dimBackground.transform.SetAsLastSibling();
        playPanel.transform.SetAsLastSibling();
    }

    public static bool PlayOpen => keptPlay != null && keptPlay.activeSelf;

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

public class OptionsHost : MonoBehaviour
{
    public static OptionsHost Instance;
    public GameObject panel;
    public GameObject dim;
    public bool wired;
    bool closing;
    bool resume;
    float resumeScale = 1f;

    void Awake()
    {
        Instance = this;
    }

    public void Open()
    {
        Debug.Assert(panel != null && dim != null, "Options panel missing");
        panel.SetActive(true);
        dim.SetActive(true);
        dim.transform.SetAsLastSibling();
        panel.transform.SetAsLastSibling();
        var animator = panel.GetComponent<Animator>();
        animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        animator.Play("OptionsSlideIn", 0, 0f);
        animator.Update(0f);
    }

    public void OpenPaused()
    {
        if (!resume)
        {
            resumeScale = Time.timeScale;
            resume = true;
            Time.timeScale = 0f;
        }
        Open();
    }

    public void PlayClick()
    {
        if (AudioManager.Instance == null || MenuManager.ClickSfx == null)
            return;
        AudioManager.Instance.PlaySFX(MenuManager.ClickSfx);
    }

    public void Close()
    {
        if (closing)
            return;
        StartCoroutine(CloseRoutine());
    }

    System.Collections.IEnumerator CloseRoutine()
    {
        closing = true;
        var animator = panel.GetComponent<Animator>();
        animator.Play("OptionsSlideOut", 0, 0f);
        yield return null;
        yield return new WaitForSecondsRealtime(animator.GetCurrentAnimatorStateInfo(0).length);
        panel.SetActive(false);
        closing = false;
        if (resume)
        {
            Time.timeScale = resumeScale;
            resume = false;
        }
        bool playOpen = MenuManager.PlayOpen;
        dim.SetActive(playOpen);
    }
}
