using UnityEngine;

public class TowerSelectionUI : MonoBehaviour
{
    public static GameObject selectedTowerPrefab;

    public void SelectTower(GameObject towerPrefab)
    {
        if (selectedTowerPrefab == towerPrefab)
        {
            selectedTowerPrefab = null;
            return;
        }

        selectedTowerPrefab = towerPrefab;
    }
}
