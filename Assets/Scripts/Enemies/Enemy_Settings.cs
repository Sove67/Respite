using UnityEngine;

[CreateAssetMenu]
public class Enemy_Settings : ScriptableObject
{
    // General
    public float awarenessRadius = 5;

    // Spawner
    public int spawnTimer = 20;
    public float radiusMin = 40;
    public float radiusMax = 80;
    public int popMin = 40;
    public int popMax = 70;
    public float packCount = 3;

    // Boid
    public float maxSpeed = 5;
    public float minSpeed = 1;
    public float maxSteerForce = 3;
    public float minSteerForce = .5f;

    public float alignmentWeight = 1;
    public float cohesionWeight = 1;
    public float seperationWeight = 2;
    public float avoidanceWeight = 1;

    public float separationRadius = 20;
    public int avoidanceLayer = 8;
    public int avoidanceStepMagnitude = 20;
    public float avoidancePathWidth = .5f;

    // Targeting
    public float traceTime = 5;
    public GameObject trace;
}
