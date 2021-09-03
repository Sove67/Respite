using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkTile
{
    // Generation Data
    public int sectorValue { get; set; }
    public int? sectorID { get; set; }
    public bool[] connected { get; set; }
    public List<(int, int)> patternSet { get; private set; }

    // Static Constraints
    private static readonly int CONNECTION_SIZE = 3;
    private static readonly int CARDINAL_DIRECTIONS = 4;

    // Initializer
    public ChunkTile(int value)
    {
        sectorValue = value;
        connected = new bool[8];
    }

    // Get patterns and rotations for current connection state
    public void MatchPatterns()
    {
        // Convert array to matrix
        bool[,] parsedConnections = new bool[,] {
            { connected[0], connected[1], connected[2]},
            { connected[3], false,        connected[4]},
            { connected[5], connected[6], connected[7]} };

        patternSet = new List<(int, int)>();

        // For each cardinal rotation of each pattern in Tile_Pattern
        for (int a = 0; a < Tile_Pattern.patterns.Length; a++)
        {
            Tile_Pattern.value[,] pattern = Tile_Pattern.patterns[a];
            for (int b = 0; b < CARDINAL_DIRECTIONS; b++)
            {
                pattern = Tile_Pattern.RotatePattern(pattern);

                // Check if the values in the connection matrix matches the pattern matrix
                bool match = true;
                for (int y = 0; y < CONNECTION_SIZE; y++)
                {
                    for (int x = 0; x < CONNECTION_SIZE; x++)
                    {
                        match |= Tile_Pattern.CompareBool(parsedConnections[x, y], pattern[x, y]);
                    }
                }
                if (match) 
                { patternSet.Add((a, b)); }
            }
        }
    }
}
