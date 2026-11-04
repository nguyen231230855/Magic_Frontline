using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] Button capButton;
    [SerializeField] Animator gameTitle;
    [SerializeField] GameObject menuScreen;

    void Awake()
    {
        if (gameTitle != null)
            gameTitle.enabled = false;
        if (capButton != null)
            capButton.onClick.AddListener(OnCapClicked);
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
}
