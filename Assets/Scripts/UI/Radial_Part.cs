using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Radial_Part : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [Range(0, 3)]
    public int id;
    public bool mouseOver { get; private set; }
    public Color colourInactive;
    public Color colourActive;

    public void Start()
    {
        GetComponent<Image>().alphaHitTestMinimumThreshold = .5f;
        SetState(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    { SetState(true); }

    public void OnPointerExit(PointerEventData eventData)
    { SetState(false); }

    public void SetState(bool val)
    {
        mouseOver = val;
        if (mouseOver)
        { GetComponent<Image>().color = colourActive; }
        else
        { GetComponent<Image>().color = colourInactive; }
    }
}
