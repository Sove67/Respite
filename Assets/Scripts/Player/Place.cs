using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Place : MonoBehaviour
{
    public GameObject arrayParent;
    public GameObject player;
    private MeshRenderer cursor;
    public MeshRenderer originCursor;
    private Rotate rotate;

    private float height;
    public bool running { get; private set; }
    public bool triggered { get; private set; }
    public bool valid { get; private set; }
    public Spell_Methods highlighedArray { get; private set; }

    private List<Spell_Methods> arrayList = new List<Spell_Methods>();

    private Quaternion rotation;

    public float snapDistance;
    public class OrderByDistance : IComparer<Spell_Methods>
    {
        Vector3 origin;

        public OrderByDistance(Vector3 origin)
        { this.origin = origin; }

        public int Compare(Spell_Methods a, Spell_Methods b)
        {
            float first = Vector3.Distance(a.snapPoint, origin);
            float second = Vector3.Distance(b.snapPoint, origin);
            float difference = first - second;
            return (int)difference;
        }
    }

    public void Start()
    {
        height = transform.localPosition.y;
        cursor = GetComponent<MeshRenderer>();
        rotate = GetComponent<Rotate>();
    }

    public IEnumerator Routine(GameObject @object, bool snapToArray)
    {
        arrayList = arrayParent.GetComponentsInChildren<Spell_Methods>().ToList();
        highlighedArray = null;
        running = true;
        cursor.enabled = true;
        valid = false;
        triggered = false;

        while (!(triggered && valid) && running)
        {
            FetchPosition(snapToArray);
            yield return null;
        }
        if (running)
        {
            yield return new WaitUntil(() => !Input.GetMouseButton(0));
            if (snapToArray)
            {
                Vector3 originPos = cursor.transform.position;
                Spell_Methods origin = highlighedArray;
                arrayList.Remove(origin);
                originCursor.transform.position = originPos;
                originCursor.enabled = true;
                cursor.enabled = false;

                valid = false;
                triggered = false;
                while (!(triggered && valid) && running)
                {
                    FetchPosition(snapToArray);
                    originCursor.transform.rotation = rotate.GetAngle(originCursor.transform.position);
                    yield return null;
                }

                Spell_Methods end = highlighedArray;
                Vector3 endPos = cursor.transform.position;

                if (running)
                {
                    Vector3 midpoint = Vector3.Lerp(originPos, endPos, 0.5f);
                    Vector3 offset = endPos - originPos;
                    Spell_Methods array = Instantiate(@object, midpoint, Quaternion.FromToRotation(Vector3.back, offset), arrayParent.transform).GetComponent<Spell_Methods>();
                    Spell_Interface @interface = array.GetComponent<Spell_Interface>();
                    origin.outputTargets.Add(@interface);
                    array.end = end;
                }
            } else
            {
                StartCoroutine(rotate.Routine());
                yield return new WaitUntil(() => Input.GetMouseButton(0));
                rotate.Trigger();
                yield return new WaitUntil(() => !rotate.running);
                rotation = Quaternion.Euler(0, rotate.rotation ?? default, 0);
                Instantiate(@object, transform.position, rotation, arrayParent.transform);
            }

            running = false;
        }

        cursor.enabled = false;
        originCursor.enabled = false;
    }

    public void FetchPosition(bool snapToArray)
    {
        highlighedArray = null;

        // Get all arrays
        int index = 0;
        if (snapToArray)
        {
            if (arrayList.Any(item => item.GetComponent<Renderer>().isVisible))
            {
                (valid, transform.position, index) = getSnappedPosition(arrayList, snapDistance);
                highlighedArray = index != -1 ? arrayList[index] : null;
            }
            else // Break if there are no points to snap to.
            { running = false; }
        }
        else
        { (valid, transform.position) = getBlockedPosition(arrayList, snapDistance); }

        cursor.enabled = valid;
    }

    // Block cursor around points
    private (bool, Vector3) getBlockedPosition(List<Spell_Methods> points, float distance)
    {
        Vector3 mouse = CustomMouseRaycast();
        if (points.Count == 0 || !points.Any(item => Vector3.Distance(mouse, item.snapPoint) <= distance))
        { return (true, mouse); }
        else
        { return (false, transform.position); }
    }

    // Snap cursor towards points
    private (bool, Vector3, int) getSnappedPosition(List<Spell_Methods> points, float distance)
    {
        Vector3 mouse = CustomMouseRaycast();
        List<Spell_Methods> snappableArrays = points.Where(item => Vector3.Distance(mouse, item.snapPoint) <= distance).ToList();
        snappableArrays.Sort(new OrderByDistance(mouse));

        if (snappableArrays.Count > 0)
        { return (true, snappableArrays[0].snapPoint, points.FindIndex(item => item.Equals(snappableArrays[0]))); }
        else 
        { return (false, transform.position, -1); }
    }

    public Vector3 CustomMouseRaycast()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float camHeight = ray.origin.y;
        float buildHeight = transform.position.y;
        float heightDist = camHeight - buildHeight;
        float directionMagnitudeAdjustment = heightDist / ray.direction.y;
        Vector3 offset = ray.direction * directionMagnitudeAdjustment;

        Vector3 rayToBuildLayer = new Vector3(-offset.x + Camera.main.transform.localPosition.x, height, -offset.z + Camera.main.transform.localPosition.z);
        Vector3 globalRay = rayToBuildLayer + player.transform.position;
        return globalRay;
    }

    public void Trigger()
    { triggered = true; }

    public void Close()
    { running = false; }
}
