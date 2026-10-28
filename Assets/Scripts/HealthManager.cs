using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance;

    public int health = 100;
    public TextMeshProUGUI healthTxt;
    private void Awake()
    {
        Instance = this;
    }

    public void UpdateHealth(int changeAmount)
    {
        health += changeAmount;

        healthTxt.text = health.ToString();

        if (health <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
