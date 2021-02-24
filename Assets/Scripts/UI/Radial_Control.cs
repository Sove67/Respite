using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Radial_Control : MonoBehaviour
{
    public Radial_Part active { get; private set; }
    public bool running { get; private set; }
    public bool triggered { get; private set; }
    public Radial_Part[] partList;
    private int partCount;

    public IEnumerator AwaitInput(int partCount)
    {
        triggered = false;
        running = true;
        gameObject.SetActive(true);
        for (int i = 0; i < partList.Length; i++)
        {
            partList[i].SetState(false);
            partList[i].gameObject.SetActive(i < partCount);
        }

        while (!triggered)
        {
            active = null;
            int index = 0;
            while (active == null && index < partCount)
            {
                Radial_Part part = partList[index];
                if (part.mouseOver)
                {
                    active = part;
                }
                index++;
            }
            yield return new WaitForSeconds(0.1f);
        }
        active.GetComponent<Image>().color = active.colourInactive;
        yield return new WaitForSeconds(0.1f);

        running = false;
        gameObject.SetActive(false);
        yield break;
    }

    public void Trigger()
    { triggered = true; }

    public void Close()
    { StopCoroutine(AwaitInput(partCount)); }
}
