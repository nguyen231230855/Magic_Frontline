using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager instance;

    public int coins;
    public TextMeshProUGUI coinTxt;
    int startCoins;

    private void Awake()
    {
        instance = this;
        startCoins = coins;
        UpdateCoins(0);
    }

    public void ResetCoins()
    {
        coins = startCoins;
        instance = this;
        UpdateCoins(0);
    }

    public void UpdateCoins(int changeAmount)
    {
        coins += changeAmount;
        coinTxt.text = coins.ToString();
    }
}
