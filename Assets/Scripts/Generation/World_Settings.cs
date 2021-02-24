using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class World_Settings : ScriptableObject
{
    [Header("Generation Settings")]
    public int activeChunkRadius;

    [Header("Chunk Settings")] // The chance settings are comparative. 100 on all is the same as 1 on all.
    public int seed;
    public int chunkTiles;
    public Vector2Int forestRange;
    public int tileSize;
    public Material grass;

    [Header("Area Settings")]
    [Range(1, 10)]
    public int attempts;

    [Header("Town")]
    public int townAreaResolution;
    public int townMinDimension;
    public int townPrimaryConnectionCount;
    public int townSecondaryConnectionCount;
    public int townSectorCount;
    public List<GameObject> tilesetWallInterior;
    public List<GameObject> tilesetWallExterior;

    [Header("House")]
    public int houseAreaResolution;
    public int houseMinDimension;
    public int housePrimaryConnectionCount;
    public int houseSecondaryConnectionCount;
    public int houseSectorCount;
    public List<GameObject> tilesetHouseInterior;
    public List<GameObject> tilesetHouseExterior;

    [Header("Nest")]
    public int nestAreaResolution;
    public int nestMinDimension;
    public int nestPrimaryConnectionCount;
    public int nestSecondaryConnectionCount;
    public int nestSectorCount;
    public List<GameObject> tilesetNestInterior;
    public List<GameObject> tilesetNestExterior;
}
