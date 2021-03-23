using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Spell_Methods : MonoBehaviour
{
    /* This class includes a workaround to Unity's inability to use default interface methods.
     * Until Unity supports C#8, this class must hold all default methods, and be extended alongside Spell_Interface.cs
     * Aside from that functionality, this class also holds universal spell parameters. (as in all spells have these variables, but they may differ)
     * 
     * Types of spells
     * -Creates a visible meshed object that has logic to interact with
     * -Creates a trigger field that has parameters to apply to entities within
     * -Adds an instance of spell logic checking
     * -Incinerate (removes a spell)
     * -Modifies physics, limits & parenting (Water Spells)
     */

    // Visuals
    public Sprite @classSprite;
    public Sprite spellSprite;
    public Color classColour;
    public GameObject array;
    private GameObject canvas;

    // Parameters
    public int classIndex;
    public float interval;
    public float duration;
    public Transform objectParent { get; private set; }

    // Spell Logic
    public bool inputPower;
    private bool wasInputPower;
    public bool hasOutput;
    public bool output { get; private set; }
    public float outputPulseDuration;
    private Spell_Interface spellLogic;
    private bool inRangeOfPlayer;
    private List<Spell_Methods> links = new List<Spell_Methods>();
    


    public void Start()
    {
        // Set runes;
        Transform sprite = Instantiate(array, transform).transform;
        sprite.GetComponent<SpriteRenderer>().color = classColour;
        sprite.GetComponentInChildren<Light>().color = classColour;

        SpriteRenderer[] parts = sprite.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in parts)
        {
            renderer.color = classColour;
        }

        sprite.Find("Class").GetComponent<SpriteMask>().sprite = @classSprite;
        for (int i = 1; i <= 3; i++)
        { sprite.Find("Spell " + i).GetComponent<SpriteMask>().sprite = spellSprite; }

        sprite.Find("Output Node").gameObject.SetActive(hasOutput);

        canvas = sprite.Find("Canvas").gameObject;
        canvas.transform.localRotation = Quaternion.Euler(0, 0, transform.localRotation.eulerAngles.y + 180);

        // Assign variables
        try
        {
            objectParent = GameObject.Find("Objects").transform;
            spellLogic = GetComponent<Spell_Interface>();
        } catch (System.Exception e)
        {
            Debug.LogError("Spell Method initialzation failed.\n" + e);
        }
    }

    public void Update() 
    {
        InputPowerHandling();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        { inRangeOfPlayer = true; }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        { inRangeOfPlayer = false; }
    }

    private void InputPowerHandling() // Check power state
    {
        bool linkPower = links.Any(item => item.output);
        bool channeling = inRangeOfPlayer && Input.GetAxisRaw("Use") != 0;

        if ((links.Count == 0 && channeling) || linkPower)
        { inputPower = true; }
        else // If there is no powered inputs
        { inputPower = false; } 

        if (inputPower != wasInputPower && inputPower) //If inactive & powered, start the array
        { StartCoroutine(Activate()); }

        wasInputPower = inputPower;
    }

    private IEnumerator Activate() // Activate the array for a full cycle
    {
        spellLogic.Trigger();

        float timer = 0;
        while (inputPower)
        {
            timer += Time.deltaTime;
            if (timer >= interval)
            {
                spellLogic.Sustain();
                timer = 0;
            }
            yield return null;
        }

        spellLogic.End();
    }


    public IEnumerator PulseOutput()
    {
        output = true;
        yield return new WaitForSeconds(outputPulseDuration);
        output = false;
    }
}