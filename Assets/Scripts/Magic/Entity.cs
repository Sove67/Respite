using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public Spell_Interface parentArray;
    public System.Action onDestruction;
    public float health;
    
    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            onDestruction();
            Destroy(gameObject);
        }
    }
}
