using System.Globalization;
using TMPro;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 2f;
    public int health = 1;
    public int reward = 1;
    public Transform[] waypoints;

    public int currentWayPoint = 0;
    public float damageTaken = 1f;
    float damageLeft;
    int burnPerTick;
    float burnLeft;
    float burnTick;
    float baseSpeed;
    float slowLeft;
    float slowScale = 1f;
    TextMeshPro status;

    public void Hurt(int amount)
    {
        Take(amount * damageTaken);
    }

    public void HurtRaw(int amount)
    {
        Take(amount);
    }

    void Take(float amount)
    {
        if (amount <= 0f) return;
        ShowDamage(amount);
        damageLeft += amount;
        int lost = Mathf.FloorToInt(damageLeft + 0.0001f);
        if (lost > 0)
        {
            damageLeft -= lost;
            health -= lost;
        }
        if (health > 0) return;
        CoinManager.instance.UpdateCoins(reward);
        Destroy(gameObject);
    }

    void ShowDamage(float amount)
    {
        if (amount <= 0f) return;
        var go = new GameObject("Damage");
        go.transform.position = transform.position + new Vector3(Random.Range(-0.15f, 0.15f), 0.55f, 0f);
        go.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        go.transform.localScale = new Vector3(-1f, 1f, 1f);
        var text = go.AddComponent<TextMeshPro>();
        text.font = TMP_Settings.defaultFontAsset;
        Debug.Assert(text.font != null, "Damage number missing font");
        bool whole = Mathf.Abs(amount - Mathf.Round(amount)) < 0.001f;
        text.text = "-" + (whole
            ? Mathf.RoundToInt(amount).ToString()
            : amount.ToString("0.##", CultureInfo.InvariantCulture));
        text.fontSize = 5f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(1f, 0.25f, 0.2f);
        text.sortingOrder = 20;
        text.rectTransform.sizeDelta = new Vector2(3f, 1f);
        go.AddComponent<DamageNumber>();
    }

    public void Burn(int perTick, float duration)
    {
        if (health <= 0 || perTick <= 0 || duration <= 0f) return;
        if (slowLeft > 0f)
        {
            slowLeft = 0f;
            speed = baseSpeed;
            RefreshStatus();
            return;
        }
        burnPerTick = perTick;
        burnLeft = duration;
        RefreshStatus();
    }

    public void Slow(float scale, float duration)
    {
        if (health <= 0 || duration <= 0f || scale <= 0f || scale >= 1f) return;
        if (burnLeft > 0f)
        {
            burnLeft = 0f;
            RefreshStatus();
            return;
        }
        slowScale = scale;
        slowLeft = duration;
        speed = baseSpeed * slowScale;
        RefreshStatus();
    }

    void Awake()
    {
        Debug.Assert(damageTaken > 0f, "damageTaken must be above 0");
        baseSpeed = speed;
        var go = new GameObject("Status");
        go.transform.SetParent(transform, false);
        go.transform.position = transform.position + Vector3.up * 0.95f;
        go.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        Vector3 parentScale = transform.lossyScale;
        go.transform.localScale = new Vector3(-1f / parentScale.x, 1f / parentScale.y, 1f / parentScale.z);
        status = go.AddComponent<TextMeshPro>();
        status.font = TMP_Settings.defaultFontAsset;
        Debug.Assert(status.font != null, "Status missing font");
        status.fontSize = 3f;
        status.fontStyle = FontStyles.Bold;
        status.alignment = TextAlignmentOptions.Center;
        status.color = new Color(1f, 0.45f, 0.1f);
        status.sortingOrder = 20;
        status.rectTransform.sizeDelta = new Vector2(2f, 1f);
        status.text = "";
    }

    void RefreshStatus()
    {
        status.text = "";
        if (burnLeft > 0f)
            status.text = "Burn";
        if (slowLeft > 0f)
            status.text += status.text.Length > 0 ? "\nSlow" : "Slow";
    }

    void Update()
    {
        if (burnLeft > 0f)
        {
            burnLeft -= Time.deltaTime;
            burnTick += Time.deltaTime;
            if (burnTick >= 1f)
            {
                burnTick -= 1f;
                HurtRaw(burnPerTick);
                if (health <= 0) return;
            }
            if (burnLeft <= 0f)
                RefreshStatus();
        }

        if (slowLeft > 0f)
        {
            slowLeft -= Time.deltaTime;
            if (slowLeft <= 0f)
            {
                speed = baseSpeed;
                RefreshStatus();
            }
        }

        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[currentWayPoint];
        Vector3 current = transform.position;
        Vector3 dest = target.position;
        dest.z = current.z;
        Vector3 dir = (dest - current).normalized;

        transform.position += dir * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            currentWayPoint++;

            if (currentWayPoint >= waypoints.Length)
            {
                HealthManager.Instance.UpdateHealth(-reward);
                Destroy(gameObject);
            }
        }
    }
}

public class DamageNumber : MonoBehaviour
{
    void Start()
    {
        Destroy(gameObject, 0.6f);
    }

    void Update()
    {
        transform.position += Vector3.up * Time.deltaTime;
    }
}
