using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Build : MonoBehaviour
{
    [Header("Components")]
    public GameObject cursorPoint;
    public Material validCursor;
    public Material invalidCursor;
    public GameObject objectParent;
    public Camera playerCamera;

    [Header("UI")]
    public Radial_Control radialMenu;

    [Header("Buildings")]
    public List<GameObject> buildingListLeft;
    public List<GameObject> buildingListRight;
    public List<GameObject> buildingListUp;
    public List<GameObject> buildingListDown;
    private List<List<GameObject>> buildingList = new List<List<GameObject>>();

    private bool menuActive = false;

    public void Start()
    {
        buildingList.Add(buildingListLeft);
        buildingList.Add(buildingListRight);
        buildingList.Add(buildingListUp);
        buildingList.Add(buildingListDown);
    }

    public void SetMenuActive()
    {
        SetMenuActive(!menuActive);
    }

    public void SetMenuActive(bool val)
    {
        menuActive = val;
        if (menuActive && !radialMenu.running)
        {
            StartCoroutine(ChooseBuilding());
        }
        else if (!menuActive && radialMenu.running)
        {
            Debug.Log("Closing Radial.");
            radialMenu.Close();
            StopCoroutine(ChooseBuilding());
            //Need to stop the placing coroutine here too...
        }
    }

    public void TryTrigger()
    {
        if (radialMenu.active != null)
        {
            radialMenu.Trigger();
        }
    }

    public IEnumerator Place(GameObject @object)
    {
        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;
        bool valid = false;
        while (!valid || !Input.GetMouseButton(0))
        {
            float rayOffset = 1.05f;
            Vector3 mouseOffset = new Vector3((Screen.width - Screen.width * rayOffset) / 2, (Screen.height - Screen.height * rayOffset) / 2);
            Vector3 mouse = (Input.mousePosition * rayOffset) + mouseOffset;
            Ray ray = playerCamera.ScreenPointToRay(mouse);

            if (Physics.SphereCast(ray, 1, out RaycastHit hitInfo, Mathf.Infinity))
            {
                if (hitInfo.collider.gameObject.layer == 8)
                {
                    valid = true;
                    position = hitInfo.point;
                    rotation = hitInfo.collider.transform.rotation;
                    cursorPoint.SetActive(true);
                    cursorPoint.transform.position = hitInfo.point;
                }
                else
                {
                    valid = false;
                    cursorPoint.SetActive(false);
                }
            }
            yield return new WaitForSeconds(0.01f);
        }
        Instantiate(@object, position, rotation, objectParent.transform);
        Debug.Log("Placed " + @object);
        cursorPoint.SetActive(false);
        SetMenuActive(false);
        yield break;
    }

    private IEnumerator ChooseBuilding()
    {
        //Choose a category from the radial menu
        StartCoroutine(radialMenu.AwaitInput(buildingList.Count));
        yield return new WaitUntil(() => radialMenu.active != null && Input.GetMouseButton(0));
        radialMenu.Trigger();
        yield return new WaitUntil(() => !radialMenu.running);
        int category = radialMenu.active.id;

        //choose an object from the radial menu
        yield return new WaitUntil(() => !Input.GetMouseButton(0));
        StartCoroutine(radialMenu.AwaitInput(buildingList[category].Count));
        yield return new WaitUntil(() => Input.GetMouseButton(0));
        radialMenu.Trigger();
        yield return new WaitUntil(() => !radialMenu.running);
        int @object = radialMenu.active.id;

        //place the object
        yield return new WaitUntil(() => !Input.GetMouseButton(0));
        StartCoroutine(Place(buildingList[category][@object]));
        yield break;
    }
}
