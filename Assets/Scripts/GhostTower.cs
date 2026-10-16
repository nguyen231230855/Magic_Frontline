using UnityEngine;
using System.IO;
using System.Net;
using System.Text;

public class GhostTower : MonoBehaviour
{
    SpriteRenderer sr;

    static readonly Color ValidColor = new Color(0.15f, 1f, 0.25f, 0.75f);
    static readonly Color InvalidColor = new Color(1f, 0.12f, 0.12f, 0.75f);

    
    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.color = ValidColor;
    }

    public void SetValid(bool valid)
    {
        sr.color = valid ? ValidColor : InvalidColor;
    }
}
