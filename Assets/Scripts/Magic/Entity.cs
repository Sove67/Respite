using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public Spell_Interface parentArray;
    public System.Action onDestruction;
    public float maxHealth;
    private float currentHealth;
    private const float I_FRAME_DURATION = .5f;
    private float IFrames;
    public bool killSwitch;

    public void Start()
    {
        currentHealth = maxHealth;
    }

    public void Update()
    {
        if (killSwitch) 
        { ModifyHealth(-maxHealth); }

        if (IFrames > 0)
        { IFrames -= Time.deltaTime; }
    }
    public void ModifyHealth(float modifier)
    {
        if (IFrames <= 0)
        {
            currentHealth = Mathf.Clamp(currentHealth + modifier, 0, maxHealth);
            Debug.Log(name + " health changed by " + modifier + " to " + currentHealth);

            if (currentHealth == 0)
            {
                onDestruction();
                Destroy(gameObject);
            }

            IFrames = I_FRAME_DURATION;
        }
    }
}
