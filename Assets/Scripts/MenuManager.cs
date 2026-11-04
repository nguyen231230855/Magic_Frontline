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
    bool closingOptions;
    bool closingPlay;

    static bool revealed;
    CanvasGroup canvasGroup;

    void Awake()
    {
        if (gameTitle != null)
            gameTitle.enabled = false;
        if (capButton != null)
            capButton.onClick.AddListener(OnCapClicked);

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
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
            button.onClick.AddListener(PlayClick);

        if (AudioManager.Instance == null)
            return;
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
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
        optionsPanel.GetComponent<Animator>().Play("OptionsSlideIn", 0, 0f);
        RefreshDim();
    }

    public void CloseOptions()
    {
        if (closingOptions)
            return;
        StartCoroutine(CloseOptionsRoutine());
    }

    IEnumerator CloseOptionsRoutine()
    {
        closingOptions = true;
        Animator animator = optionsPanel.GetComponent<Animator>();
        animator.Play("OptionsSlideOut", 0, 0f);
        yield return null;
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        optionsPanel.SetActive(false);
        closingOptions = false;
        RefreshDim();
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
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
