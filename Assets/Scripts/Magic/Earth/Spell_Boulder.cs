using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell_Boulder : Spell_Methods, Spell_Interface
{
    public void Destroy()
    {
        Debug.Log("Destroying!");
        Destroy(this);
        throw new System.NotImplementedException();
    }

    public void End()
    {
        // Unused for the boulder spell.
        Debug.LogError("Invalid Method!");
    }

    public void Sustain()
    {
        // Unused for the boulder spell.
        Debug.LogError("Invalid Method!");
    }

    public void Trigger()
    {
        // Create a boulder, and then call the destroy command
        Destroy();
        throw new System.NotImplementedException();
    }
}
