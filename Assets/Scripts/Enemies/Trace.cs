using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trace : MonoBehaviour
{
    public Enemy_Settings settings;

    void Start()
    {
        StartCoroutine(Delay());
    }

    IEnumerator Delay()
    {
        yield return new WaitForSeconds(settings.traceTime);

        GetComponentInParent<Enemy_AI>().PurgeTarget(gameObject);
        Destroy(gameObject);
    }
}
