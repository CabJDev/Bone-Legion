using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Tilemaps;

struct TileNode
{
    public Vector3Int[] cellNeighbours { get; }
    public int cost { set; get; }
    public int value { set; get; }
    public Vector3Int bestDirection { set; get; }

    public TileNode(Vector3Int[] cellNeighbours)
    {
        this.cellNeighbours = cellNeighbours;
        cost = 1;
        value = int.MaxValue;
        bestDirection = new Vector3Int(0, 0, 0);
    }
}

public class FlowfieldPathfinding : MonoBehaviour
{
    [SerializeField] private Tilemap placedBuildings;
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap decorations;

    [SerializeField] private Grid grid;

    [SerializeField] private TileBase[] groundWhitelist;
    [SerializeField] private TileBase[] decorationBlacklist;

    private TerrainGenerator terrainGenerator;

    public Dictionary<Vector3Int, GameObject> buildingMap = new Dictionary<Vector3Int, GameObject>();
    public Dictionary<GameObject, List<Vector3Int>> buildingCells = new Dictionary<GameObject, List<Vector3Int>>();
    private Dictionary<Vector3Int, TileNode> nodeInformation;
    private Queue<Vector3Int> openNodes;
    private List<Vector3Int> finishedNodes;

    public bool gridInitialized = false;

    public int mapChanged = 0;
    private bool generatingFlowfield = false;

    private async void Update()
    {
        if (generatingFlowfield == false && mapChanged > 0)
        {
            generatingFlowfield = true;
            await Task.Run(() => { GenerateNewFlowField(); });
            generatingFlowfield = false;
            mapChanged--;
        }
    }

    public Vector3 GetDirection(Vector3 currentPosition)
    {
        Vector3Int positionInCell = grid.WorldToCell(currentPosition);

        if (!nodeInformation.ContainsKey(positionInCell)) return new Vector3Int(0, 0, 0);
        return grid.CellToWorld(nodeInformation[positionInCell].bestDirection);
    }

    public GameObject Attacking(Vector3 currentPosition)
    {
		Vector3Int positionInCell = grid.WorldToCell(currentPosition);

        if (!nodeInformation.ContainsKey(positionInCell)) return null;
        if (!buildingMap.ContainsKey(positionInCell)) return null;
        return buildingMap[positionInCell];
	}

    public void GenerateInitialFlowField(TerrainGenerator terrainGenerator)
    {
        this.terrainGenerator = terrainGenerator;

        nodeInformation = new Dictionary<Vector3Int, TileNode>();
        openNodes = new Queue<Vector3Int>();
        finishedNodes = new List<Vector3Int>();

		GetGroundDecorationCost();
		GetBuildingCost();
		GetGoalTiles();
		CalculateNodeValues();
		CreateFlowField();

		gridInitialized = true;
    }

    public void GenerateNewFlowField()
    {
        ResetValues();
        GetBuildingCost();
        GetGoalTiles();
        CalculateNodeValues();
        CreateFlowField();
    }

    private void GetGroundDecorationCost()
    {
        for (int y = 0; y < terrainGenerator.GetMapSize().y; ++y)
        {
            for (int x = 0; x < terrainGenerator.GetMapSize().x; ++x)
            {
                Vector3Int currentCell = new Vector3Int(x, y, 0);
                int cost = 1;

                if (!groundWhitelist.Contains(groundTilemap.GetTile(currentCell)))
                    cost = 2;

                if (decorations.HasTile(currentCell) && decorationBlacklist.Contains(decorations.GetTile(currentCell)))
                    cost = 2;

                Vector3Int[] neighbours = new Vector3Int[8];
                Vector3Int leftCell = new Vector3Int(x - 1, y, 0);
                Vector3Int rightCell = new Vector3Int(x + 1, y, 0);
                Vector3Int upCell = new Vector3Int(x, y + 1, 0);
                Vector3Int downCell = new Vector3Int(x, y - 1, 0);
                Vector3Int leftUpCell = new Vector3Int(x - 1, y + 1, 0);
				Vector3Int leftDownCell = new Vector3Int(x - 1, y - 1, 0);
				Vector3Int rightUpCell = new Vector3Int(x + 1, y + 1, 0);
				Vector3Int rightDownCell = new Vector3Int(x + 1, y - 1, 0);
				Vector3Int noCell = new Vector3Int(-1, -1, -1);

                if (groundTilemap.HasTile(leftCell)) neighbours[0] = leftCell;
                else neighbours[0] = noCell;
                if (groundTilemap.HasTile(rightCell)) neighbours[1] = rightCell;
                else neighbours[1] = noCell;
                if (groundTilemap.HasTile(upCell)) neighbours[2] = upCell;
                else neighbours[2] = noCell;
                if (groundTilemap.HasTile(downCell)) neighbours[3] = downCell;
                else neighbours[3] = noCell;
				if (groundTilemap.HasTile(leftUpCell)) neighbours[4] = leftUpCell;
				else neighbours[4] = noCell;
				if (groundTilemap.HasTile(leftDownCell)) neighbours[5] = leftDownCell;
				else neighbours[5] = noCell;
				if (groundTilemap.HasTile(rightUpCell)) neighbours[6] = rightUpCell;
				else neighbours[6] = noCell;
				if (groundTilemap.HasTile(rightDownCell)) neighbours[7] = rightDownCell;
				else neighbours[7] = noCell;

				TileNode tileNode = new TileNode(neighbours);
                tileNode.cost = cost;
                tileNode.value = int.MaxValue;

                nodeInformation.Add(currentCell, tileNode);
            }
        }
    }

    private void ResetValues()
    {
        Vector3Int[] cells = nodeInformation.Keys.ToArray(); 
        foreach (Vector3Int cell in cells)
        {
            TileNode newTile = nodeInformation[cell];
            newTile.value = int.MaxValue;
            nodeInformation[cell] = newTile;
        }

        finishedNodes = new List<Vector3Int>();
    }

    private void GetBuildingCost()
    {
        foreach (Vector3Int cell in buildingMap.Keys)
        {
            TileNode node = nodeInformation[cell];
            node.cost = 2;
        }
    }

    private void GetGoalTiles()
    {
		TileNode currentNode;

        foreach (Vector3Int cell in buildingMap.Keys)
        {
            currentNode = nodeInformation[cell];
            currentNode.value = 0;
            nodeInformation[cell] = currentNode;

            openNodes.Enqueue(cell);
        }
    }

    private void CalculateNodeValues()
    {
		TileNode currentNode;
        if (openNodes.Count <= 0) return;
		while (openNodes.Count > 0)
        {
            Vector3Int currentPos = openNodes.Dequeue();

            currentNode = nodeInformation[currentPos];
            finishedNodes.Add(currentPos);

            if (currentNode.value == int.MaxValue) continue;

            foreach (Vector3Int neighbour in currentNode.cellNeighbours)
            {
                if (!nodeInformation.ContainsKey(neighbour)) continue;
                TileNode neighbourNode = nodeInformation[neighbour];
                
                if (neighbourNode.value == 0) continue;
                if (neighbourNode.cost == 2)
                {
					if (!finishedNodes.Contains(neighbour) && !openNodes.Contains(neighbour))
						openNodes.Enqueue(neighbour);
					continue;
                }
                
                int newValue = currentNode.value + neighbourNode.cost;
				if (newValue < neighbourNode.value)
                {
                    neighbourNode.value = newValue;
                    nodeInformation[neighbour] = neighbourNode;

                    if (!finishedNodes.Contains(neighbour) && !openNodes.Contains(neighbour))
                        openNodes.Enqueue(neighbour);
                }
			}
        }
    }

    private void CreateFlowField()
    {
		TileNode currentNode;

		foreach (Vector3Int cell in finishedNodes)
        {
            currentNode = nodeInformation[cell];
            Vector3Int bestCell = cell;

            foreach (Vector3Int neighbour in currentNode.cellNeighbours)
            {
                if (!nodeInformation.ContainsKey(neighbour)) continue;
                if (nodeInformation[neighbour].value < nodeInformation[bestCell].value)
                    bestCell = neighbour;
            }

            currentNode.bestDirection = bestCell - cell;

            nodeInformation[cell] = currentNode;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
	{
        if (Application.isPlaying)
        {
			GUIStyle style = new GUIStyle(GUI.skin.label);
			foreach (Vector3Int cell in nodeInformation.Keys)
			{
				Vector3 pos = grid.CellToWorld(cell);
                pos.y += 0.25f;
				Handles.Label(pos, nodeInformation[cell].value.ToString(), style);
			}
		}
	}
#endif
}
