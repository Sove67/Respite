using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public bool running { get; private set; }
    public bool triggered { get; private set; }
    public float? rotation { get; private set; }

    public IEnumerator Routine(GameObject cursor, Vector3 initialCursor)
    {
        running = true;
        float angle = 0;
        rotation = null;
        while (!(triggered || !running))
        {
                Vector3 difference = Input.mousePosition - initialCursor;
                angle = (Mathf.Atan2(difference.x, difference.y) + (2 * Mathf.PI)) % (2 * Mathf.PI) * Mathf.Rad2Deg;
                
                cursor.transform.rotation = Quaternion.Euler(0, angle, 0);

            yield return null;
        }
        if (running)
        {
            rotation = angle;
            running = false;
        }

        triggered = false;
        yield break;
    }

    public void Trigger()
    { triggered = true; }

    public void Close()
    { running = false; }
}
