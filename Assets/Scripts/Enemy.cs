using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 2f;
    public int health = 1;
    public Transform[] waypoints;

    public int currentWayPoint = 0;

    void Update()
    {
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
                HealthManager.Instance.UpdateHealth(-1);
                Destroy(gameObject);
            }
        }
    }
}
