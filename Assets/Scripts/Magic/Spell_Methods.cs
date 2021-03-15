using System.Collections;
using System.Collections.Generic;
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
    public Sprite magicClass;
    public Sprite magicSpell;
    public Sprite magicArray;

    // Parameters
    public int classIndex;
    public float duration;
    public bool consumable;

    // Spell Logic
    public bool inputPower;
    private bool wasInputPower;
    public bool outputPower { get; private set; }
    private Spell_Interface spell;

    /* 
     * Default Methods:
     * -Check for power
     */
    public void Start()
    {
        spell = GetComponent<Spell_Interface>();

        if (spell == null)
        { Debug.LogError("No Spell Attached!"); }
    }

    public void Update()
    {
        InputPowerHandling();
    }

    public void InputPowerHandling()
    {
        if (inputPower != wasInputPower)
        {
            if (inputPower)
            { spell.Trigger(); }
            else
            { spell.End(); }
        } 
        
        else if (inputPower)
        { spell.Sustain(); }
        wasInputPower = inputPower;
    }
}