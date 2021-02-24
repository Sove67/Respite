using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    // Variables
    public Enemy_Settings settings;
    public Transform player;
    public GameObject boid;

    private float timer;
    private readonly List<Transform> children = new List<Transform>();

    // Functions
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= settings.spawnTimer)
        {
            timer = 0;

            Debug.Log("Timer Elapsed.");

            int i = 0;
            children.Clear();
            foreach (Transform child in transform)
            {
                // Clearing
                if (Vector3.Distance(child.position, player.position) > settings.radiusMax)
                {
                    child.GetComponent<Enemy_AI>().Die(); i++;
                }

                else
                { children.Add(child); }
            }
            Debug.Log("Destroyed " + i + " Children.");
            Debug.Log("Found " + children.Count + " Children.");

            // Spawning
            if (children.Count < settings.popMin)
            {
                Debug.Log("Spawning.");
                // if pop below min, run the spawn script until max is reached.
                SpawnGroup(children.Count);
            }
        }
    }

    public void SpawnGroup(int count)
    {
        //while pop count is less than max
        while (count < settings.popMax)
        {
            // Generate a randomized radius between the max & min
            float radius = Random.Range(settings.radiusMin, settings.radiusMax + 1);
            // Generate a random angle from 0 to 360
            float angle = Random.Range(0, 360);

            // Apply to character coords
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            Vector3 position = new Vector3(player.position.x + x, 1, player.position.z + z);

            // TODO Check if coordinate is blocked

            // Spawn all boids in “pack” at same coordinate (Collision displacement will spread them out)
            int packSize = (int)Random.Range(1, settings.packCount + 1);
            for (int i = 0; i < packSize; i++)
            {
                Vector3 offset = new Vector3(Random.value, 0, Random.value);
                Instantiate(boid, position + offset, new Quaternion(), transform);
                count++;
            }
        }
    }
}
