// Author: John Cabusora (J.C.)
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

/*
 * Building placement with transparent sprite and grid snapping.
 */
public class BuildingManager : MonoBehaviour
{
    SoundFXManager soundFXManager;
    [SerializeField] AudioClip placeBuildingSound;

    [SerializeField]
    private Grid grid;

    [SerializeField]
    private Tilemap buildingGrid;
    [SerializeField]
    private TileBase gridTile;

    [SerializeField]
    private TerrainGenerator terrainGenerator;

    [SerializeField] private FlowfieldPathfinding flowfieldPathfinding;

    // Temporary. TODO: Create list of building prefabs.
    [SerializeField]
    private GameObject[] buildingPrefabs;
    private GameObject currentBuilding;
    private BuildingInfo buildingInfo;

    [SerializeField]
    private Tilemap tileMap;
    [SerializeField]
    private Tilemap decorationMap;

    [SerializeField]
    private TileBase[] legalGroundTiles;
    [SerializeField]
    private TileBase[] legalDecorationTiles;

    private InputAction pointer;

    private Vector3 worldPos;
    private Vector3Int mouseCell;
    private Vector3 finalPos;

    private float buildingYSize;

    private Vector3Int[] oldCells;
    private Vector3Int[] newCells;
    private List<Vector3Int> ignoredCells;

    private Vector3Int oldCellPos;

    [SerializeField]
    private GameObject placedBuildings;
    [SerializeField]
    private Tilemap placedBuildingsTiles;

    private int buildingNumber;

    public void Build(int buildingNumber)
    {
        this.buildingNumber = buildingNumber;
		Vector2 pointerPos = pointer.ReadValue<Vector2>();
		worldPos = Camera.main.ScreenToWorldPoint(new Vector3(pointerPos.x, pointerPos.y, 0));
		mouseCell = grid.WorldToCell(worldPos);
		finalPos = grid.CellToWorld(mouseCell);

		GameObject prefab = buildingPrefabs[buildingNumber];

		SpriteRenderer spriteRenderer = prefab.GetComponent<SpriteRenderer>();

		buildingInfo = prefab.GetComponent<BuildingInfo>();

        if (!ResourceManager.Instance.CanBuy(buildingInfo.buildingType))
        {
            UIManager.Instance.NotEnoughGoldNotification();
            return;
        }
        
        if (buildingInfo.buildingType == Type.Wall || buildingInfo.buildingType == Type.Tower)
            newCells = new Vector3Int[(buildingInfo.size.x) * (buildingInfo.size.y)];
        else
            newCells = new Vector3Int[(buildingInfo.size.x + 1) * (buildingInfo.size.y + 1)];

		buildingYSize = spriteRenderer.size.y;

		currentBuilding = Instantiate(prefab, new Vector3(finalPos.x, finalPos.y + (buildingYSize / 2) + 0.25f, finalPos.y + (buildingYSize / 2) + 0.25f), Quaternion.identity);
		currentBuilding.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
		currentBuilding.GetComponent<SpriteRenderer>().sortingOrder = 3;

        this.enabled = true;
	}

    private void Start()
    {
        soundFXManager = SoundFXManager.Instance;

        pointer = InputSystem.actions.FindAction("Pointer");

        newCells = new Vector3Int[0];
        this.enabled = false;
    }

    private void Update()
    {
        if (currentBuilding != null)
        {
            Vector2 pointerPos = pointer.ReadValue<Vector2>();
            worldPos = Camera.main.ScreenToWorldPoint(new Vector3(pointerPos.x, pointerPos.y, 0));
            mouseCell = grid.WorldToCell(worldPos);
            finalPos = grid.CellToWorld(mouseCell);

            currentBuilding.transform.position = new Vector3(finalPos.x, finalPos.y + (buildingYSize / 2) + 0.25f, finalPos.y + (buildingYSize / 2) + 0.25f);

            if (mouseCell != oldCellPos)
                FindNewTiles();

            ClearOldTiles();

            if (CanBuild() && Input.GetMouseButtonDown(0))
            {
                if (buildingNumber == 1)
                {
                    ResourceManager.Instance.AddGoldMineBuilding();
                    ResourceManager.Instance.SpendGoldCoins(buildingInfo.buildingType);
                }
                PlaceBuilding();
                soundFXManager.PlaySound(placeBuildingSound, currentBuilding.transform.position, 1f);
                CleanUp();
                Build(buildingNumber);
            } 
            else if (Input.GetMouseButtonDown(1))
            {
                CleanUp();
                this.enabled = false;
            }
        }
    }

    private void PlaceBuilding()
    {
        GameObject buildingToPlace = Instantiate(currentBuilding, currentBuilding.transform.position, Quaternion.identity);
        buildingToPlace.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
        buildingToPlace.GetComponent<SpriteRenderer>().sortingOrder = 2;
        buildingToPlace.transform.SetParent(placedBuildings.transform);

        List<Vector3Int> placedCells = new List<Vector3Int>();

        foreach (Vector3Int newCell in newCells)
        {
            placedBuildingsTiles.SetTile(newCell, gridTile);
            placedCells.Add(newCell);
            if (decorationMap.HasTile(newCell))
                decorationMap.SetTile(newCell, null);
            
            if (!ignoredCells.Contains(newCell) || ignoredCells.Count == 0)
                flowfieldPathfinding.buildingMap.Add(newCell, buildingToPlace);
        }

        flowfieldPathfinding.buildingCells.Add(buildingToPlace, placedCells);
        flowfieldPathfinding.mapChanged++;
    }

    private void FindNewTiles()
    {
        int index = 0;

        oldCells = (Vector3Int[])newCells.Clone();
        ignoredCells = new List<Vector3Int>();

        Type buildingType = currentBuilding.GetComponent<BuildingInfo>().buildingType;

        int xInitial = -1;
        int yInitial = -1;
        if (currentBuilding != null && (buildingType == Type.Tower || buildingType == Type.Wall))
        {
            xInitial = 0;
            yInitial = 0;
        }

        for (int x = xInitial; x < buildingInfo.size.x; ++x)
        {
            for (int y = yInitial; y < buildingInfo.size.y; ++y)
            {
                if (x == -1 || y == -1)
                    ignoredCells.Add(new Vector3Int(mouseCell.x + x - 2, mouseCell.y + y - 2, 0));
                newCells[index] = new Vector3Int(mouseCell.x + x - 2, mouseCell.y + y - 2, 0);
                ++index;
            }
        }

        oldCellPos = mouseCell;
    }

    private void ClearOldTiles()
    {
        if (oldCells == null) return;
        foreach (Vector3Int oldCell in oldCells)
        {
            if (newCells.Contains(oldCell)) continue;
            buildingGrid.SetTile(oldCell, null);
        }
    }

    private void CleanUp()
    {
        Destroy(currentBuilding);

        foreach (Vector3Int oldCell in oldCells)
            buildingGrid.SetTile(oldCell, null);

        foreach (Vector3Int newCell in newCells)
            buildingGrid.SetTile(newCell, null);

        oldCells = null;
        newCells = null;
    }

    private bool CanBuild()
    {
        bool canBuild = true;
        foreach (Vector3Int newCell in newCells)
        {
            buildingGrid.SetTile(newCell, gridTile);
            buildingGrid.SetTileFlags(newCell, TileFlags.None);

            bool onValidGroundTile = true;
            bool notBlockedByDecoration = true;

            foreach (TileBase legalGroundTile in legalGroundTiles)
            {
                if (tileMap.GetTile(newCell).Equals(legalGroundTile))
                {
                    onValidGroundTile = true;
                    break;
                }
                onValidGroundTile = false;
            }

            foreach (TileBase legalDecorationTile in legalDecorationTiles)
            {
                if (!decorationMap.HasTile(newCell) || decorationMap.GetTile(newCell).Equals(legalDecorationTile))
                {
                    notBlockedByDecoration = true;
                    break;
                }
                notBlockedByDecoration = false;
            }

            bool notBlockedByBuilding = placedBuildingsTiles.HasTile(newCell);

            if (onValidGroundTile && !notBlockedByBuilding && notBlockedByDecoration)
            {
                buildingGrid.SetColor(newCell, new Color(0, 1, 0, 0.4f));
                continue;
            }
            buildingGrid.SetColor(newCell, new Color(1, 0, 0, 0.4f));
            canBuild = false;
        }

        return canBuild;
    }
}
