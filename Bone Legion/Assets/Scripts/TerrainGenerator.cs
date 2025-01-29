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
     * Scale: 10
     * Frequency: 0.1
     * Amplitude: 0.657
     */

    // Map generation information
    [SerializeField]
    private Vector2Int mapSize;
    [SerializeField]
    private Tilemap environmentMap;
    [SerializeField]
    private TileBase[] tiles;
    // Map generation information

    // Perlin noise parameters
    [Range(1f, 10f)]
    [SerializeField]
    private float scale;
    [Range(0.0001f, 1f)]
    [SerializeField]
    private float frequency;
    [Range(0.0001f, 1)]
    [SerializeField]
    private float amplitude;
    [Range(0.0001f, 1f)]
    private float seed;
    // Perlin noise parameters

    private float[,] mapSamples;

    private Dictionary<TileBase, List<Vector3Int>> tileDict;

    private List<TileBase> groundTile;
    private List<TileBase> waterTile;

    // 128x128 is a good size for chunks.
    [SerializeField]
    private Vector2Int chunkSize;
    private bool mapGenerated;
    private bool chunkGenerating;
    private Vector2Int currentChunks;

    void Start()
    {
        //GenerateMapChunked();

        mapGenerated = true;
    }

    public void GenerateMapChunked()
    {
        seed = Random.value;

        groundTile = new List<TileBase>();
        waterTile = new List<TileBase>();

        for (int i = 0; i < chunkSize.x * chunkSize.y; i++)
        {
            groundTile.Add(tiles[0]);
            waterTile.Add(tiles[1]);
        }

        mapSamples = new float[chunkSize.x, chunkSize.y];

        chunkGenerating = false;
        mapGenerated = false;
        currentChunks = new Vector2Int(0, 0);

        tileDict = new Dictionary<TileBase, List<Vector3Int>>();
        tileDict.Add(tiles[0], new List<Vector3Int>());
        tileDict.Add(tiles[1], new List<Vector3Int>());
    }

    public void GenerateChunkWhole()
    {
        seed = Random.value;

        groundTile = new List<TileBase>();
        waterTile = new List<TileBase>();

        for (int i = 0; i < mapSize.x * mapSize.y; i++)
        {
            groundTile.Add(tiles[0]);
            waterTile.Add(tiles[1]);
        }

        mapSamples = new float[mapSize.x, mapSize.y];

        currentChunks = new Vector2Int(0, 0);

        tileDict = new Dictionary<TileBase, List<Vector3Int>>();
        tileDict.Add(tiles[0], new List<Vector3Int>());
        tileDict.Add(tiles[1], new List<Vector3Int>());

        DrawMap(new Vector2Int(0, 0), mapSize);
        SetTiles();
    }

    // Chunked Map Generation
    private void Update()
    {
        if (!mapGenerated && !chunkGenerating)
        {
            Vector2Int maxBounds = new Vector2Int(currentChunks.x, currentChunks.y);

            // Increment the max bounds by 128 until it reaches the max size on its specific axis.
            if ((maxBounds.x + chunkSize.x) / mapSize.x != 0 && maxBounds.x + chunkSize.x != mapSize.x)
                maxBounds.x += mapSize.x % chunkSize.x;
            else maxBounds.x += chunkSize.x;
            if ((maxBounds.y + chunkSize.y) / mapSize.y != 0 && maxBounds.y + chunkSize.y != mapSize.y)
                maxBounds.y+= mapSize.y % chunkSize.y;
            else maxBounds.y += chunkSize.y;

            DrawMap(currentChunks, maxBounds);
            SetTiles();

            currentChunks.x += chunkSize.x;
            if (mapSize.x - currentChunks.x <= 0)
            {
                currentChunks.x = 0;
                currentChunks.y += chunkSize.y;

                if (mapSize.y - currentChunks.y <= 0)
                    mapGenerated = true;
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
                float sampleX = (float)x / scale;
                float sampleY = (float)y / scale;

                mapSamples[xIndex, yIndex] += amplitude * Mathf.PerlinNoise(sampleX * frequency + seed, sampleY * frequency + seed);

                if (mapSamples[xIndex, yIndex] > 0.2)
                {
                    List<Vector3Int> tileList = tileDict[tiles[0]];
                    tileList.Add(new Vector3Int(x, y, 0));
                    tileDict[tiles[0]] = tileList;
                }
                else
                {
                    List<Vector3Int> tileList = tileDict[tiles[1]];
                    tileList.Add(new Vector3Int(x, y, 0));
                    tileDict[tiles[1]] = tileList;
                }

                ++yIndex;
            }
            yIndex = 0;
            ++xIndex;
        }
    }

    private void SetTiles()
    {
        environmentMap.SetTiles(tileDict[tiles[0]].ToArray(), groundTile.ToArray());
        environmentMap.SetTiles(tileDict[tiles[1]].ToArray(), waterTile.ToArray());

        tileDict[tiles[0]] = new List<Vector3Int>();
        tileDict[tiles[1]] = new List<Vector3Int>();

        mapSamples = new float[chunkSize.x, chunkSize.y];
        chunkGenerating = false;

        // For profiler demonstration
        /*
        foreach (Vector2Int tilePos in tileDict[tiles[0]])
            environmentMap.SetTile(new Vector3Int(tilePos.x, tilePos.y, 0), tiles[0]);

        foreach (Vector2Int tilePos in tileDict[tiles[1]])
            environmentMap.SetTile(new Vector3Int(tilePos.x, tilePos.y, 0), tiles[1]);
        */
    }
}
