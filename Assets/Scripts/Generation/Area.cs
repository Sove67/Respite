using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Area : MonoBehaviour // This class holds data on which edges of a sector border another sector of the same ID.
{
    public enum tileType { exterior, border, interior, partition, connection };

    private Vector2Int size { get; }
    private int areaResolution { get; }
    private int minDimension { get; }
    private int primaryConnectionCount { get; }
    private bool divideSectors { get; }
    private List<Vector2Int> exterior { get; }
    private List<Vector2Int> interior { get; }
    private tileType[,] tiles { get; }
    private int[,] sectors { get; }

    // Constructor
    public Area(Vector2Int location, Vector2Int size, int areaResolution, int minDimension, int primaryConnectionCount, bool divideSectors, int seed)
    {
        this.size = size;
        this.areaResolution = areaResolution;
        this.minDimension = minDimension;
        this.primaryConnectionCount = primaryConnectionCount;
        this.divideSectors = divideSectors;
        tiles = new tileType[size.x, size.y];
        sectors = new int[size.x, size.y];

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                exterior.Add(new Vector2Int(x, y));
                tiles[x, y] = tileType.exterior;
            }
        }

        generateBounds(seed);
    }

    // Constructor - Offset Variant
    public Area(Vector2Int location, Vector2Int offset, Vector2Int size, int areaResolution, int minDimension, int primaryConnectionCount, bool divideSectors, int seed)
    {
        this.size = size;
        this.areaResolution = areaResolution;
        this.minDimension = minDimension;
        this.primaryConnectionCount = primaryConnectionCount;
        this.divideSectors = divideSectors;
        tiles = new tileType[size.x, size.y];
        sectors = new int[size.x, size.y];

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                exterior.Add(new Vector2Int(x, y));
                tiles[x, y] = tileType.exterior;
            }
        }

        generateBounds(seed);
    }

    // Set a tile
    public void setTile(tileType type, Vector2Int location)
    {
        exterior.Remove(location);
        interior.Add(location);
        tiles[location.x, location.y] = type;
    }

    /*
     * Generates an outline - A very blocky shape, like a bunch of squares meshed together.
     * For each new square:
     * Generate a third point within "interior"
     * Generate a fourth point within "exterior"
     * Mark all border and exterior parts in the interior "interior"
     * Mark the outline that isn't interior "border"
     */
    private void generateBounds(int seed)
    {
        Vector4 bounds = new Vector4(0, 0, size.x, size.y);
        for (int i = 0; i < areaResolution; i++)
        {

            // Generate a square with the first point within the interior, and the second in the exterior
            Vector2Int point1 = interior[Generate_World.GenerateIntWithSeed(i, seed, interior.Count)];
            Vector2Int point2 = exterior[Generate_World.GenerateIntWithSeed(i, seed, exterior.Count)];
            Vector4 dimensions = new Vector4(point1.x, point1.y, point2.x, point2.y);

            // Mark horizontal borders
            for (int x = (int)dimensions.x; x < dimensions.z; x++)
            {
                setTile(tileType.border, new Vector2Int(x, (int)dimensions.y));
                setTile(tileType.border, new Vector2Int(x, (int)dimensions.w));
            }
            // Mark vertical borders
            for (int y = (int)dimensions.y; y < dimensions.w; y++)
            {
                setTile(tileType.border, new Vector2Int((int)dimensions.z, y));
                setTile(tileType.border, new Vector2Int((int)dimensions.x, y));
            }
            // Mark interior
            for (int x = (int)dimensions.x + 1; x < dimensions.z - 1; x++)
            {
                for (int y = (int)dimensions.y + 1; y < dimensions.w - 1; y++)
                { setTile(tileType.interior, new Vector2Int(x, y)); }
            }
        }
    }

    /* Generate primary connections
     * 
     * get all border tiles
     * choose a doorCount amount of tiles with a seeded randomizer
     * set those tiles to connection
     */
    private void generatePrimaryConnections(int seed)
    {

    }

    //if divide sections, generate partitions
    //choose an axis
    //choose a value within bounds
    //check that no border or partition tiles are on neighbouring axis within range: minDimension
    //choose two border or partition tiles
    //set all interior tiles between them to partition
    private void generateSections(int seed)
    {

    }

    //if divide sections, generate secondary connections
    private void generateSecondaryConnections(int seed)
    {

    }
}
