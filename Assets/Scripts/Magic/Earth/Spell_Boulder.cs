using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell_Boulder : Spell_Methods, Spell_Interface
{
    public int max;
    public GameObject boulderPrefab;
    public float offset = 2;
    private List<GameObject> instantiatedBoulders = new List<GameObject>();

    public void Destroy() // Destroy the array
    {
        StopAllCoroutines();
        for (int i = 0; i < instantiatedBoulders.Count; i++)
        {
            Destroy(instantiatedBoulders[0]);
            instantiatedBoulders.RemoveAt(0);
        }

        Destroy(gameObject);
    }

    public void End() 
    {
        // Unused.
    }

    public void Trigger() 
    {
        if (instantiatedBoulders.Count < max)
        {
            Vector3 position = transform.position + (transform.rotation * Vector3.forward * (boulderPrefab.transform.localScale.z / 2 + offset));

            Collider[] hitColliders = Physics.OverlapBox(position, boulderPrefab.transform.localScale / 2, transform.rotation);

            if (hitColliders.Length == 0 || (hitColliders.Length == 1 && hitColliders[0].CompareTag("Ignore Collisions")))
            {
                GameObject obj = Instantiate(boulderPrefab, position, transform.rotation, objectParent);
                obj.GetComponent<Entity>().onDestruction = () => Output();
                instantiatedBoulders.Add(obj);
            }
        }
    }
}
