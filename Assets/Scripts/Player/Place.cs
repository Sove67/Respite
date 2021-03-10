using System.Collections;
using UnityEngine;

public class Place : MonoBehaviour
{
    [Header("Components")]
    public GameObject cursorPoint;
    public Material validCursor;
    public Material invalidCursor;
    public GameObject objectParent;
    public Camera playerCamera;
    public Rotate rotate;
    [Header("Settings")]
    public float buildHeight;

    public bool running { get; private set; }
    public bool triggered { get; private set; }

    private Vector3 position;
    private Quaternion rotation;
    public IEnumerator Routine(GameObject @object)
    {
        running = true;
        position = Vector3.zero;
        cursorPoint.GetComponent<MeshFilter>().sharedMesh = @object.GetComponent<MeshFilter>().sharedMesh;
        bool valid = false;
        while (!(triggered && valid) && running)
        {
            RaycastHit? hitInfo = GetComponent<Player_Control>().getMouseSphereCast();
            if (hitInfo != null)
            {
                RaycastHit parsedHitInfo = hitInfo ?? default;
                if (parsedHitInfo.collider.gameObject.layer == 8)
                {
                    valid = true;
                    position = parsedHitInfo.point;
                    cursorPoint.SetActive(true);
                    cursorPoint.transform.position = parsedHitInfo.point;
                }
                else
                {
                    valid = false;
                    cursorPoint.SetActive(false);
                }
            }
            yield return new WaitForSeconds(0.01f);
        }
        if (running)
        {
            position.y = buildHeight;
            yield return new WaitUntil(() => !Input.GetMouseButton(0));
            StartCoroutine(rotate.Routine(cursorPoint, position));
            yield return new WaitUntil(() => Input.GetMouseButton(0));
            rotate.Trigger();
            yield return new WaitUntil(() => !rotate.running);
            rotation = Quaternion.Euler(0, rotate.rotation ?? default, 0);

            Instantiate(@object, position, rotation, objectParent.transform);
            running = false;
        }

        cursorPoint.SetActive(false);
        triggered = false;
        yield break;
    }

    public void Trigger()
    { triggered = true; }

    public void Close()
    { running = false; }
}
