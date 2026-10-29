using UnityEngine;
using UnityEngine.UI;

public class SpeedUpManager : MonoBehaviour
{
    public bool speedUp;
    public Image btnImage;
    public Color normalColor;
    public Color speedUpColor;

    public void ToggleSpeedUp()
    {
        speedUp = !speedUp;
        
        if (speedUp)
        {
            Time.timeScale = 2; 
            btnImage.color = speedUpColor; 
        }
        else
        {
            Time.timeScale = 1; 
            btnImage.color = normalColor; 
        }
    }
}
