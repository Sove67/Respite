using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public bool running { get; private set; }
    public bool triggered { get; private set; }
    public float? rotation { get; private set; }
    public float rotationOffset;
    private Place place;

    public void Start()
    {
        place = GetComponent<Place>();
    }
    public IEnumerator Routine()
    {
        running = true;
        rotation = null;
        while (!(triggered || !running))
        {
            transform.rotation = GetAngle(transform.position);
            yield return null;
        }
        if (running)
        {
            rotation = transform.rotation.eulerAngles.y;
            running = false;
        }

        triggered = false;
        yield break;
    }

    public Quaternion GetAngle(Vector3 center)
    {
        Vector3 difference = center - place.CustomMouseRaycast();
        float angle = ((Mathf.Atan2(difference.x, difference.z) + (2 * Mathf.PI)) % (2 * Mathf.PI) * Mathf.Rad2Deg) + rotationOffset;
        return Quaternion.Euler(0, angle, 0);
    }

    public void Trigger()
    { triggered = true; }

    public void Close()
    { running = false; }
}
