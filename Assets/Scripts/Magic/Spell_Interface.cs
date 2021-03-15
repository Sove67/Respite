using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Spell_Interface
{
    /* Definable Methods
     * void Trigger(); // The effect created when the array is powered
     * void Sustain(); // The effect created while the array is powered
     * void End(); // The effect created when the array loses power
     * void Destroy(); // Logic to clean up the spell effects when the array is destroyed
     */

    // Definable Methods
    void Trigger(); // The effect created when the array is powered
    void Sustain(); // The effect created while the array is powered
    void End(); // The effect created when the array loses power
    void Destroy(); // Logic to clean up the spell effects when the array is destroyed
}