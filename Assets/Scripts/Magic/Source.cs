using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Source : MonoBehaviour
{
    public List<Spell_Methods> arrayList = new List<Spell_Methods>();

    private void OnTriggerEnter(Collider other)
    {
        Spell_Methods array = other.gameObject.GetComponent<Spell_Methods>();
        if (array != null)
        {
            arrayList.Add(array);
            array.sources++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Spell_Methods array = other.gameObject.GetComponent<Spell_Methods>();
        if (array != null)
        {
            arrayList.Remove(array);
            array.sources--;
        }
    }
}
