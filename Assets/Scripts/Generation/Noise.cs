using UnityEngine;
using System.Collections;

// Taken from Sebastian Lauge's series: https://www.youtube.com/watch?v=MRNFcywkUSA
public static class Noise
{
    public static float[,] GenerateChunk(int chunkSize, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[chunkSize, chunkSize];
        System.Random prng = new System.Random(seed); //Start a new randomizer based on a seed

        // For each octave, generate a random offset
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++) 
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0) // Verifcation
        {
            scale = 0.0001f;
        }


        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = chunkSize / 2f;
        float halfHeight = chunkSize / 2f;


        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {

                float amplitude = 1; //Determines the weight of the values
                float frequency = 1; //Determines the volatility of the values
                float noiseHeight = 0;

                //calculates the perlin noise for each octave
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance; //Each octave decreases the weight
                    frequency *= lacunarity; //Each octave increases the volatility
                }

                //Finds the lowest and highest values generated
                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                //Assigns one point in the noise map
                noiseMap[x, y] = noiseHeight;
            }
        }


        // Changes the noisemap into values between 0 and 1
        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }
}