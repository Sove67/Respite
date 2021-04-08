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
    private readonly List<GameObject> swarm = new List<GameObject>();

    // Pathfinding Logic
    private readonly List<(GameObject, bool)> targets = new List<(GameObject, bool)>();

    // Self
    private Rigidbody body;
    private SphereCollider awareness;
    private NavMeshAgent agent;

    // Linger Targeting
    public Vector3 lingerForce;

    // Functions
    private void Start()
    {
        body = GetComponent<Rigidbody>();
        awareness = GetComponentInChildren<SphereCollider>();
        awareness.radius = settings.awarenessRadius;
        agent = GetComponent<NavMeshAgent>();
        lingerForce = Vector3.zero;
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
            ValidateTargets();
            agent.SetDestination(PriorityTarget().transform.position);
            agent.enabled = true;
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

    public void ShareTargetWithSwarm(GameObject target)
    {
        foreach (GameObject boid in swarm)
        {
            boid.GetComponent<Enemy_AI>().AddTarget(target, false);
        }
    }

    public void AddTarget(GameObject target, bool primarySource)
    {
        int index = targets.FindIndex(item => item.Item1 == target);
        if (index != -1) // Update existing target
        {
            bool currentSource = targets[index].Item2;
            targets[index] = (target, currentSource || primarySource);
        } else // Add new target
        {
            targets.Add((target, primarySource));
        }
    }

    public void PurgeSwarm(GameObject target) // Remove the target from this boid's swarm
    {
        swarm.Remove(target);
    }

    public void PurgeTarget(GameObject target, bool isInitiator) // Remove the target from this boid's targets & it's swarmmates if it is the initiator. Do nothing if it is a primary target and not the initiator.
    {
        (GameObject, bool) listing = targets.Find(items => items.Item1 == target);
        if (listing.Item2 && isInitiator) // Is the primary contact & called the removal
        {
            targets.Remove(listing);
            foreach (GameObject boid in swarm)
            {
                boid.GetComponent<Enemy_AI>().PurgeTarget(target, false);
            }
        } else if (!listing.Item2 && !isInitiator) // Is the secondary contact & did not call the removal
        {
            targets.Remove(listing);
        }
    }

    // Entity Awareness Assignment
    private void OnTriggerEnter(Collider collider)
    {
        // Swarm Assignment
        if (collider.CompareTag("Enemy"))
        { swarm.Add(collider.gameObject); }

        // Target Assignment
        else if (collider.CompareTag("Player") || collider.CompareTag("Base Core"))
        { 
            AddTarget(collider.gameObject, true); 
            ShareTargetWithSwarm(collider.gameObject); 
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        // Swarm Removal
        if (collider.CompareTag("Enemy"))
        {
            PurgeSwarm(collider.gameObject);
        }

        // Target Removal
        else if (collider.CompareTag("Player") || collider.CompareTag("Base Core"))
        {
            StartCoroutine(Pursue());
            PurgeTarget(collider.gameObject, true);
        }
    }

    // Core Boid Logic
    public void Boid_Control() // Apply all forces to rigidbody
    {
        IEnumerable<Enemy_AI> boids = swarm.Select(o => o.GetComponent<Enemy_AI>()).ToList();
        Vector3 desiredForce = Alignment(boids) + Separation(boids) + Cohesion(boids) + Avoidance() + lingerForce;
        Debug.DrawRay(transform.position, lingerForce, Color.white);

        Vector3 inputVelocity = LimitVector3(desiredForce * Time.fixedDeltaTime, settings.minSteerForce, settings.maxSteerForce); // Convert force to velocity
        Debug.DrawRay(transform.position, inputVelocity, Color.red);
        Vector3 resultVelocity = body.velocity + inputVelocity;
        body.velocity = resultVelocity.normalized * settings.speed;
        float angle = Mathf.Atan2(body.velocity.z, body.velocity.x) * Mathf.Rad2Deg + 90;
        body.transform.rotation = Quaternion.Euler(new Vector3(0, angle, 0));
    }

    private Vector3 Alignment(IEnumerable<Enemy_AI> boids) // Move in the same direction as the group
    {
        Vector3 force = Vector3.zero;
        if (!boids.Any()) return force;

        foreach (Enemy_AI boid in boids)
        { force += boid.body.velocity; }
        force /= boids.Count();

        Debug.DrawRay(transform.position, force * settings.alignmentWeight, Color.blue);
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

        Debug.DrawRay(transform.position, force * settings.cohesionWeight, Color.green);
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

        Debug.DrawRay(transform.position, force * settings.seperationWeight, Color.black);
        return force * settings.seperationWeight;
    }

    private Vector3 Avoidance()
    {
        Vector3 force = Vector3.zero;
        int layerMask = 1 << settings.avoidanceLayer;

        for (int angle = 0; angle < 360; angle += settings.avoidanceStepMagnitude)
        {
            Vector3 direction = Quaternion.Euler(0, angle, 0) * body.velocity.normalized;

            if (Physics.SphereCast(transform.position, settings.avoidancePathWidth, direction, out RaycastHit hit, settings.awarenessRadius - settings.avoidancePathWidth, layerMask))
            {
                // For each failed ray, add an avoidance force based on distance between origin and ray end
                force += body.position - hit.point;
            }
        }

        force /= Mathf.Ceil(360 / settings.avoidanceStepMagnitude);

        force = force.magnitude > 0 ? force : Vector3.zero;
        Debug.DrawRay(transform.position, force * settings.avoidanceWeight, Color.cyan);
        return force * settings.avoidanceWeight;
    }

    private IEnumerator Pursue()
    {
        body.velocity = agent.desiredVelocity;
        lingerForce = agent.desiredVelocity * settings.lingerWeight;

        yield return new WaitForSeconds(settings.lingerTime);

        lingerForce = Vector3.zero;
    }

    // Trap Interaction
    private void OnCollisionStay(Collision collision)
    {
        // Force
        Vector3 force;
        if (agent.enabled)
        { force = agent.desiredVelocity.normalized * settings.collisionForceMultiplier; }
        else
        { force = body.velocity.normalized * settings.collisionForceMultiplier; }

        if (collision.rigidbody != null)
        {
            collision.rigidbody.AddForce(force);
        }

        // Damage
        Entity entity = collision.gameObject.GetComponent<Entity>();
        if (entity != null)
        {
            entity.ModifyHealth(-settings.damage);
        }
    }

    // Calculation Helpers
    private Vector3 LimitVector3(Vector3 baseVector, float min, float max)
    {
        float sqrMagnitude = baseVector.sqrMagnitude;

        if (sqrMagnitude == 0) // No Direction - Generate a new direction
        { return Quaternion.Euler(0, Random.Range(0, 360), 0) * Vector3.forward * min; }

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

        int index = 0;
        while (lure == null && index < targets.Count) // Find the first occurence of each priority level
        {
            GameObject item = targets[index].Item1;
            if (item.CompareTag("Lure"))
            { lure = item; }

            else if (item.CompareTag("Base Core") && core == null)
            { core = item; }

            else if (item.CompareTag("Player") && player == null)
            { player = item; }


            index++;
        }

        // Assign the highest priority found as the target, null otherwise
        GameObject target = null;
        if (lure != null) { target = lure; }
        else if (core != null) { target = core; }
        else if (player != null) { target = player; }

        return target;
    }

    private void ValidateTargets()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i].Item1 == null)
            {
                targets.RemoveAt(i);
                i--;
            }
        }
    }
}
