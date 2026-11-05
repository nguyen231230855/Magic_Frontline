using System.Globalization;
using TMPro;
using UnityEngine;
public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance;

    public int health = 100;
    public TextMeshProUGUI healthTxt;
    public int MaxHealth { get; private set; }

    private void Awake()
    {
        Instance = this;
        MaxHealth = health;
    }

    public void ResetHealth()
    {
        health = MaxHealth;
        Instance = this;
        healthTxt.text = health.ToString("0.##", CultureInfo.InvariantCulture);
    }

    public void UpdateHealth(int changeAmount)
    {
        health += changeAmount;

        healthTxt.text = health.ToString("0.##", CultureInfo.InvariantCulture);

        if (health <= 0)
        {
            health = 0;
            healthTxt.text = "0";
            var home = FindFirstObjectByType<LevelHome>();
            Debug.Assert(home != null, "LevelHome missing");
            if (home != null)
                home.ShowLost();
        }
    }
}
