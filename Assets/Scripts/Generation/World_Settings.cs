using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class World_Settings : ScriptableObject
{
    [Header("Generation")]
    public int activeChunkRadius;

    [Header("Chunk")] // The chance settings are comparative. 100 on all is the same as 1 on all.
    public int seed;
    public int tileCount;
    public int tileScale;
    public int tileRandomizerRange;
    public List<GameObject> tileset;
    //public Material grass;

    //[Header("Area Settings")]
    //[Range(1, 10)]
    //public int attempts;
}
