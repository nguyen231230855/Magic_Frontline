using UnityEngine;

[System.Serializable]
public class TowerUpgradeStages
{
    public float range;
    public float fireRate;
    public Sprite sprite;
    public int price;
}
public class Tower : MonoBehaviour
{
    public float range = 3f;
    public float fireRate = 1f;
    public GameObject projectilePrefab;
    public Transform firePoint;

    public TowerUpgradeStages[] upgradeStages;
    public int upgradeStage = 0;
    private SpriteRenderer sr;
    public GameObject towerUpgradeUIPrefab;
    private GameObject currentUI;

    public int towerPrice = 1;

    private float fireCooldown = 0f;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        fireCooldown -= Time.deltaTime;

        Enemy target = FindBestTarget();

        if (target != null && fireCooldown <= 0f)
        {
            Shoot(target);
            fireCooldown = 1f / fireRate;
        }
    }

    Enemy FindBestTarget()
    {
        Enemy[] enemies = GameObject.FindObjectsOfType<Enemy>();

        Enemy best = null;
        float bestProgress = -1f;

        foreach (Enemy e in enemies)
        {
            float dist = Vector2.Distance(transform.position, e.transform.position);

            if (dist <= range)
            {
                if (e.currentWayPoint > bestProgress)
                {
                    bestProgress = e.currentWayPoint;
                    best = e;
                }
            }
        }

        return best;
    }

    void Shoot(Enemy target)
    {
        GameObject p = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Projectile pr = p.GetComponent<Projectile>();
        pr.target = target.transform;
    }

    public void Upgrade()
    {
        TowerUpgradeStages currentUpgradeStage = upgradeStages[upgradeStage];

        range = currentUpgradeStage.range;
        fireRate = currentUpgradeStage.fireRate;
        sr.sprite = currentUpgradeStage.sprite;
        CoinManager.instance.UpdateCoins(-currentUpgradeStage.price);
        upgradeStage++;
    }

    private void OnMouseDown()
    {
        if(currentUI == null)
        {
            currentUI =Instantiate(towerUpgradeUIPrefab, FindObjectOfType<Canvas>().transform);
        }

        TowerUpgradeUI currentUpgradeUI = currentUI.GetComponent<TowerUpgradeUI>();
        currentUpgradeUI.tower = this;

        currentUI.transform.position = Input.mousePosition + new Vector3(50, -50);
    
        if(upgradeStage >= upgradeStages.Length)
        {
            return;
        }
        currentUpgradeUI.priceTxt.text = upgradeStages[upgradeStage].price.ToString();
    }
}
