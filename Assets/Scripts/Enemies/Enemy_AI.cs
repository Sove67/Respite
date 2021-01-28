using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Enemy_AI : MonoBehaviour
{
    // Variables
    // Boid Logic
    public Enemy_Settings settings;
    private List<GameObject> swarm = new List<GameObject>();

    // Pathfinding Logic
    private List<GameObject> targets = new List<GameObject>();

    // Self
    private Rigidbody body;
    private SphereCollider awareness;
    private NavMeshAgent agent;



    // Functions
    private void Start()
    {
        body = GetComponent<Rigidbody>();
        awareness = GetComponentInChildren<SphereCollider>();
        awareness.radius = settings.awarenessRadius;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        
        if (!(targets.Count > 0)) // If there are no targets, wander.
        {
            agent.enabled = false;
            Boid_Control(); 
        }
        else // Otherwise, pathfind to the priority target
        {
            agent.enabled = true;
            agent.SetDestination(PriorityTarget().transform.position);
        }
    }

    // Control
    public void Die() // Kill this boid
    {
        foreach (GameObject boid in swarm)
        {
            boid.GetComponent<Enemy_AI>().PurgeSwarm(gameObject);
        }

        Destroy(gameObject);
    }

    public void PurgeSwarm(GameObject target) // Remove the target from this boid's swarm
    {
        swarm.Remove(target);
    }

    public void PurgeTarget(GameObject target) // Remove the target from this boid's targets
    {
        targets.Remove(target);
    }

    // Entity Awareness Assignment
    private void OnTriggerEnter(Collider collider)
    {
        // Swarm Assignment
        if (collider.CompareTag("Enemy"))
        { swarm.Add(collider.gameObject); }

        // Target Assignment
        else if (collider.CompareTag("Defense") || collider.CompareTag("Player") || collider.CompareTag("Lure"))
        { targets.Add(collider.gameObject); }
    }

    private void OnTriggerExit(Collider collider)
    {
        // Swarm Removal
        if (collider.CompareTag("Enemy"))
        {
            PurgeSwarm(collider.gameObject);
        }

        // Target Removal
        else if (collider.CompareTag("Defense"))
        {
            PurgeTarget(collider.gameObject);
        }
        else if (collider.CompareTag("Player") || collider.CompareTag("Lure"))
        {
            targets.Add(Instantiate(settings.trace, collider.transform.position, Quaternion.identity, transform));
            PurgeTarget(collider.gameObject);
        }
    }

    // Core Boid Logic
    public void Boid_Control() // Apply all forces to rigidbody
    {
        IEnumerable<Enemy_AI> boids = swarm.Select(o => o.GetComponent<Enemy_AI>()).ToList();
        Vector3 desiredForce = Alignment(boids) + Separation(boids) + Cohesion(boids) + Avoidance();
        Vector3 limitedForce = LimitVector3(desiredForce, settings.minSteerForce, settings.maxSteerForce);
        Vector3 inputVelocity = limitedForce.normalized * (limitedForce.magnitude / body.mass) * Time.fixedDeltaTime; // Taken from https://forum.unity.com/threads/calculating-velocity-from-addforce-and-mass.166557/
        Vector3 resultVelocity = body.velocity + inputVelocity;
        body.velocity = LimitVector3(resultVelocity, settings.minSpeed, settings.maxSpeed);

        if (body.velocity.magnitude > .6) // Stops twitching from tiny speeds
        {
            float angle = Mathf.Atan2(body.velocity.z, body.velocity.x) * Mathf.Rad2Deg;
            body.transform.rotation = Quaternion.Euler(new Vector3(0, angle, 0));
        }
    }

    private Vector3 Alignment(IEnumerable<Enemy_AI> boids) // Move in the same direction as the group
    {
        Vector3 force = Vector3.zero;
        if (!boids.Any()) return force;

        foreach (Enemy_AI boid in boids)
        { force += boid.body.velocity; }
        force /= boids.Count();

        return force * settings.alignmentWeight;
    }

    private Vector3 Cohesion(IEnumerable<Enemy_AI> boids) // Move towards group centre
    {
        if (!boids.Any()) return Vector3.zero;

        Vector3 sumPositions = Vector3.zero;
        foreach (Enemy_AI boid in boids)
        { sumPositions += boid.body.position; }
        Vector3 average = sumPositions / boids.Count();
        Vector3 force = average - body.position;

        return force * settings.cohesionWeight;
    }

    private Vector3 Separation(IEnumerable<Enemy_AI> boids) // Move away from nearby boids
    {
        Vector3 force = Vector3.zero;
        boids = boids.Where(o => Vector3.Distance(o.transform.position, body.position) <= settings.separationRadius);
        if (!boids.Any()) return force;

        foreach (Enemy_AI boid in boids)
        {
            force += body.position - boid.body.position;
        }
        force /= boids.Count();

        return force * settings.seperationWeight;
    }

    private Vector3 Avoidance()
    {
        Vector3 force = Vector3.zero;
        int layerMask = 1 << settings.avoidanceLayer;

        for (int angle = 0; angle < 360; angle += settings.avoidanceStepMagnitude)
        {
            Vector3 direction = Quaternion.Euler(0, angle, 0) * body.velocity.normalized;

            RaycastHit hit;
            if (Physics.SphereCast(transform.position, settings.avoidancePathWidth, direction, out hit, settings.awarenessRadius - settings.avoidancePathWidth, layerMask))
            {
                Debug.DrawLine(transform.position, hit.point, Color.red, .1f);
                // For each failed ray, add an avoidance force based on distance between origin and ray end
                force += body.position - hit.point;
            }
        }

        force /= Mathf.Ceil(360 / settings.avoidanceStepMagnitude);

        force = force.magnitude > 0 ? force : Vector3.zero;
        return force * settings.avoidanceWeight;
    }

    // Calculation Helpers
    private Vector3 LimitVector3(Vector3 baseVector, float min, float max) {
        float sqrMagnitude = baseVector.sqrMagnitude;

        if (sqrMagnitude == 0) // No Direction - Generate a new direction
        { return Quaternion.Euler(0, Random.Range(0, 360), 0) * Vector3.forward * min;  }

        else if (sqrMagnitude < min * min) // Too Small - Increase the speed of current direction
        { return baseVector.normalized * min; }

        else if (sqrMagnitude > max * max) // Too Big - Decrease the speed of current direction
        { return baseVector.normalized * max; }

        return baseVector;
    }

    private GameObject PriorityTarget()
    {
        GameObject lure = null;
        GameObject player = null;
        GameObject core = null;
        GameObject defense = null;
        GameObject trace = null;

        int index = 0;
        while (lure == null && index < targets.Count) // Find the first occurence of each priority level
        {
            if (targets[index].CompareTag("Lure"))
            { lure = targets[index]; }

            else if (targets[index].CompareTag("Player") && player == null)
            { player = targets[index]; }

            else if (targets[index].CompareTag("Base Core") && core == null)
            { core = targets[index]; }

            else if (targets[index].CompareTag("Defense") && defense == null)
            { defense = targets[index]; }

            else if (targets[index].CompareTag("Trace") && trace == null)
            { trace = targets[index]; }

            index++;
        }

        // Assign the highest priority found as the target, null otherwise
        GameObject target = null;
        if (lure != null) { target = lure; } 
        else if (player != null) { target = player; }
        else if (core != null) { target = core; }
        else if (defense != null) { target = defense; }
        else if (trace != null) { target = trace; }

        return target;
    }
}
