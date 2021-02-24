using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Radial_Control : MonoBehaviour
{
    public int? active { get; private set; }
    public bool running { get; private set; }
    public bool triggered { get; private set; }
    public List<GameObject> partList = new List<GameObject>();
    public GameObject part;
    
    public Material materialInactive;
    public Material materialActive;

    public IEnumerator AwaitInput(int partCount)
    {
        if (partCount < 3)
        { Debug.LogError("Radial Menu can't handle less than 3 options."); }

        for (int i = 0; i < partCount; i++)
        {
            GameObject newPart = Instantiate(part, transform);
            newPart.layer = 5;
            newPart.GetComponent<Radial_Part>().CreateMesh(10, 50, partCount, 5, i, materialInactive);
            partList.Add(newPart);
        }

        running = true;
        triggered = false;
        gameObject.SetActive(true);

        // wait for a trigger
        while (!triggered)
        {
            if (!running)
            { break; }
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (active != null)
            {
                partList[active ?? default].GetComponent<MeshRenderer>().material = materialInactive;
                active = null;
            }
            if (Physics.Raycast(ray, out RaycastHit hitInfo, 5))
            {
                int index = partList.IndexOf(hitInfo.collider.gameObject);
                if (index != -1)
                {
                    active = index;
                    partList[index].GetComponent<MeshRenderer>().material = materialActive;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
        if (running)
        {
            partList[active ?? default].GetComponent<MeshRenderer>().material = materialInactive;
            yield return new WaitForSeconds(0.1f);
        }

        foreach (GameObject part in partList)
        { Destroy(part); }
        partList.Clear();
        running = false;
        gameObject.SetActive(false);
        yield break;
    }

    public void Trigger()
    { triggered = true; }

    public void Close()
    { running = false; }
}
