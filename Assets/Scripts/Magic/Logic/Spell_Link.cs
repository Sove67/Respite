using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell_Link : Spell_Methods, Spell_Interface
{
    public void Destroy() // Destroy the array
    {
        StopAllCoroutines();
        Destroy(gameObject);
    }

    public void End()
    {
        // Unused.
    }

    public void Trigger()
    {
        end.spellLogic.Trigger();
    }
}
