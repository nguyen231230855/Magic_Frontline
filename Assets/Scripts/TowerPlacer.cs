using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.IO;
using System.Net;

public class TowerPlacer : MonoBehaviour
{
    public Tilemap placementMap;
    public Tilemap nonPlacementMap;

    public GameObject ghostPrefab;

    private HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();
    private GameObject ghostInstance;


    void Update()
    {
        HandlePlacementHover();
        HandlePlacementClick();
    }

    void HandlePlacementHover()
    {
        if (TowerSelectionUI.selectedTowerPrefab == null)
        {
            if (ghostInstance != null)
            {
                Destroy(ghostInstance);
            }
            return;
        }

        if (ghostInstance == null)
        {
            ghostInstance = Instantiate(ghostPrefab);
        }

        ghostInstance.GetComponent<SpriteRenderer>().sprite = TowerSelectionUI.selectedTowerPrefab.GetComponent<SpriteRenderer>().sprite;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Vector3Int cellPos = placementMap.WorldToCell(mouseWorldPos);

        Vector3 worldCenter = placementMap.GetCellCenterWorld(cellPos);
        worldCenter.z = 0;

        ghostInstance.transform.position = worldCenter + new Vector3(0, placementMap.cellSize.y * 0.25f, 0);

        bool valid = placementMap.HasTile(cellPos) && !occupiedCells.Contains(cellPos);
        GhostTower ghost = ghostInstance.GetComponent<GhostTower>();
        if (ghost != null)
        {
            ghost.SetValid(valid);
        }

    }

    void HandlePlacementClick()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (TowerSelectionUI.selectedTowerPrefab == null) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Vector3Int cellPos = placementMap.WorldToCell(mouseWorldPos);

        if (!placementMap.HasTile(cellPos) || occupiedCells.Contains(cellPos)) return;

        Instantiate(TowerSelectionUI.selectedTowerPrefab, ghostInstance.transform.position, Quaternion.identity);

        TowerSelectionUI.selectedTowerPrefab = null;
        occupiedCells.Add(cellPos);
    }
}
