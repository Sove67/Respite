using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell_Core : Spell_Methods, Spell_Interface
{
    public GameObject corePrefab;
    private GameObject instantiation;
    public float healValue;
    public void Destroy() // Destroy the array
    {
        StopAllCoroutines();
        Destroy(instantiation);
        Destroy(gameObject);
    }

    public void End()
    {
        // Unused
    }

    public void Trigger() // Start the activation & summon a wave
    {
        if (instantiation == null)
        {
            // snap to nearest index
            Vector3 constrainedPosition = new Vector3(transform.position.x, corePrefab.transform.localScale.y / 2, transform.position.z); //Set height to gameobject height
            Vector3 offset = transform.rotation * Vector3.forward * (corePrefab.transform.localScale.z / 2);

            instantiation = Instantiate(corePrefab, constrainedPosition + offset, transform.rotation, objectParent);


            instantiation.GetComponent<Entity>().onDestruction = () => { Destroy(); };
        }
        else
        {
            StartCoroutine(Wave());
        }
    }

    private IEnumerator Wave()
    {
        // Add global lure to boids
        // Call the spawning script's wave spawn function at difficulty 1
        // Await wave destruction
        // Call the spawning script's wave spawn function at difficulty 2
        // Await wave destruction
        // Call the spawning script's wave spawn function at difficulty 3
        // Await wave destruction
        // Establish Core
        yield return null;
    }
}
