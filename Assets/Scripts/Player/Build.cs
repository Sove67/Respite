using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Build : MonoBehaviour
{
    [Header("UI")]
    public Radial_Control radialMenu;
    public Place place;

    [Header("Buildings")]
    public List<GameObject> buildingListLeft;
    public List<GameObject> buildingListRight;
    public List<GameObject> buildingListUp;
    public List<GameObject> buildingListDown;
    private List<List<GameObject>> buildingList = new List<List<GameObject>>();

    private bool running;
    public void Start()
    {
        buildingList.Add(buildingListLeft);
        buildingList.Add(buildingListRight);
        buildingList.Add(buildingListUp);
        buildingList.Add(buildingListDown);
    }

    public void ToggleState()
    {
        if (running)
        { Close(); }
        else
        { Begin(); }
    }

    public void Begin()
    {
        running = true;
        StartCoroutine(Routine());
    }

    public void Close()
    {
        running = false;
        radialMenu.Close();
        place.Close();
        StopCoroutine(Routine());
    }

    public void Trigger()
    { 
        if (radialMenu.running)
        { radialMenu.Trigger(); }
        else if (place.running)
        { 
            place.Trigger();
            running = false;
        }
    }


    private IEnumerator Routine()
    {
        radialMenu.SetCenterToMouse();
        //Choose a category from the radial menu
        StartCoroutine(radialMenu.AwaitInput(buildingList.Count));
        yield return new WaitUntil(() => radialMenu.activePart != null && Input.GetMouseButton(0));
        radialMenu.Trigger();
        int category = radialMenu.activePart.id;
        yield return new WaitUntil(() => !radialMenu.running && !radialMenu.triggered);

        if (running) // If the build menu has not been closed prematurely
        {
            //choose an object from the radial menu
            StartCoroutine(radialMenu.AwaitInput(buildingList[category].Count));
            yield return new WaitUntil(() => radialMenu.activePart != null && Input.GetMouseButton(0));
            radialMenu.Trigger();
            int @object = radialMenu.activePart.id;
            yield return new WaitUntil(() => !radialMenu.running && !radialMenu.triggered);

            if (running) // If the build menu has not been closed prematurely
            {
                //place the object
                StartCoroutine(place.Routine(buildingList[category][@object]));
                yield break;
            }
        }

    }
}
