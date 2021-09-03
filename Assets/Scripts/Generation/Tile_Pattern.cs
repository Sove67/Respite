using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Tile_Pattern
{
    public enum value { T, F, E }; // True, False, Either

    // Connection patterns for each mesh type
    public static readonly value[][,] patterns = new value[][,]{
        // Big L
        new value[,] {
        { value.E, value.T, value.E},
        { value.F, value.T, value.T},
        { value.F, value.F, value.E} },

        // Little L
        new value[,] {
        { value.E, value.T, value.F},
        { value.E, value.T, value.T},
        { value.E, value.E, value.E} },

        // Pipe
        new value[,] {
        { value.E, value.T, value.E},
        { value.E, value.T, value.F},
        { value.E, value.T, value.E} }
    };

    // Check if bool matches a given pattern value (True, False, or Either)
    public static bool CompareBool(bool boolean, value patternValue)
    {
        bool output = false;
        output |= patternValue == value.E;
        output |= patternValue == value.T && boolean;
        output |= patternValue == value.F && !boolean;

        return output;
    }


    // Rotate a pattern by 90 degrees.
    public static value[,] RotatePattern(value[,] array)
    {
        if (array.GetLength(0) != array.GetLength(1))
        { throw new System.ArgumentException(); }

        int dimension = array.GetLength(0);
        value[,] result = new value[dimension, dimension];
        for (int x = 0; x < dimension; x++)
        {
            for (int y = 0; y < dimension; y++)
            {
                value transposed = array[y, x];
                int invertedIndex = dimension - 1 - y;
                result[x, invertedIndex] = transposed;
            }
        }

        return result;
    }
}
