using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Radial_Control : MonoBehaviour
{
    public Radial_Part activePart { get; private set; }
    public bool running { get; private set; }
    public bool triggered { get; private set; }
    public List<GameObject> partList = new List<GameObject>();
    public GameObject part;
    
    public Material materialInactive;
    public Material materialActive;

    private static int MIN_PART_COUNT = 3;
    private static int PARTIAL_MENU_COUNT = 2;
    public void Start()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 sizeDelta = new Vector2(rectTransform.rect.width / (Screen.height / 2), rectTransform.rect.height / (Screen.height / 2));
        rectTransform.sizeDelta = sizeDelta;
    }

    public IEnumerator AwaitInput(int partCount)
    {
        activePart = null;
        running = true;
        triggered = false;
        if (partCount == PARTIAL_MENU_COUNT)
        {
            GameObject newPart = Instantiate(part, transform);
            newPart.name = "Radial Part 1";
            newPart.GetComponent<Radial_Part>().Create(0, 10, 50, Screen.height / 200, MIN_PART_COUNT, 5, 0, materialInactive, null);
            partList.Add(newPart);

            newPart = Instantiate(part, transform);
            newPart.name = "Radial Part 1";
            newPart.GetComponent<Radial_Part>().Create(1, 10, 50, Screen.height / 200, MIN_PART_COUNT, 5, 2, materialInactive, null);
            partList.Add(newPart);
        } else
        {
            for (int i = 0; i < partCount; i++)
            {
                GameObject newPart = Instantiate(part, transform);
                newPart.name = "Radial Part " + i;
                newPart.GetComponent<Radial_Part>().Create(i, 10, 50, Screen.height / 200, partCount, 5, i, materialInactive, null);
                partList.Add(newPart);
            }
        }

        gameObject.SetActive(true);

        // wait for a trigger
        while (!(triggered && activePart != null) && running)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (activePart != null)
            {
                activePart.GetComponent<MeshRenderer>().material = materialInactive;
                activePart = null;
            }
            if (Physics.Raycast(ray, out RaycastHit hitInfo)) //Can't filter to layer 5, because that breaks when near the edges of the screen & camera offset is working... (No clue why)
            {
                int index = partList.IndexOf(hitInfo.collider.gameObject);
                if (index != -1)
                {
                    activePart = partList[index].GetComponent<Radial_Part>();
                    activePart.GetComponent<MeshRenderer>().material = materialActive;
                }
            }
            yield return null;
        }
        if (running)
        {
            activePart.GetComponent<MeshRenderer>().material = materialInactive;
            yield return null;
            running = false;
        }

        gameObject.SetActive(false);
        foreach (GameObject part in partList)
        { Destroy(part); }
        partList.Clear();
        triggered = false;
        yield break;
    }

    public void Trigger()
    { triggered = true; }

    public void Close()
    { running = false; }

    public void SetCenterToMouse()
    { GetComponent<RectTransform>().anchoredPosition = new Vector2(Input.mousePosition.x - Screen.width / 2, Input.mousePosition.y - Screen.height / 2); }
}
