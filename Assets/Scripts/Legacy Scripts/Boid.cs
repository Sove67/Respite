using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Boid : MonoBehaviour
{
    // Variables
    // Boid Logic
    public Boid_Settings settings;
    private readonly List<GameObject> swarm = new List<GameObject>();
    public List<GameObject> targets = new List<GameObject>();

    // Self
    private Rigidbody body;
    private SphereCollider awareness;
    public Vector2 velocity
    {
        get
        { return new Vector2(body.velocity.x, body.velocity.z); }
        set
        { body.velocity = new Vector3(value.x, body.velocity.y, value.y); }
    }
    public Vector2 position
    {
        get
        { return new Vector2(body.position.x, body.position.z); }
    }



    // Functions
    private void Start()
    {
        body = GetComponentInParent<Rigidbody>();
        awareness = GetComponent<SphereCollider>();
        awareness.radius = settings.awarenessRadius;
    }

    private void Update()
    {
        IEnumerable<Boid> boids = swarm.Select(o => o.GetComponentInChildren<Boid>()).ToList();

        Vector2 targetForce = Alignment(boids) + Separation(boids) + Cohesion(boids) + Target();
        Vector2 currentDirection = new Vector2(Mathf.Cos(body.rotation.y), Mathf.Sin(body.rotation.y));
        Vector2 limitedForce = LimitVector2(targetForce, currentDirection, settings.minSteerForce, settings.maxSteerForce);

        UpdatePhysics(limitedForce);
    }

    // Entity Awareness Assignment
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Zombie"))
        { swarm.Add(collider.gameObject); }
        else if (collider.CompareTag("Player") || collider.CompareTag("Lure"))
        { targets.Add(collider.gameObject); }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("Zombie"))
        { swarm.Remove(collider.gameObject); }
        else if (collider.CompareTag("Player") || collider.CompareTag("Lure"))
        {
            targets.Remove(collider.gameObject);
            StartCoroutine(LingerTarget(collider.gameObject));
        }
    }

    IEnumerator LingerTarget(GameObject target)
    {
        targets.Add(target);
        yield return new WaitForSeconds(settings.lingerTime);
        targets.Remove(target);
    }


    // Core Boid Logic
    public void UpdatePhysics(Vector2 force) // Apply all forces to rigidbody
    {
        Vector2 inputVelocity = force.normalized * (force.magnitude / body.mass) * Time.fixedDeltaTime; // Taken from https://forum.unity.com/threads/calculating-velocity-from-addforce-and-mass.166557/
        Vector2 resultVelocity = velocity + inputVelocity;
        velocity = LimitVector2(resultVelocity, velocity, settings.minSpeed, settings.maxSpeed);

        if (velocity.magnitude > .6) // Stops twitching from tiny speeds
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            body.transform.rotation = Quaternion.Euler(new Vector3(0, angle, 0));
        }
    }


    private Vector2 Alignment(IEnumerable<Boid> boids) // Move in the same direction as the group
    {
        Vector2 force = Vector2.zero;
        if (!boids.Any()) return force;

        foreach (Boid boid in boids)
        { force += boid.velocity; }
        force /= boids.Count();

        return force * settings.alignmentWeight;
    }

    private Vector2 Cohesion(IEnumerable<Boid> boids) // Move towards group centre
    {
        if (!boids.Any()) return Vector2.zero;

        Vector2 sumPositions = Vector2.zero;
        foreach (Boid boid in boids)
        { sumPositions += boid.position; }
        Vector2 average = sumPositions / boids.Count();
        Vector2 force = average - position;

        Debug.DrawLine(new Vector3(position.x, 0, position.y), new Vector3(average.x, 0, average.y));

        return force * settings.cohesionWeight;
    }

    private Vector2 Separation(IEnumerable<Boid> boids) // Move away from nearby boids
    {
        Vector2 force = Vector2.zero;
        boids = boids.Where(o => Vector3.Distance(o.transform.position, body.position) <= settings.avoidanceRadius);
        if (!boids.Any()) return force;

        foreach (Boid boid in boids)
        {
            force += position - boid.position;
        }
        force /= 1;
        force /= boids.Count();

        return force * settings.seperationWeight;
    }
    private Vector2 Target() // Move towards group centre
    {
        if (!targets.Any()) return Vector2.zero;

        Vector3 target = targets.First().transform.position;
        Vector2 distance = new Vector2(target.x, target.z) - position;

        return distance * settings.targetWeight;
    }

    // Calculation Helpers
    private Vector2 LimitVector2(Vector2 baseVector, Vector2 backupVector, float min, float max)
{
    float sqrMagnitude = baseVector.sqrMagnitude;

    if (sqrMagnitude == 0) // No Direction
    { return backupVector.normalized * min; }

    else if (sqrMagnitude < min * min) // Too Small
    { return baseVector.normalized * min; }

    else if (sqrMagnitude > max * max) // Too Big
    { return baseVector.normalized * max; }

    return baseVector;
}
}
