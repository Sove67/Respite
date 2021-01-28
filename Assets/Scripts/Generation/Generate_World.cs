using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Generate_World : MonoBehaviour
{

    /* Town Procedure
     * divide area into rectangles (with "streets" separating each rectangle)  -BASED ON SEED
     * set some rectangles to open space based on target density (Instantiate building parents)  -BASED ON SEED
     * Inside each "occupied" rectangle, Generate a blocky shape, and mark exits/entrances  -BASED ON SEED
     * subdivide the blocky shape until the minimum room size has been met (width or height)  -BASED ON SEED
     * Mark doorways between rooms
     * Instantiate tileset based on floorplan
     */

    /* Ruin Procedure
     * 
     * Same as town, but with less density
     */

    /* Forest Procedure
     * 
     * Instantiate trees/boulders at random coordinates  -BASED ON SEED
     */

    public Renderer textureRender;
    public int seed;

    [Header("Chunk Settings")] // The rarity settings are comparative. 100 on all is the same as 1 on all.
    public int chunkSize;
    public int areaResolution;
    public int minDimension;
    [Range(1, 100)]
    public int townRarity;
    [Range(1, 100)]
    public int forestRarity;
    [Range(1, 100)]
    public int nestRarity;

    private float nestThreshold;
    private float townThreshold;
    
    private enum chunkType {town,forest,nest};

    public void Start()
    {
        // Calculate thresholds
        int total = nestRarity + forestRarity + townRarity;
        townThreshold = townRarity / total;
        nestThreshold = nestRarity / total;

        Vector2Int chunkCoord = new Vector2Int(0, 0);
        int primaryConnectionCount = 3;
        Area area = new Area(chunkCoord, new Vector2Int(chunkSize, chunkSize), areaResolution, minDimension, primaryConnectionCount, true, seed);

    }

    public void Update()
    {       
        /* 
         * Run Forest/Ruin/Town Procedure when player comes within range of new chunks
         * Show the chunks when player returns to generated chunks
         * hide the generated objects when player leaves range
         */
    }

    // Functions
    public void GenerateChunk(Vector2 position)
    {
        // Is the chunk hidden? If so, show it
        // else, generate a new chunk:
        GenerateType(position);
    }

    private chunkType GenerateType(Vector2 position)
    {
        System.Random prng = new System.Random(seed); //Start a new randomizer based on a seed

        float offsetX = prng.Next(-100000, 100000) + position.x;
        float offsetY = prng.Next(-100000, 100000) + position.y;

        float perlinValue = Mathf.PerlinNoise(offsetX, offsetY);

        chunkType type;
        if (perlinValue < nestThreshold) { type = chunkType.nest; }
        else if (perlinValue > townThreshold) { type = chunkType.town; }
        else { type = chunkType.forest; }

        return type;
    }

    public static int GenerateIntWithSeed(int input, int seed, int range) //generates a number between 0 (inclusive) and range (inclusive)
    {
        float perlinValue = Mathf.PerlinNoise(input, seed);
        return (int)(perlinValue * range);
    }

    public void GenerateSeed() // Generates a random int for the seed of the world
    { seed = (int)Random.Range(float.MinValue, float.MaxValue); }
}
