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

    public Spell_Settings settings;

    // Visuals
    public Sprite spellSprite;
    private Transform sprite;
    private Player_Control playerControl;
    private SpriteRenderer dot;
    private SpriteRenderer colouring;
    private GameObject classRune;
    private float hue;

    // Parameters
    [Range(0, 4)]
    public int classIndex;

    public float interval; // Time between sustain calls
    public float duration; // Time taken to apply effect

    public Transform objectParent { get; private set; }

    // Spell Logic
    public int inputs;

    public bool hasOutput;
    public List<Spell_Interface> outputTargets { get; set; } = new List<Spell_Interface>();

    public Spell_Interface spellLogic;
    public int sources {
        get { return privateSources; }
        
        set
        {
            // If the boolean hasSource has changed, call the visualizer function.
            bool hasSource = value > 0;
            if (hasSource != privateSources > 0)
            { SourceVisuals(hasSource); }

            privateSources = value;
        }
    }
    private int privateSources;
    public bool overrideSourcesForTesting;
    public Vector3 snapPoint { get { return transform.position; } }
    public IEnumerator activate;

    // Logic Spells Support
    public Spell_Methods end { get; set; }

    public void Start()
    {
        // Assign visuals
        sprite = Instantiate(settings.arrayPrefab, transform).transform;

        // Assign Objects
        objectParent = GameObject.Find("Objects").transform;
        playerControl = GameObject.Find("Player").GetComponent<Player_Control>();
        dot = sprite.Find("Dot").GetComponent<SpriteRenderer>();
        colouring = sprite.Find("Rune Colouring").GetComponent<SpriteRenderer>();
        classRune = sprite.Find("Class").gameObject;
        spellLogic = GetComponent<Spell_Interface>();
        activate = Activate();

        // Set colours;
        Color.RGBToHSV(settings.classColours[classIndex], out hue, out _, out _); // Parse out hue
        sprite.GetComponentInChildren<Light>().color = settings.classColours[classIndex]; // Colour light
        SourceVisuals(false); // Colour runes

        // Set Runes
        classRune.GetComponent<SpriteMask>().sprite = settings.classRunes[classIndex];
        sprite.Find("Spell").GetComponent<SpriteMask>().sprite = spellSprite;
    }

    public void Update() 
    {
        Mathf.Clamp(sources, 0, int.MaxValue);

        if ((sources > 0 || overrideSourcesForTesting) && inputs > 0 && !activate.MoveNext())
        {
            activate = Activate();
            StartCoroutine(activate); 
        }

        dot.color = new Color(dot.color.r, dot.color.g, dot.color.b, playerControl.zoomLevel);
    }

    private IEnumerator Activate() // Activate the array for a full cycle
    {
        float timer = interval;
        while (inputs > 0)
        {
            timer += Time.deltaTime;
            if (timer >= interval)
            {
                spellLogic.Trigger();
                timer = 0;
            }
            yield return null;
        }

        spellLogic.End();
    }

    public void Output()
    { 
        outputTargets.ForEach(item => item.Trigger()); 
    }

    private void SourceVisuals(bool hasSource)
    {
        // Find target values
        float value = hasSource ? settings.activeVisualValue : settings.inactiveVisualValue;

        // Set colour to new values
        Color newColour = Color.HSVToRGB(hue, value, value);
        dot.color = newColour;
        colouring.color = newColour;
    }
}