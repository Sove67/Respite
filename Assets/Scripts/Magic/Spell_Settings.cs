using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Spell_Settings : ScriptableObject
{
    [Header("Runes")]
    public List<Sprite> classRunes;
    public List<Color> classColours;
    [Range(0, 1)]
    public float inactiveVisualValue;
    [Range(0, 1)]
    public float activeVisualValue;

    [Header("Prefabs")]
    public GameObject arrayPrefab;
    public List<GameObject> spellList;
}

