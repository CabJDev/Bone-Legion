// Author: John Cabusora (J.C.)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/*
 * Terrain generation using Unity's tilemaps and perlin noise functions.
 */
public class TerrainGenerator : MonoBehaviour
{
    /*
     * Good values to generate map
     * Scale: 4
     * Frequency: 0.1
     * Amplitude: 0.657
     */

    // Map generation information
    [SerializeField]
    private Vector2Int mapSize;
    [SerializeField]
    private Tilemap environmentMap;
    [SerializeField]
    private TileBase[] validMapTiles;
    // Map generation information

    // Decoration generation information
    [SerializeField]
    private Tilemap decorationMap;
    [SerializeField]
    private TileBase[] validDecorationTiles;

    // Camera
    [SerializeField]
    private Camera mainCam;

    // World mapSeed
    private float mapSeed;
    private float decorationSeed;

    // Map perlin noise parameters
    [Range(1f, 10f)]
    [SerializeField]
    private float mapScale;
    [Range(0.0001f, 1f)]
    [SerializeField]
    private float mapFrequency;
    [Range(0.0001f, 1)]
    [SerializeField]
    private float mapAmplitude;
    // Map perlin noise parameters

    // Decoration perlin noise parameters
    [Range(1f, 10f)]
    [SerializeField]
    private float decorationScale;
    [Range(0.0001f, 1f)]
    [SerializeField]
    private float decorationFrequency;
    [Range(0.0001f, 1)]
    [SerializeField]
    private float decorationAmplitude;
    // Decoration perlin noise parameters

    private float[,] mapSamples;
    private float[,] decorationSamples;

    private Dictionary<TileBase, List<Vector3Int>> mapDict;

    private List<TileBase> groundTile;
    private List<TileBase> waterTile;

    private Dictionary<int, List<Vector3Int>> decorationDict;
    private Dictionary<int, List<TileBase>> decorationTile;

    // 64x64 is a good size for chunks.
    [SerializeField]
    private Vector2Int chunkSize;
    private bool mapGenerated;
    private bool chunkGenerating;
    private Vector2Int currentChunks;

    // Protected area where there is no water or decorations
    [SerializeField]
    private int protectedRadius;
    private Vector2Int mapCentre;

    // Stuff to initialize the first building
    [SerializeField]
    private GameObject castle;

    [SerializeField]
    private Grid grid;

    [SerializeField]
    private GameObject placedBuildings;
    [SerializeField]
    private Tilemap placedBuildingsTiles;
    [SerializeField]
    private TileBase gridTile;

    private Vector3 finalPos;
    // Stuff to initialize the first building

    [SerializeField] FlowfieldPathfinding flowfieldPathfinding;

    public bool mapBuildingGenerated;

    void Start()
    {
        mainCam = Camera.main;
        mainCam.transform.position = new Vector3(0, mapSize.y / 4, -5);
        mapBuildingGenerated = false;

        mapCentre = new Vector2Int(Mathf.FloorToInt(mapSize.x / 2), Mathf.FloorToInt(mapSize.y / 2));

        int index = 0;

        decorationTile = new Dictionary<int, List<TileBase>>();
        decorationDict = new Dictionary<int, List<Vector3Int>>();

        foreach (TileBase decoration in validDecorationTiles)
        {
            decorationDict.Add(index, new List<Vector3Int>());
            ++index;
        }

        GenerateMapChunked();

        PlaceCastle();
    }

    private void PlaceCastle()
    {
        finalPos = grid.CellToWorld(new Vector3Int(mapSize.x / 2, mapSize.y / 2, 0));

        SpriteRenderer spriteRenderer = castle.GetComponent<SpriteRenderer>();

        float buildingYSize = spriteRenderer.size.y;

        GameObject buildingToPlace = Instantiate(castle, new Vector3(finalPos.x, finalPos.y + (buildingYSize / 2), finalPos.y + (buildingYSize / 2)), Quaternion.identity);
        buildingToPlace.transform.SetParent(placedBuildings.transform);

        Vector2Int buildingSize = buildingToPlace.GetComponent<BuildingInfo>().size;

		List<Vector3Int> placedCells = new List<Vector3Int>();

		for (int x = -1; x < buildingSize.x; ++x)
        {
			for (int y = -1; y < buildingSize.y; ++y)
			{
                Vector3Int gridPos = new Vector3Int(mapSize.x / 2 + x, mapSize.y / 2 + y, 0);
				placedBuildingsTiles.SetTile(gridPos, gridTile);

                placedCells.Add(gridPos);
                if (x > -1 && y > -1)
                {
                    flowfieldPathfinding.buildingMap.Add(gridPos, buildingToPlace);
                }
			}
		}
        flowfieldPathfinding.buildingCells.Add(buildingToPlace, placedCells);
    }

    public Vector3 GetCastlePosition()
    {
        return finalPos;
    }

    private void GenerateMapChunked()
    {
        mapSeed = Random.value;
        decorationSeed = Random.value;

        groundTile = new List<TileBase>();
        waterTile = new List<TileBase>();

        for (int i = 0; i < chunkSize.x * chunkSize.y; i++)
        {
            groundTile.Add(validMapTiles[0]);
            waterTile.Add(validMapTiles[1]);
        }

        for (int i = 0; i < validDecorationTiles.Length; i++)
        {
            decorationTile.Add(i, new List<TileBase>());
            for (int j = 0; j < chunkSize.x * chunkSize.y; j++)
            {
                decorationTile[i].Add(validDecorationTiles[i]);
            }
        }

        mapSamples = new float[chunkSize.x, chunkSize.y];
        decorationSamples = new float[chunkSize.x, chunkSize.y];

        chunkGenerating = false;
        mapGenerated = false;
        currentChunks = new Vector2Int(0, 0);

        mapDict = new Dictionary<TileBase, List<Vector3Int>>();
        mapDict.Add(validMapTiles[0], new List<Vector3Int>());
        mapDict.Add(validMapTiles[1], new List<Vector3Int>());
    }

    // Chunked Map Generation
    private void Update()
    {
        if (!mapGenerated && !chunkGenerating)
        {
            Vector2Int maxBounds = new Vector2Int(currentChunks.x, currentChunks.y);

            // Increment the max bounds by the given chunk size until it reaches the max size on its specific axis.
            if ((maxBounds.x + chunkSize.x) / mapSize.x != 0 && maxBounds.x + chunkSize.x != mapSize.x)
                maxBounds.x += mapSize.x % chunkSize.x;
            else maxBounds.x += chunkSize.x;
            if ((maxBounds.y + chunkSize.y) / mapSize.y != 0 && maxBounds.y + chunkSize.y != mapSize.y)
                maxBounds.y+= mapSize.y % chunkSize.y;
            else maxBounds.y += chunkSize.y;

            DrawMap(currentChunks, maxBounds);
            DecorateMap(currentChunks, maxBounds);
            SetTiles();
            SetDecorations();

            chunkGenerating = false;

            currentChunks.x += chunkSize.x;
            if (mapSize.x - currentChunks.x <= 0)
            {
                currentChunks.x = 0;
                currentChunks.y += chunkSize.y;

                if (mapSize.y - currentChunks.y <= 0)
                {
                    mapGenerated = true;
					mapBuildingGenerated = true;
					mainCam.GetComponent<CameraMovement>().enabled = true;
                    flowfieldPathfinding.GenerateInitialFlowField(this);
					GameManager.Instance.UpdateGameState(GameState.GameStart);
					this.enabled = false;
                }
            }
        }
    }

    private void DrawMap(Vector2Int minBounds, Vector2Int maxBounds)
    {
        chunkGenerating = true;
        int xIndex = 0;
        int yIndex = 0;

        for (int x = minBounds.x; x < maxBounds.x; ++x)
        {
            for (int y = minBounds.y; y < maxBounds.y; ++y)
            {
                float sampleX = (float)x / mapScale;
                float sampleY = (float)y / mapScale;

                if (InProtectedArea(x, y) || x == 0 || y == 0 || x == mapSize.x - 1 || y == mapSize.y - 1) // Ensure that the centre of the map has room
                    mapSamples[xIndex, yIndex] = 1;
                else
                    mapSamples[xIndex, yIndex] += mapAmplitude * Mathf.PerlinNoise(sampleX * mapFrequency + mapSeed, sampleY * mapFrequency + mapSeed);

                if (mapSamples[xIndex, yIndex] > 0.2)
                {
                    List<Vector3Int> tileList = mapDict[validMapTiles[0]];
                    tileList.Add(new Vector3Int(x, y, 0));
                    mapDict[validMapTiles[0]] = tileList;
                }
                else
                {
                    List<Vector3Int> tileList = mapDict[validMapTiles[1]];
                    tileList.Add(new Vector3Int(x, y, 0));
                    mapDict[validMapTiles[1]] = tileList;
                }

                ++yIndex;
            }
            yIndex = 0;
            ++xIndex;
        }
    }

    private void DecorateMap(Vector2Int minBounds, Vector2Int maxBounds)
    {
        int xIndex = 0;
        int yIndex = 0;

        for (int x = minBounds.x; x < maxBounds.x; ++x)
        {
            for (int y = minBounds.y; y < maxBounds.y; ++y)
            {
                if (mapSamples[xIndex, yIndex] <= 0.2) continue;

                float sampleX = (float)x / decorationScale;
                float sampleY = (float)y / decorationScale;

                decorationSamples[xIndex, yIndex] += decorationAmplitude * Mathf.PerlinNoise(sampleX * decorationFrequency + decorationSeed, sampleY * decorationFrequency + decorationSeed);


                if (decorationSamples[xIndex, yIndex] > 0 && decorationSamples[xIndex, yIndex] < 0.15 && !InProtectedArea(x, y))
                    decorationDict[0].Add(new Vector3Int(x, y, 0));
                else if (decorationSamples[xIndex, yIndex] >= 0.29 && decorationSamples[xIndex, yIndex] <= 0.3)
                    decorationDict[1].Add(new Vector3Int(x, y, 0));
                else if (decorationSamples[xIndex, yIndex] > 0.3 && decorationSamples[xIndex, yIndex] <= 0.32)
                    decorationDict[2].Add(new Vector3Int(x, y, 0));
                else if (decorationSamples[xIndex, yIndex] > 0.32 && decorationSamples[xIndex, yIndex] <= 0.34)
                    decorationDict[3].Add(new Vector3Int(x, y, 0));

                ++yIndex;
            }
            yIndex = 0;
            ++xIndex;
        }
    }

    private bool InProtectedArea(int x, int y) { return (x - mapCentre.x) * (x - mapCentre.x) + (y - mapCentre.y) * (y - mapCentre.y) <= protectedRadius * protectedRadius; }

    private void SetDecorations()
    {
        for (int i = 0; i < decorationDict.Count; ++i)
        {
            decorationMap.SetTiles(decorationDict[i].ToArray(), decorationTile[i].ToArray());

            decorationDict[i] = new List<Vector3Int>();
            decorationSamples = new float[chunkSize.x, chunkSize.y];
        }
    }

    private void SetTiles()
    {
        environmentMap.SetTiles(mapDict[validMapTiles[0]].ToArray(), groundTile.ToArray());
        environmentMap.SetTiles(mapDict[validMapTiles[1]].ToArray(), waterTile.ToArray());

        mapDict[validMapTiles[0]] = new List<Vector3Int>();
        mapDict[validMapTiles[1]] = new List<Vector3Int>();

        mapSamples = new float[chunkSize.x, chunkSize.y];
    }

    public Vector2Int GetMapSize() { return mapSize; }
    public bool IsMapGenerated() { return mapGenerated; }
}
