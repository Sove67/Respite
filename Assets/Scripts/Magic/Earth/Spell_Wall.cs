using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell_Wall : Spell_Methods, Spell_Interface
{
    public GameObject wallPrefab;
    private GameObject instantiation;

    public void Destroy() // Destroy the array
    {
        Destroy(gameObject);
    }

    public void End() // Make wall phasable
    {
        instantiation.GetComponent<Collider>().enabled = false;
    }

    public void Sustain() // Make wall solid
    {
        instantiation.GetComponent<Collider>().enabled = true;
    }

    public void Trigger() // Create a wall if none has been made
    {
        if (instantiation == null)
        {
            // snap to nearest index
            Vector3 constrainedPosition = new Vector3(transform.position.x, wallPrefab.transform.localScale.y / 2, transform.position.z); //Set height to gameobject height
            Vector3 offset = transform.rotation * Vector3.forward * (wallPrefab.transform.localScale.z / 2);

            instantiation = Instantiate(wallPrefab, constrainedPosition + offset, transform.rotation, objectParent);


            instantiation.GetComponent<Entity>().onDestruction = () => { StartCoroutine(PulseOutput()); instantiation = null; };
        }
    }
}
