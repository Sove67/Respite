using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class World : MonoBehaviour
{
    public class Chunk
    {
        public World world { get; }
        public Vector2Int position { get; }
        public GameObject self { get; }
        public Dictionary<chunkTypes, System.Action> instantiatorList { get; }

        public Area area { get; set; }
        public List<Area> sectors { get; set; }
        private static int PLANE_SIZE = 10;
        public Chunk(World world, Vector2Int position, Transform parent)
        {
            this.world = world;
            this.position = position;
            self = new GameObject("Chunk");
            self.transform.localScale = Vector3.one * world.settings.tileSize;
            self.transform.SetParent(parent);
            sectors = new List<Area>();

            float chunkSize = world.settings.tileSize * world.settings.chunkTiles;
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.localScale = new Vector3(chunkSize / PLANE_SIZE, 1, chunkSize / PLANE_SIZE);
            plane.transform.position = new Vector3(position.x * chunkSize + chunkSize / 2, 0, position.y * chunkSize + chunkSize / 2);
            plane.transform.SetParent(self.transform);
            plane.GetComponent<MeshRenderer>().material = world.settings.grass;

            instantiatorList = new Dictionary<chunkTypes, System.Action> {
                { chunkTypes.town, () => InstantiateTown() },
                { chunkTypes.forest, () => InstantiateForest() },
                { chunkTypes.nest, () => InstantiateNest() }
            };

            System.Action instantiator;
            if (instantiatorList.TryGetValue(GenerateChunkType(position), out instantiator)){
                instantiator.Invoke();
            } else
            { throw new System.Exception("Error: Chunk instantiation dictionary did not contain action for specified key."); }
        }


        public void InstantiateForest()
        {
            Debug.Log("Created Forest at " + position);
        }

        public void InstantiateTown()
        {
            Area.Type townParameters;
            if (!world.areaParameters.TryGetValue(areaTypes.town, out townParameters))
            { throw new System.Exception("Error: Area parameter dictionary did not contain values for specified key 'town'."); }

            Area.Type houseParameters;
            if (!world.areaParameters.TryGetValue(areaTypes.house, out houseParameters))
            { throw new System.Exception("Error: Area parameter dictionary did not contain values for specified key 'house'."); }
            
            Debug.Log("Creating Town Area");
            area = new Area(world.settings.seed, world.settings.attempts, position, Vector2Int.one * world.settings.chunkTiles, townParameters);
            GameObject town = new GameObject("Town");
            town.transform.SetParent(self.transform);
            town.transform.localScale = Vector3.one;

            foreach (Vector2Int tile in area.tileMap.getListOfType(Area.tileType.border))
            {
                GameObject tileParent = new GameObject("Tile " + tile);
                tileParent.transform.SetParent(town.transform);
                tileParent.transform.position = new Vector3(tile.x * world.settings.tileSize, 0, tile.y * world.settings.tileSize) + town.transform.position;
                Area.tileType[] solidTiles = new Area.tileType[] { Area.tileType.border, Area.tileType.connection };
                Area.tileType[,] neighbouringTiles = area.tileMap.getNeighbouringTiles(tile);
                Debug.Log(tile);
                InstantiateTile(neighbouringTiles, world.settings.tilesetWallExterior, solidTiles, new Area.tileType[] { Area.tileType.exterior, Area.tileType.partition, Area.tileType.invalid }, false, tileParent);
                InstantiateTile(neighbouringTiles, world.settings.tilesetWallInterior, solidTiles, new Area.tileType[] { Area.tileType.interior, Area.tileType.partition, Area.tileType.invalid }, true, tileParent);
            }

            List<Vector2Int> areaConnections = area.tileMap.getListOfType(Area.tileType.connection);
            foreach (Vector2Int tile in areaConnections)
            {
                GameObject tileParent = new GameObject("Tile " + tile);
                tileParent.transform.SetParent(town.transform);
                tileParent.transform.position = new Vector3(tile.x * world.settings.tileSize, 0, tile.y * world.settings.tileSize) + town.transform.position;
                bool[] connections = area.tileMap.getConnections(tile, new List<Area.tileType>() { Area.tileType.border, Area.tileType.partition });
                if (connections[0] && connections[1])
                { Instantiate(world.settings.tilesetWallInterior[3], tileParent.transform.position, Quaternion.Euler(0, 90, 0), tileParent.transform); }
                else if (connections[2] && connections[3])
                { Instantiate(world.settings.tilesetWallInterior[3], tileParent.transform.position, Quaternion.Euler(0, 0, 0), tileParent.transform); }
            }

            for (int i = 0; i < area.sectors.Count; i++)
            {
                Debug.Log("Creating Sector Area" + i);
                Vector2Int offset = area.getSectorOffset(i) * world.settings.tileSize;
                GameObject house = new GameObject("House " + i + ": " + offset);
                house.transform.SetParent(self.transform);
                house.transform.localScale = Vector3.one;
                house.transform.position = new Vector3(offset.x, 0, offset.y);
                Area thisSector = new Area(world.settings.seed, world.settings.attempts, offset, area.sectors[i], houseParameters);
                sectors.Add(thisSector);

                List<Vector2Int> houseTiles = thisSector.tileMap.getListOfType(new List<Area.tileType> { Area.tileType.border, Area.tileType.partition });
                foreach (Vector2Int tile in houseTiles)
                {
                    GameObject tileParent = new GameObject("Tile " + tile);
                    tileParent.transform.SetParent(house.transform);
                    tileParent.transform.position = new Vector3(tile.x * world.settings.tileSize, 0, tile.y * world.settings.tileSize) + house.transform.position;
                    Area.tileType[] solidTiles = new Area.tileType[] { Area.tileType.border, Area.tileType.partition, Area.tileType.connection };
                    Area.tileType[,] neighbouringTiles = thisSector.tileMap.getNeighbouringTiles(tile);
                    InstantiateTile(neighbouringTiles, world.settings.tilesetHouseExterior, solidTiles, new Area.tileType[] { Area.tileType.exterior, Area.tileType.invalid }, false, tileParent);
                    InstantiateTile(neighbouringTiles, world.settings.tilesetHouseInterior, solidTiles, new Area.tileType[] { Area.tileType.interior, Area.tileType.invalid }, true, tileParent);
                }

                List<Vector2Int> houseConnections = thisSector.tileMap.getListOfType(Area.tileType.connection);
                foreach (Vector2Int tile in houseConnections)
                {
                    GameObject tileParent = new GameObject("Tile " + tile);
                    tileParent.transform.SetParent(house.transform);
                    tileParent.transform.position = new Vector3(tile.x * world.settings.tileSize, 0, tile.y * world.settings.tileSize) + house.transform.position;
                    bool[] connections = thisSector.tileMap.getConnections(tile, new List<Area.tileType>() { Area.tileType.border, Area.tileType.partition });
                        if (connections[0] && connections[1])
                        { Instantiate(world.settings.tilesetWallInterior[3], tileParent.transform.position, Quaternion.Euler(0, 90, 0), tileParent.transform); }
                        else if (connections[2] && connections[3])
                        { Instantiate(world.settings.tilesetWallInterior[3], tileParent.transform.position, Quaternion.Euler(0, 0, 0), tileParent.transform); }
                }
            }

            Debug.Log("Created Town at " + position);
        }

        public void InstantiateNest()
        {
            try
            {
                Area.Type nestParameters;
                if (!world.areaParameters.TryGetValue(areaTypes.nest, out nestParameters))
                { throw new System.Exception("Error: Area parameter dictionary did not contain values for specified key 'nest'."); }

                area = new Area(world.settings.seed, world.settings.attempts, position, Vector2Int.one * world.settings.chunkTiles, nestParameters);
                Debug.Log("Created Nest at " + position);
            }
            catch (System.Exception e)
            {
                Debug.Log(e);
            }
        }

        private void InstantiateTile(Area.tileType[,] neighbouringTiles, List<GameObject> tileset, Area.tileType[] solidTiles, Area.tileType[] emptyTile, bool connection, GameObject parent)
        {
            /* Tile Patterns
             * Key
             * x - Connection is false
             * * - Connection is true
             * ? - Connection can be either
            */

            /*
               Little L Pattern
             ? * x
             ? L *
             ? ? ?
             */
            bool?[,] littleLPattern = new bool?[3, 3];
            littleLPattern[0, 0] = null;
            littleLPattern[1, 0] = null;
            littleLPattern[2, 0] = null;
            littleLPattern[0, 1] = null;
            littleLPattern[1, 1] = null;
            littleLPattern[2, 1] = true;
            littleLPattern[0, 2] = null;
            littleLPattern[1, 2] = true;
            littleLPattern[2, 2] = false;

            /*
               Big L Pattern
             ? * ?
             x ∟ *
             x x ?
             */
            bool?[,] bigLPattern = new bool?[3, 3];
            bigLPattern[0, 0] = false;
            bigLPattern[1, 0] = false;
            bigLPattern[2, 0] = null;
            bigLPattern[0, 1] = false;
            bigLPattern[1, 1] = null;
            bigLPattern[2, 1] = true;
            bigLPattern[0, 2] = null;
            bigLPattern[1, 2] = true;
            bigLPattern[2, 2] = null;

            /*
               Pipe Pattern
             ? * ?
             ? | x
             ? * ?
             */
            bool?[,] pipePattern = new bool?[3, 3];
            pipePattern[0, 0] = null;
            pipePattern[1, 0] = true;
            pipePattern[2, 0] = null;
            pipePattern[0, 1] = null;
            pipePattern[1, 1] = null;
            pipePattern[2, 1] = false;
            pipePattern[0, 2] = null;
            pipePattern[1, 2] = true;
            pipePattern[2, 2] = null;

            string tiles = "\nCardinal Tiles:";
            for (int y = 2; y >= 0; y--)
            {
                tiles += "\n";
                for (int x = 0; x < 3; x++)
                {
                    tiles += neighbouringTiles[x, y] + " ";
                }
            }
            for (int rotation = 0; rotation < 360; rotation += 90)
            {
                bool littleLValid = true;
                bool bigLValid = true;
                bool pipeValid = true;
                bool littleLHasEmpty = false;
                bool bigLHasEmpty = false;
                bool pipeHasEmpty = false;
                for (int x = 0; x < neighbouringTiles.GetLength(0); x++)
                {
                    for (int y = 0; y < neighbouringTiles.GetLength(1); y++)
                    {
                        Area.tileType tile = neighbouringTiles[x, y];
                        List<bool?> validTypes = new List<bool?> { null };

                        if (solidTiles.Contains(tile))
                        { validTypes.Add(true); }
                        else if (emptyTile.Contains(tile))
                        {
                            validTypes.Add(false);

                            littleLHasEmpty |= littleLPattern[x, y] == false;
                            bigLHasEmpty |= bigLPattern[x, y] == false;
                            pipeHasEmpty |= pipePattern[x, y] == false;
                        }

                        littleLValid &= validTypes.Contains(littleLPattern[x, y]);
                        bigLValid &= validTypes.Contains(bigLPattern[x, y]);
                        pipeValid &= validTypes.Contains(pipePattern[x, y]);
                    }
                }

                if (littleLValid && littleLHasEmpty)
                { Instantiate(tileset[0], parent.transform.position, Quaternion.Euler(0, rotation, 0), parent.transform); }
                if (bigLValid && bigLHasEmpty)
                { Instantiate(tileset[1], parent.transform.position, Quaternion.Euler(0, rotation, 0), parent.transform); }
                if (pipeValid && pipeHasEmpty)
                { Instantiate(tileset[2], parent.transform.position, Quaternion.Euler(0, rotation, 0), parent.transform); }

                littleLPattern = rotateArray(littleLPattern);
                bigLPattern = rotateArray(bigLPattern);
                pipePattern = rotateArray(pipePattern);
            }
        }

        // Rotate a 2d array of type T by 90 degrees.
        public T[,] rotateArray<T>(T[,] array)
        {
            if (array.GetLength(0) != array.GetLength(1))
            { throw new System.ArgumentException(); }

            int dimension = array.GetLength(0);
            T[,] result = new T[dimension, dimension];
            for (int x = 0; x < dimension; x++)
            {
                for (int y = 0; y < dimension; y++)
                {
                    T transposed = array[y, x];
                    int invertedIndex = dimension - 1 - y;
                    result[x, invertedIndex] = transposed;
                }
            }

            return result;
        }

        public chunkTypes GenerateChunkType(Vector2Int position)
        {
            System.Random randomizer = new System.Random((int)(world.settings.seed * (position.x + .5f) * (position.y + .5f)));
            float value = (float) randomizer.NextDouble();

            chunkTypes type;
            if (value < world.settings.forestRange.x) { type = chunkTypes.nest; }
            else if (value > world.settings.forestRange.y) { type = chunkTypes.town; }
            else { type = chunkTypes.forest; }

            return type;
        }
    }

    public World_Settings settings;
    public GameObject player;
    private System.Random randomizer;

    public enum chunkTypes { town, forest, nest };

    private Vector2 playerPos { get { return new Vector2(player.transform.position.x, player.transform.position.z); } }
    private List<Chunk> chunkList = new List<Chunk>();
    private List<Chunk> activeChunks = new List<Chunk>();

    public enum areaTypes { town, house, nest };
    private Dictionary<areaTypes, Area.Type> areaParameters = new Dictionary<areaTypes, Area.Type>();

    public void Start()
    {
        //Initialize randomizer with seed
        randomizer = new System.Random(settings.seed);

        // Package Area Parameters
        areaParameters.Add(areaTypes.town, 
            (new Area.Type(
            settings.townAreaResolution,
            settings.townMinDimension,
            settings.townPrimaryConnectionCount,
            settings.townSecondaryConnectionCount,
            settings.townSectorCount)));

        areaParameters.Add(areaTypes.house, 
            (new Area.Type(
            settings.houseAreaResolution,
            settings.houseMinDimension,
            settings.housePrimaryConnectionCount,
            settings.houseSecondaryConnectionCount,
            settings.houseSectorCount)));

        areaParameters.Add(areaTypes.nest, 
            (new Area.Type( 
            settings.nestAreaResolution,
            settings.nestMinDimension,
            settings.nestPrimaryConnectionCount,
            settings.nestSecondaryConnectionCount,
            settings.nestSectorCount)));

        // Initialize chunks
        UpdateActiveChunks(playerPos);

        //Test Chunk Constructor
        Chunk testChunk = new Chunk(this, Vector2Int.zero, transform);
    }

    public void Update()
    {
        //
        //UpdateActiveChunks(playerPos);
    }

    public void UpdateActiveChunks(Vector2 position)
    {
        List<Chunk> oldChunks = activeChunks;
        activeChunks = chunkList.Where(chunk => Vector2.Distance(chunk.position, position) < settings.activeChunkRadius).ToList();
        foreach (Chunk chunk in activeChunks)
        {
            oldChunks.Remove(chunk);
            chunk.self.SetActive(true);
        }

        foreach (Chunk chunk in oldChunks)
        { chunk.self.SetActive(false); }
    }

}
