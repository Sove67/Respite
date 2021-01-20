using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu]
public class Boid_Settings : ScriptableObject
{
    // Boid
    public float maxSpeed = 250;
    public float minSpeed = 100;
    public float avoidanceRadius = 10;
    public float maxSteerForce = 3;
    public float minSteerForce = .5f;

    public float lingerTime = 2.5f;
    public float alignmentWeight = 1;
    public float cohesionWeight = 1;
    public float seperationWeight = 1;
    public float targetWeight = 1;

    // Spawner
    public float awarenessRadius = 2.5f;
}
