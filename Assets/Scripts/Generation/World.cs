using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class World : MonoBehaviour
{
    public World_Settings settings;
    public GameObject chunkInstance;

    //private readonly List<Chunk> chunkList = new List<Chunk>();
    //private List<Chunk> activeChunks = new List<Chunk>();

    public void Start()
    {
        // Initialize chunks
        //UpdateActiveChunks(playerPos);

        //Test Chunk Constructor
        Instantiate(chunkInstance, transform);
    }

    /*
    public void UpdateActiveChunks(Vector2 position)
    {
        List<Chunk> oldChunks = activeChunks;
        activeChunks = chunkList.Where(chunk => Vector2.Distance(chunk.position, position) < settings.activeChunkRadius).ToList();
        foreach (Chunk chunk in activeChunks)
        {
            oldChunks.Remove(chunk);
            chunk.gameObject.SetActive(true);
        }

        foreach (Chunk chunk in oldChunks)
        { chunk.gameObject.SetActive(false); }
    }
    */
}
