using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Build : MonoBehaviour
{
    [Header("UI")]
    public Radial_Control radialMenu;
    private Place place;

    [Header("Buildings")]
    public Spell_Settings settings;
    public int logicIndex;

    private bool running;

    public void Start()
    {
        place = GetComponent<Place>();
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
        else if (place.running && place.valid)
        { 
            place.Trigger();
            running = false;
        }
    }

    private IEnumerator Routine()
    {
        radialMenu.SetCenterToMouse();
        //Choose a category from the radial menu
        StartCoroutine(radialMenu.AwaitInput(settings.classRunes, settings.classColours));
        yield return new WaitUntil(() => radialMenu.activePart != null && Input.GetMouseButton(0));
        radialMenu.Trigger();
        int classIndex = radialMenu.activePart.id;
        yield return new WaitUntil(() => !radialMenu.running && !radialMenu.triggered && !Input.GetMouseButton(0));

        List<GameObject> categorySpells = new List<GameObject>();
        categorySpells = settings.spellList.Where(item => item.GetComponent<Spell_Methods>().classIndex == classIndex).ToList();

        List<Sprite> categoryRunes = new List<Sprite>();
        categorySpells.ForEach(item => categoryRunes.Add(item.GetComponent<Spell_Methods>().spellSprite));
        if (categorySpells.Count == 0)
        {
            Debug.LogWarning("Chosen category has no spells.");
            yield break;
        } 
        else if (categorySpells.Count == 1 && running)
        {
            Debug.LogWarning("Chosen category has one spell. No secondary radial needs to be created.");
            StartCoroutine(place.Routine(categorySpells[0], classIndex == logicIndex));
            yield break;
        }
        else if (running) // If the build menu has not been closed prematurely
        {
            //choose an object from the radial menu
            StartCoroutine(radialMenu.AwaitInput(categoryRunes, new List<Color>() { settings.classColours[classIndex] }));
            yield return new WaitUntil(() => radialMenu.activePart != null && Input.GetMouseButton(0));
            radialMenu.Trigger();
            int spellIndex = radialMenu.activePart.id;
            yield return new WaitUntil(() => !radialMenu.running && !radialMenu.triggered && !Input.GetMouseButton(0));
            

            if (running) // If the build menu has not been closed prematurely
            {
                // place the object
                
                StartCoroutine(place.Routine(categorySpells[spellIndex], classIndex == logicIndex));
                yield break;
            }
        }
    }
}
