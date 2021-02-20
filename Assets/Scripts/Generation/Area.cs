using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Area // This class holds data on which edges of a sector border another sector of the same ID.
{
    // Tile Logic
    public enum tileType { exterior, border, interior, partition, connection, invalid };

    public class TileGroup
    {
        public tileType[,] typeMap { get; }

        //Constructor
        public TileGroup(Vector2Int size)
        {
            typeMap = new tileType[size.x, size.y];
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    typeMap[x, y] = tileType.exterior;
                }
            }
        }
        public TileGroup(List<Vector2Int> area, Vector2Int size) //Invalid tiles support
        {
            typeMap = new tileType[size.x, size.y];
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    setTile(x, y, tileType.invalid);
                }
            }

            foreach (Vector2Int point in area)
            {
                setTile(point, tileType.exterior);
            }
        }

        // Set a tile
        public void setTile(int x, int y, tileType type)
        {
            typeMap[x, y] = type;
        }

        public void setTile(Vector2Int location, tileType type) // Vector2Int Support
        { setTile(location.x, location.y, type); }

        // return coords of typeMap of type
        public List<Vector2Int> getListOfType(tileType type)
        {
            List<Vector2Int> coords = new List<Vector2Int>();
            for (int x = 0; x < typeMap.GetLength(0); x++)
            {
                for (int y = 0; y < typeMap.GetLength(1); y++)
                {
                    if (typeMap[x, y] == type)
                    {
                        coords.Add(new Vector2Int(x, y));
                    }
                }
            }

            return coords;
        }
        public List<Vector2Int> getListOfType(List<tileType> type)
        {
            List<Vector2Int> coords = new List<Vector2Int>();
            for (int x = 0; x < typeMap.GetLength(0); x++)
            {
                for (int y = 0; y < typeMap.GetLength(1); y++)
                {
                    if (type.Contains(typeMap[x, y]))
                    {
                        coords.Add(new Vector2Int(x, y));
                    }
                }
            }

            return coords;
        }

        // Get the connections of one tile [L,R,U,D]
        public bool[] getConnections(Vector2Int pos)
        {
            int x = pos.x;
            int y = pos.y;
            bool[] connections = new bool[4];
            // Left
            connections[0] = (x - 1 >= 0 && typeMap[x, y] == typeMap[x - 1, y]) ? true : false;
            // Right
            connections[1] = (x + 1 < typeMap.GetLength(0) && typeMap[x, y] == typeMap[x + 1, y]) ? true : false;
            // Up
            connections[2] = (y + 1 < typeMap.GetLength(1) && typeMap[x, y] == typeMap[x, y + 1]) ? true : false;
            // Down
            connections[3] = (y - 1 >= 0 && typeMap[x, y] == typeMap[x, y - 1]) ? true : false;

            return connections;
        }        
        public bool[] getConnections(Vector2Int pos, List<Vector2Int> tilesToCheck) // For connecting to any tile within a list
        {
            int x = pos.x;
            int y = pos.y;
            bool[] connections = new bool[4];
            for (int i = 0; i < cardinalOffsets.Length; i++)
            {
                Vector2Int scanTile = pos + cardinalOffsets[i];
                connections[i] = tilesToCheck.Contains(scanTile);
            }

            return connections;
        }
        public bool[] getConnections(Vector2Int pos, tileType validConnectionType) // For connecting a different tile type
        {
            int x = pos.x;
            int y = pos.y;
            bool[] connections = new bool[4];
            // Left
            connections[0] = (x - 1 >= 0 && validConnectionType == typeMap[x - 1, y]) ? true : false;
            // Right
            connections[1] = (x + 1 < typeMap.GetLength(0) && validConnectionType == typeMap[x + 1, y]) ? true : false;
            // Up
            connections[2] = (y + 1 < typeMap.GetLength(1) && validConnectionType == typeMap[x, y + 1]) ? true : false;
            // Down
            connections[3] = (y - 1 >= 0 && validConnectionType == typeMap[x, y - 1]) ? true : false;

            return connections;
        }
        public bool[] getConnections(Vector2Int pos, List<tileType> validConnectionTypes) // For connecting several tile types
        {
            int x = pos.x;
            int y = pos.y;
            bool[] connections = new bool[4];
            // Left
            connections[0] = (x - 1 >= 0 && validConnectionTypes.Contains(typeMap[x - 1, y])) ? true : false;
            // Right
            connections[1] = (x + 1 < typeMap.GetLength(0) && validConnectionTypes.Contains(typeMap[x + 1, y])) ? true : false;
            // Up
            connections[2] = (y + 1 < typeMap.GetLength(1) && validConnectionTypes.Contains(typeMap[x, y + 1])) ? true : false;
            // Down
            connections[3] = (y - 1 >= 0 && validConnectionTypes.Contains(typeMap[x, y - 1])) ? true : false;

            return connections;
        }

        public tileType[,] getNeighbouringTiles(Vector2Int pos)
        {
            int x = pos.x;
            int y = pos.y;
            tileType[,] connections = new tileType[3,3];

            connections[0, 0] = x - 1 >= 0 && y - 1 >= 0
                ? typeMap[x - 1, y - 1] : tileType.invalid;
            connections[1, 0] = y - 1 >= 0
                ? typeMap[x, y - 1] : tileType.invalid;
            connections[2, 0] = x + 1 < typeMap.GetLength(0) && y - 1 >= 0
                ? typeMap[x + 1, y - 1] : tileType.invalid;

            connections[0, 1] = x - 1 >= 0
                ? typeMap[x - 1, y] : tileType.invalid;
            connections[1, 1] = typeMap[x, y];
            connections[2, 1] = x + 1 < typeMap.GetLength(0)
                ? typeMap[x + 1, y] : tileType.invalid;

            connections[0, 2] = x - 1 >= 0 && y + 1 < typeMap.GetLength(1)
                ? typeMap[x - 1, y + 1] : tileType.invalid;
            connections[1, 2] = y + 1 < typeMap.GetLength(1)
                ? typeMap[x, y + 1] : tileType.invalid;
            connections[2, 2] = x + 1 < typeMap.GetLength(0) && y + 1 < typeMap.GetLength(1)
                ? typeMap[x + 1, y + 1] : tileType.invalid;

            return connections;
        }

        //reset the type and connection maps
        public void reset()
        {
            for (int x = 0; x < typeMap.GetLength(0); x++)
            {
                for (int y = 0; y < typeMap.GetLength(1); y++)
                {
                    if (typeMap[x, y] != tileType.invalid)
                    { typeMap[x, y] = tileType.exterior; }
                }
            }
        }
    }

    public class OrderClockwise : IComparer<Vector2Int>
    {
        Vector2Int center;

        public OrderClockwise(Vector2Int center)
        { this.center = center; }

        public int Compare(Vector2Int a, Vector2Int b)
        {
            Vector2Int one = a - center;
            Vector2Int two = b - center;
            float angle1 = (Mathf.Atan2(one.x, one.y) + (2 * Mathf.PI)) % (2 * Mathf.PI);
            float angle2 = (Mathf.Atan2(two.x, two.y) + (2 * Mathf.PI)) % (2 * Mathf.PI);
            float difference = angle1 * Mathf.Rad2Deg - angle2 * Mathf.Rad2Deg;
            return (int)difference;
        }
    }

    public class OrderPartition : IComparer<(int, List<Vector2Int>, Vector2Int)>
    {
        public int Compare((int, List<Vector2Int>, Vector2Int) x, (int, List<Vector2Int>, Vector2Int) y)
        {
            return x.Item2.Count - y.Item2.Count;
        }
    }

    public class Type
    {
        public int areaResolution { get; }
        public int minDimension { get; }
        public int primaryConnectionCount { get; }
        public int secondaryConnectionCount { get; }
        public int sectorCount { get; }

        public Type(int areaResolution, int minDimension, int primaryConnectionCount, int secondaryConnectionCount, int sectorCount)
        {
            this.areaResolution = areaResolution;
            this.minDimension = minDimension;
            this.primaryConnectionCount = primaryConnectionCount;
            this.secondaryConnectionCount = secondaryConnectionCount;
            this.sectorCount = sectorCount;
        }
    }

    private static Vector2Int[] cardinalOffsets = new Vector2Int[] // {L,R,U,D}
    {
        new Vector2Int(-1, 0),
        new Vector2Int( 1, 0),
        new Vector2Int( 0, 1),
        new Vector2Int( 0,-1)
    };

    private Vector2Int size;
    private List<Vector2Int> area;


    // Output
    public Vector2Int offset;
    public TileGroup tileMap { get; }
    public List<List<Vector2Int>> sectors { get; }
    public Type parameters;
    public List<GameObject> tileset { get; }


    // Constructor
    public Area(int seed, int attempts, Vector2Int offset, Vector2Int size, Type parameters)
    {
        // Initialize Values
        Random.InitState((int) (seed * (offset.x + .5f) * (offset.y + .5f)));

        this.size = size;
        this.parameters = parameters;
        this.offset = offset;
        sectors = new List<List<Vector2Int>>();
        tileMap = new TileGroup(size);

        generateArea(attempts);
    }

    public Area(int seed, int attempts, Vector2Int offset, List<Vector2Int> area, Type parameters) //Invalid tiling support
    { 
        // Initialize Values
        Random.InitState((int)(seed * (offset.x + .5f) * (offset.y + .5f)));

        (Vector2Int, Vector2Int) bounds = getRect(area);
        for (int i = 0; i < area.Count; i++) // set lowest coords to 0.
        {
            area[i] -= bounds.Item1;
        }
        size = new Vector2Int(bounds.Item2.x - bounds.Item1.x + 1, bounds.Item2.y - bounds.Item1.y + 1);
        this.area = area;
        this.parameters = parameters;
        this.offset = offset;
        sectors = new List<List<Vector2Int>>();
        tileMap = new TileGroup(area, size);

        generateArea(attempts);
    }

    public void generateArea(int attempts)
    {
        // Generate Bounds
        if (!regenerateErrors(generateBounds, attempts))
        { boundsFallBack(); }
        //Debug.Log("Bounds:\n" + printType());

        // Generate Partitions
        bool partitionSuccess = regenerateErrors(generatePartition, attempts, parameters.sectorCount -1);
        //Debug.Log("Partitions:\n" + printType());

        // Generate Sections
        generateSectors();
        //Debug.Log("Sectors:\n" + printSectors());

        generatePrimaryConnections(parameters.primaryConnectionCount);
        if (parameters.secondaryConnectionCount > 0 && sectors.Count > 0)
        { generateSecondaryConnections(parameters.secondaryConnectionCount); }
        //Debug.Log("Connections:\n" + printType());

        Debug.Log("Final:\n" + printType());
    }


    // Regenerators
    public bool regenerateErrors(System.Func<bool> method, int retryCount) // No param
    {
        bool success;
        int fails = 0;
        do
        {
            bool result = method();
            success = result;
            fails += result ? 0 : 1;
        }
        while (!success && fails <= retryCount);
        if (fails > retryCount)
        { Debug.LogError("Failed to call " + method.Method.Name + "."); }

        return success;
    }

    public bool regenerateErrors<T>(System.Func<T, bool> method, int retryCount, T param) // No param
    {
        bool success;
        int fails = 0;
        do
        {
            bool result = method(param);
            success = result;
            fails += result ? 0 : 1;
        }
        while (!success && fails <= retryCount);
        if (fails > retryCount)
        { Debug.LogError("Failed to call " + method.Method.Name + "."); }

        return success;
    }


    // Bounds Generation
    private bool generateBounds()
    {
        tileMap.reset();
        List<Vector2Int> validtileMap = tileMap.getListOfType(tileType.exterior);
        List<Vector2Int> points = new List<Vector2Int>();
        OrderClockwise order = new OrderClockwise(size / 2);

        for (int i = 0; i <= parameters.areaResolution; i++)
        {
            bool success = false;
            while (!success)
            {
                if (validtileMap.Count == 0) { Debug.LogWarning("Ran out of valid tileMap."); return false; }
                // Generate a new point
                int index = Random.Range(0, validtileMap.Count);
                Vector2Int newPoint = validtileMap[index];
                validtileMap.RemoveAt(index);
                points.Add(newPoint);

                // Find where that point is sorted
                points.Sort(order);
                int proposedIndex = points.FindIndex(item => item == newPoint);

                // Check the midpoints (if there are any)
                int lastIndex = proposedIndex - 1;
                int nextIndex = proposedIndex + 1;
                if (points.Count == parameters.areaResolution - 1) //if last point, wrapping is valid
                { nextIndex %= (points.Count); }

                success = true; // If neither midpoint is possible
                if (lastIndex >= 0) // If the last point is possible
                {
                    Vector2Int lastMidPoint = new Vector2Int(points[lastIndex].x, newPoint.y);
                    success &= validtileMap.Contains(lastMidPoint); // if the point is valid
                }
                if (nextIndex < points.Count) // if the next point is possible
                {
                    Vector2Int nextMidPoint = new Vector2Int(newPoint.x, points[nextIndex].y);
                    success &= validtileMap.Contains(nextMidPoint); // if the point is valid
                }

                if (success)
                { points.Sort(order); }
                else
                { points.Remove(newPoint); }
            }
        }

        /* For each point:
         * 
         * Alternating x and y,
         * get the next points value, and create a midpoint and the current points's opposing value and the next points value
         * fill the space between with border tileMap
         * insert the new point in the list
         * 
         * If the path crosses out of the scope of the area, return false (prompting a regeneration of the bounds)
         */
        for (int i = 0; i < points.Count; i++)
        {
            int next = (i + 1) % (points.Count);
            (bool result, List<Vector2Int> borderTiles) = getLPoints(points[i], points[next], tileMap.getListOfType(tileType.border));
            if (result)
            {
                foreach (Vector2Int tile in borderTiles)
                { tileMap.setTile(tile, tileType.border); }
            }
            else
            {
                //Debug.Log("Bounds Attempt:\n" + printType());
                return (false);
            }
        }

        // Roughly fill interior
        foreach (Vector2Int tile in tileMap.getListOfType(tileType.border))
        {
            Vector2Int scanTile = tile + cardinalOffsets[1];
            
            while (scanTile.x < tileMap.typeMap.GetLength(0) && tileMap.typeMap[scanTile.x, scanTile.y] != tileType.border)
            {
                if (tileMap.typeMap[scanTile.x, scanTile.y] == tileType.exterior)
                { tileMap.setTile(scanTile, tileType.interior); }
                scanTile += cardinalOffsets[1];
            }
        }


        // Refine Interior
        for (int x = 0; x < tileMap.typeMap.GetLength(0); x++)
        {
            if (tileMap.typeMap[x, 0] == tileType.interior)
            {
                tileMap.setTile(x, 0, tileType.exterior);
            }
            if (tileMap.typeMap[x, tileMap.typeMap.GetLength(1) - 1] == tileType.interior)
            {
                tileMap.setTile(x, tileMap.typeMap.GetLength(1) - 1, tileType.exterior);
            }
        }
        for (int y = 0; y < tileMap.typeMap.GetLength(1); y++)
        {
            if (tileMap.typeMap[0, y] == tileType.interior)
            {
                tileMap.setTile(0, y, tileType.exterior);
            }
            if (tileMap.typeMap[tileMap.typeMap.GetLength(0) - 1, y] == tileType.interior)
            {
                tileMap.setTile(tileMap.typeMap.GetLength(0) - 1, y, tileType.exterior);
            }
        }

        List<Vector2Int> borderingExterior = tileMap.getListOfType(tileType.interior).Where(tile => tileMap.getConnections(tile, tileType.exterior).Contains(true)).ToList(); // Get tiles bordering missed interior tiles

        while (borderingExterior.Count > 0)
        {
            Vector2Int tile = borderingExterior[0];
            bool[] connections = tileMap.getConnections(tile, tileType.interior);
            List<int> directions = connections.Select((b, i) => b == true ? i : -1).Where(i => i != -1).ToList();

            while (directions.Count > 0)
            {
                Vector2Int tileToCheck = tile + cardinalOffsets[directions[0]];
                borderingExterior.Add(tileToCheck);
                directions.RemoveAt(0);
            }

            tileMap.setTile(tile, tileType.exterior);
            borderingExterior.RemoveAt(0);
        }


        // Remove 0 Width Corridors
        foreach (Vector2Int tile in tileMap.getListOfType(tileType.border))
        {
            bool hasInterior = false;
            foreach (tileType type in tileMap.getNeighbouringTiles(tile))
            {
                hasInterior |= type == tileType.interior;
            }
            if (!hasInterior)
            {
                tileMap.setTile(tile, tileType.exterior);
            }
        }

        return true;
    }

    private void boundsFallBack()
    {
        //Reset map
        List<Vector2Int> failedTiles = tileMap.getListOfType(new List<tileType>() { tileType.border, tileType.interior });
        foreach (Vector2Int tile in failedTiles)
        { tileMap.setTile(tile, tileType.exterior); }
        Debug.Log("Starting Fallback.\n" + printType());

        //Get all exterior tiles that border non-exterior tiles
        List<Vector2Int> outerTiles = tileMap.getListOfType(tileType.exterior).Where(tile => tileMap.getConnections(tile, tileType.exterior).Contains(false)).ToList();

        //Set tiles to border
        foreach (Vector2Int tile in outerTiles)
        { tileMap.setTile(tile, tileType.border); }

        //Add corner tiles
        List<bool[]> cornerPattern = new List<bool[]>
        {
            //  LRUD
            new bool[] { true, false, true, false }, //LU
            new bool[] { true, false, false, true }, //LD
            new bool[] { false, true, true, false }, //RU
            new bool[] { false, true, false, true }  //RD
        };
        List<Vector2Int> cornerTiles = new List<Vector2Int>();

        foreach (Vector2Int tile in outerTiles)
        {
            bool[] exterior = tileMap.getConnections(tile, tileType.border);
            if (exterior.Where(item => item).ToArray().Length == 1)
            {
                bool[] both = tileMap.getConnections(tile, new List<tileType>() { tileType.border, tileType.invalid });

                bool match = false;
                foreach (bool[] pattern in cornerPattern)
                { match |= Enumerable.SequenceEqual(pattern, both); }
                if (match)
                {
                    int connection = System.Array.IndexOf(exterior, true);
                    /* 0 1 2 3
                     * L R U D
                     * 
                     * 0 > 1
                     * 1 > 0
                     * 
                     * 2 > 3
                     * 3 > 2
                     */
                    int opposite = connection / 2 + ((connection + 1) % 2);
                    cornerTiles.Add(tile + cardinalOffsets[opposite]);
                }
            }
        }

        //Set tiles to border
        foreach (Vector2Int tile in cornerTiles)
        { tileMap.setTile(tile, tileType.border); }

        //Get all remaining exterior tiles and set them to interior
        foreach (Vector2Int tile in tileMap.getListOfType(tileType.exterior))
        { tileMap.setTile(tile, tileType.interior); }
    }

    private (bool, List<Vector2Int>) getLPoints(Vector2Int a, Vector2Int c, List<Vector2Int> markedPoints)
    {
        List<Vector2Int> newBorder = new List<Vector2Int>();
        List<Vector2Int> newInterior = new List<Vector2Int>();
        Vector2Int minMaxX = a.x < c.x ?
            new Vector2Int(a.x, c.x) :
            new Vector2Int(c.x, a.x);

        Vector2Int minMaxY = a.y < c.y ?
            new Vector2Int(a.y, c.y) :
            new Vector2Int(c.y, a.y);

        for (int x = minMaxX.x; x <= minMaxX.y; x++)
        {
            int y = a.y;
            Vector2Int tile = new Vector2Int(x, y);
            if (tileMap.typeMap[x, y] == tileType.invalid || markedPoints.Contains(tile)) { return (false, newBorder); }

            newBorder.Add(tile);
        }

        for (int y = minMaxY.x + 1; y < minMaxY.y; y++)
        {
            int x = c.x;
            Vector2Int tile = new Vector2Int(x, y);
            if (tileMap.typeMap[x, y] == tileType.invalid || markedPoints.Contains(tile)) { return (false, newBorder); }

            newBorder.Add(tile);
        }

        return (true, newBorder);
    }


    // Section Generation
    private bool generatePartition(int count)
    {
        List<Vector2Int> oldPartitions = tileMap.getListOfType(tileType.partition);
        foreach (Vector2Int tile in oldPartitions)
        { tileMap.setTile(tile, tileType.interior); }

        List<List<Vector2Int>> partitions = new List<List<Vector2Int>>();
        List<Vector2Int> validPartitiontiles = tileMap.getListOfType(tileType.border);
        while (partitions.Count < count && validPartitiontiles.Count > 0)
        {
            List<Vector2Int> newTiles = new List<Vector2Int>();
            // Repeat the following until successful
            bool madePartition = false;

            while (!madePartition && validPartitiontiles.Count > 0)
            {
                string log = "\nTrying new tile.";
                //Select a border or partition tile
                Vector2Int startPoint = validPartitiontiles[Random.Range(0, validPartitiontiles.Count)];
                //Generate valid directions [L,R,U,D]
                bool[] connections = tileMap.getConnections(startPoint, tileType.interior);
                bool[] pendingCardinalTiles = tileMap.getConnections(startPoint, validPartitiontiles);
                for (int i = 0; i < connections.Length; i++)
                {
                    connections[i] = connections[i] & !pendingCardinalTiles[i];
                }
                List<int> validDirections = connections.Select((b, i) => b == true ? i : -1).Where(i => i != -1).ToList();

                log += "\nStarted tile " + startPoint + ". Direction use: " + connections[0] + connections[1] + connections[2] + connections[3];
                //Loop through valid directions until success or you run out
                while (!madePartition && validDirections.Count > 0)
                {
                    newTiles.Clear();
                    int directionIndex = Random.Range(0, validDirections.Count);
                    int direction = validDirections[directionIndex];
                    log += "\nTrying new direction: " + direction + ".";

                    Vector2Int step = cardinalOffsets[direction];
                    bool clear = true;
                    Vector2Int pos = startPoint;
                    do // crawl along an axis to the next tile
                    {
                        pos += step;
                        log += "\nCrawled to next tile: " + pos + ".";

                        // Scan center
                        tileType scanType = tileMap.typeMap[pos.x, pos.y];
                        if (scanType == tileType.interior) // add each step to list 
                        { newTiles.Add(pos); }

                        if (scanType == tileType.border || partitions.Any(item => item.Contains(pos)))// Successfully created a partition
                        {
                            clear = false;
                            if (newTiles.Count >= parameters.minDimension)
                            {
                                madePartition = true;
                                log += "\nDirection " + direction + " succeeded at " + pos + ".";
                            }
                            else
                            {
                                log += "\nDirection " + direction + " failed because it was too short (" + newTiles.Count + ") at " + pos + ".";
                            }
                        }
                        else if (scanType == tileType.invalid || scanType == tileType.exterior)// Partition entered invalid zone
                        {
                            clear = false;
                            log += "\nDirection " + direction + " failed because of invalid zone at " + pos + ".";
                        }
                        else
                        {
                            // Scan sides
                            for (int i = -parameters.minDimension; i <= parameters.minDimension && clear; i++)
                            {
                                Vector2Int scanTile = Vector2Int.zero;
                                switch (direction)
                                {
                                    case 0:
                                    case 1:
                                        scanTile = pos + new Vector2Int(0, i);
                                        break;
                                    case 2:
                                    case 3:
                                        scanTile = pos + new Vector2Int(i, 0);
                                        break;
                                }

                                if (scanTile.x >= 0 && scanTile.y >= 0 && scanTile.x < size.x && scanTile.y < size.y // Check that the tile is within bounds
                                    && i != 0) // Skip the center tile
                                {
                                    scanType = tileMap.typeMap[scanTile.x, scanTile.y];
                                    if (scanType == tileType.border || partitions.Any(item => item.Contains(scanTile)) || scanType == tileType.invalid)  // Match obstacle tileMap
                                    {
                                        clear = false;
                                        log += "\nDirection " + direction + " failed because of obstacle at " + scanTile + ".";
                                    }
                                }
                            }
                        }

                        // Scan next tile for edge
                        if ((pos + step).x < 0 || (pos + step).y < 0 ||
                            (pos + step).x >= tileMap.typeMap.GetLength(0) || (pos + step).y >= tileMap.typeMap.GetLength(1))
                        {
                            clear = false;
                            log += "\nDirection " + direction + " failed because of edge at " + (pos + step) + ".";
                        }
                    }
                    while (clear); //haven't reached another border/partition, haven't encountered blockage or edge
                    validDirections.RemoveAt(directionIndex);

                    if (madePartition)
                    {
                        foreach (var tile in newTiles) // set each step in list to partition
                        { validPartitiontiles.Add(tile); }
                        //Debug.Log("Successful partition from " + startPoint + " of length: " + newTiles.Count + "." + log);
                    }
                }
                if (validDirections.Count == 0)
                {
                    validPartitiontiles.Remove(startPoint);
                    //Debug.Log("Failed partition from " + startPoint + "." + log + "\n" + validPartitiontiles.Count + " valid tiles remain."); 
                }
            }
            if (madePartition)
            { partitions.Add(newTiles); }
        }

        foreach (List<Vector2Int> tileList in partitions)
        {
            foreach (Vector2Int tile in tileList)
            { tileMap.setTile(tile, tileType.partition); }
        }
        if (partitions.Count == count)
        { return true; }
        else
        { 
            //Debug.Log("Partitons failed: only " + partitions.Count + " generated."); 
            return false;
        }
    }

    private void generateSectors()
    {
        /*
         * Generates sections
         * 
         * removes partitions from sector 0
         * crawls sector 0 tileMap, adding to a sector until empty
         * repeats previous step until there are no remaining tileMap
         * removes the empty priming stack
         */
        // Create list of unassigned sector tileMap
        List<Vector2Int> tileList = tileMap.getListOfType(tileType.interior);
        sectors.Add(tileList);

        int index = 1;
        while (sectors[0].Count > 0)
        {
            sectorCrawler(sectors[0][0], index);
            index++;
        }
        sectors.RemoveAt(0);
    }

    private void sectorCrawler(Vector2Int position, int id) // Assign any cardinally connected tileMap to the same sector, then repeats the check on neighbours
    {
        //for each cardinally connected tile
        foreach (Vector2Int offset in cardinalOffsets)
        {
            Vector2Int tile = position + offset;
            if (tile.x >= 0 && tile.x <= tileMap.typeMap.GetLength(0)
                && tile.y >= 0 && tile.y <= tileMap.typeMap.GetLength(1)
                && sectors[0].Contains(tile))
            {
                //remove the tile from the stack, and add it to the sector
                sectors[0].Remove(tile);

                if (sectors.Count > id)
                { sectors[id].Add(tile); }
                else { sectors.Add(new List<Vector2Int> { tile }); }

                //view the tile's neighbours
                sectorCrawler(tile, id);
            }
        }
    }

    private void generatePrimaryConnections(int quota)
    {
        // Generate primary connections in border wall
        int connections = 0;
        List<Vector2Int> borders = tileMap.getListOfType(tileType.border);
        while (connections < quota && borders.Count > 0)
        {
            Vector2Int tile = borders[Random.Range(0, borders.Count)];
            borders.Remove(tile);
            bool[] connectionExterior = tileMap.getConnections(tile, new List<tileType>() { tileType.exterior, tileType.invalid });
            bool[] connectionInterior = tileMap.getConnections(tile, tileType.interior);
            bool isXEdge = tile.x == 0 || tile.x == tileMap.typeMap.GetLength(0);
            bool isYEdge = tile.y == 0 || tile.y == tileMap.typeMap.GetLength(1);
            if (connectionExterior.Contains(true) && connectionInterior.Contains(true))
            {
                // Mark sector as empty
                int direction = System.Array.FindIndex(connectionInterior, item => item);
                int sectorIndex = sectors.FindIndex(sector => sector.Contains(tile + cardinalOffsets[direction]));

                // Set connection
                tileMap.setTile(tile, tileType.connection);
                connections++;
            }
            else if (!tileMap.getConnections(tile, tileType.invalid).Contains(true) && (isXEdge && !isYEdge || isYEdge && !isXEdge))
            {
                // Set connection
                tileMap.setTile(tile, tileType.connection);
                connections++;
            }
        }
    }

    private void generateSecondaryConnections(int max)
    {
        // Get valid partitions for each sector
        List<Vector2Int> interior = tileMap.getListOfType(tileType.interior);
        List<(int, List<Vector2Int>, Vector2Int)> sectorPartitions = new List<(int, List<Vector2Int>, Vector2Int)>(); //List of sectors, each sector has a list of points and a connection counter + max
        for (int a = 0; a < sectors.Count; a++)
        {
            // Get interior tileMap bordering partiton tileMap
            List<Vector2Int> partitiontileMap = new List<Vector2Int>();
            Vector2Int[] sectortileMap = interior.Where(tile => sectors[a].Contains(tile)).ToArray();
            List<(Vector2Int, List<int>)> edgetileMap = new List<(Vector2Int, List<int>)>();

            // Get directions of partition tileMap
            foreach (Vector2Int tile in sectortileMap)
            {
                List<int> tileConnections = tileMap.getConnections(tile, tileType.partition) // Get the directions bordering partitions
                    .Select((direction, i) => direction == true ? i : -1).Where(i => i != -1).ToList(); // get the direction indexes as a list
                edgetileMap.Add((tile, tileConnections));
            }

            // Mark sector's partition tileMap
            foreach (var tile in edgetileMap)
            {
                foreach (int direction in tile.Item2)
                { partitiontileMap.Add(tile.Item1 + cardinalOffsets[direction]); }
            }

            // Remove tiles that only touch one sector
            partitiontileMap.RemoveAll(tile => tileMap.getConnections(tile).Where(connection => connection).Count() >= 2);

            // Get the number of partitions for the tile
            Vector2Int connections = new Vector2Int(0, Random.Range(1, Mathf.Clamp(max + 1, 1, partitiontileMap.Count)));

            // Add data to sector
            sectorPartitions.Add((a, partitiontileMap, connections));
        }

        // Sort sectors by partition count
        OrderPartition order = new OrderPartition();
        sectorPartitions.Sort(order);

        // Create connections, starting with the sector with the least partitions
        while (sectorPartitions.Any(index => index.Item3.x < index.Item3.y))
        {
            // Get First sector that has less than 1 connections
            int sectorIndex = sectorPartitions.FindIndex(index => index.Item3.x < index.Item3.y);

            // Get valid tileMap
            List<Vector2Int> sectortileMap = sectorPartitions[sectorIndex].Item2;
            List<(int, List<Vector2Int>, Vector2Int)> fullSectors = sectorPartitions.Where(sector => sector.Item3.x >= sector.Item3.y).ToList();
            List<Vector2Int> validtileMap = sectortileMap.Where(item => fullSectors.TrueForAll(sector => !sector.Item2.Contains(item))).ToList();

            if (validtileMap.Count > 0)
            {
                // Get a random valid tile and the sectors it's contained in
                Vector2Int tile = validtileMap[Random.Range(0, validtileMap.Count)];
                List<int> overlappingSectorIndexes = sectorPartitions.Select((sector, i) => sector.Item2.Contains(tile) ? i : -1).Where(i => i != -1).ToList();

                // Set that tile
                tileMap.setTile(tile, tileType.connection);

                // Update partition data
                foreach (int index in overlappingSectorIndexes)
                {
                    sectorPartitions[index].Item2.Remove(tile);

                    // Have to completely reconstuct to set the vector2int value for some reason.
                    sectorPartitions[index] = (sectorPartitions[index].Item1, sectorPartitions[index].Item2,
                        new Vector2Int(sectorPartitions[index].Item3.x + 1, sectorPartitions[index].Item3.y));
                }
            }
            else
            {
                Debug.Log("Sector " + sectorPartitions[sectorIndex].Item1 + " ran out of valid tileMap to make connections with. " +
                    "\nIt had " + sectorPartitions[sectorIndex].Item3.x + " of " + sectorPartitions[sectorIndex].Item3.y + " connections.");

                sectorPartitions[sectorIndex] = (sectorPartitions[sectorIndex].Item1, sectorPartitions[sectorIndex].Item2,
                    new Vector2Int(sectorPartitions[sectorIndex].Item3.y, sectorPartitions[sectorIndex].Item3.y));
            }

        }
    }

    public static (Vector2Int, Vector2Int) getRect(List<Vector2Int> pointList)
    {
        Vector2Int min = new Vector2Int(int.MaxValue, int.MaxValue);
        Vector2Int max = new Vector2Int(int.MinValue, int.MinValue);
        foreach (Vector2Int point in pointList)
        {
            min.x = point.x < min.x ? point.x : min.x;
            min.y = point.y < min.y ? point.y : min.y;

            max.x = point.x > max.x ? point.x : max.x;
            max.y = point.y > max.y ? point.y : max.y;
        }

        return (min, max);
    }


    // Value Generation
    public Vector2Int getSectorOffset(int i)
    {
        if (i < sectors.Count)
        {
            Vector2Int offset = sectors[i][0];
            foreach (Vector2Int tile in sectors[i])
            {
                if (tile.x < offset.x) //Get left most tile
                {
                    offset = tile;
                } else if (tile.x == offset.x && tile.y < offset.y) //Get top most tile in range of leftmost tile
                {
                    offset = tile;
                }
            }
            return offset;
        }
        else
        { throw new System.Exception("Sector not found."); }
    }

    // Debugging
    private string printType()
    {
        string typeMap = "";
        for (int y = tileMap.typeMap.GetLength(1) - 1; y >= 0; y--)
        {
            for (int x = 0; x < tileMap.typeMap.GetLength(0); x++)
            {

                if (x % 5 == 0) // Space groups of 5
                {
                    typeMap += " ";
                }
                //▥▦▤▧▨▩▨◪◩◧◨◫◤◣◢◥
                if (tileMap.typeMap[x, y] == tileType.exterior)
                { typeMap += "□"; }
                else if (tileMap.typeMap[x, y] == tileType.border)
                { typeMap += "■"; }
                else if (tileMap.typeMap[x, y] == tileType.interior)
                { typeMap += "◫"; }
                else if (tileMap.typeMap[x, y] == tileType.partition)
                { typeMap += "▧"; }
                else if (tileMap.typeMap[x, y] == tileType.connection)
                { typeMap += "◤"; }
                else if (tileMap.typeMap[x, y] == tileType.invalid)
                { typeMap += "▤"; }
            }
            typeMap += "\n";
            if (y % 5 == 0) // Space groups of 5
            {
                typeMap += "\n";
            }
        }
        return typeMap;
    }

    public string printSectors()
    {
        string sectorReferenceList = "□ABCDEFGHJKLN0PQRSTXYZ"; // Removed some letters, as unity's debugger doesn't equally space them.
        string sectorMap = "";
        int[,] compiledSectors = new int[tileMap.typeMap.GetLength(0), tileMap.typeMap.GetLength(1)];

        // Generate an empty array
        for (int x = 0; x < compiledSectors.GetLength(0); x++)
        {
            for (int y = 0; y < compiledSectors.GetLength(1); y++)
            {
                compiledSectors[x, y] = 0;
            }
        }

        // Fill array with sectors
        for (int i = 0; i < sectors.Count; i++)
        {
            foreach (var tile in sectors[i])
            {
                compiledSectors[tile.x, tile.y] = i+1;
            }
        }

        // print array
        for (int y = compiledSectors.GetLength(1) - 1; y >= 0; y--)
        {
            for (int x = 0; x < compiledSectors.GetLength(0); x++)
            {
                if (x % 5 == 0) // Space groups of 5
                {
                    sectorMap += "\t";
                }
                sectorMap += sectorReferenceList.ToCharArray()[compiledSectors[x, y]];
            }
            sectorMap += "\n";
            if (y % 5 == 0) // Space groups of 5
            {
                sectorMap += "\n";
            }
        }
        return sectorMap;
    }
}