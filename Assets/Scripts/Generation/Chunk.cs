using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public World_Settings settings;
    public ChunkTile[,] contents;
    public List<GameObject> tileset;

    private int tileCount;
    private int tileRandomizerRange;
    private int tileScale;
    private static readonly float CARDINAL_ROTATION_INTERVAL = 90;

    private readonly static Vector2Int[] POSITIONAL_OFFSETS = new Vector2Int[]
    {
        new Vector2Int(-1, -1),
        new Vector2Int( 0, -1),
        new Vector2Int( 1, -1),
        new Vector2Int(-1,  0),
        new Vector2Int( 1,  0),
        new Vector2Int( 1,  1),
        new Vector2Int( 0,  1),
        new Vector2Int(-1,  1),
    };

    private void Start()
    {
        tileCount = settings.tileCount;
        tileRandomizerRange = settings.tileRandomizerRange;
        tileScale = settings.tileScale;

        contents = new ChunkTile[tileCount, tileCount];

        GenerateChunk();
        InstantiateChunk();
    }

    private void GenerateChunk()
    {
        for (int y = 0; y < tileCount; y++)
        {
            for (int x = 0; x < tileCount; x++)
            { contents[x, y] = new ChunkTile(Random.Range(0, 0 + tileRandomizerRange)); }
        }


        int id = 0;
        for (int y = 0; y < tileCount; y++)
        {
            for (int x = 0; x < tileCount; x++)
            {
                
                if (!contents[x, y].sectorID.HasValue)
                {
                    contents[x, y].sectorID = id;
                    int value = contents[x, y].sectorValue;
                    Crawler(new Vector2Int(x,y), value, id);
                    id++;
                }
            }
        }
    }

    private void Crawler(Vector2Int origin, int value, int id) // Assign any connected tile to the same sector, then repeats the check on neighbours
    {
        //for each connected tile
        for (int i = 0; i < POSITIONAL_OFFSETS.Length; i++)
        {
            Vector2Int position = origin + POSITIONAL_OFFSETS[i];

            // If targeted position is within bounds and matches the tile value
            if (position.x >= 0 && position.x < tileCount
                && position.y >= 0 && position.y < tileCount
                && contents[position.x, position.y].sectorValue == value)
            {
                // If sector ID is unassigned, assign value
                if (contents[position.x, position.y].sectorID == null)
                { 
                    contents[position.x, position.y].sectorID = id;

                    //view the tile's neighbours
                    Crawler(position, value, id);
                }

                // Mark the connections
                contents[origin.x, origin.y].connected[i] = true;
                int oppositeSide = (i + 2) % POSITIONAL_OFFSETS.Length;
                contents[position.x, position.y].connected[oppositeSide] = true;
            }
        }
    }

    private void InstantiateChunk()
    {
        for (int y = 0; y < tileCount; y++)
        {
            for (int x = 0; x < tileCount; x++)
            {
                contents[x, y].MatchPatterns();

                foreach ((int, int) pattern in contents[x, y].patternSet)
                {
                    float rotation = pattern.Item2 * CARDINAL_ROTATION_INTERVAL;
                    Instantiate(tileset[pattern.Item1], new Vector3(x * tileScale, 0, y * tileScale), Quaternion.Euler(0, rotation, 0), transform);
                }
            }
        }
    }
}

