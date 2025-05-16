using UnityEngine;
using Unity.MLAgents; // Core ML-Agents namespace
using Unity.MLAgents.Sensors; // For observations like RayPerceptionSensor
using Unity.MLAgents.Actuators; // For defining actions

public class AgentSphereAgent : Agent // IMPORTANT: Your class must inherit from "Agent"
{
    public Transform targetTransform; // Assign your TargetSphere to this in the Inspector
    public float moveForce = 10f;    // How much force to apply for movement

    private Rigidbody rb;
    private Vector3 startingPosition; // To reset the agent's position
    // We might not need the AgentController if we handle heuristic input here directly

    // Called once when the agent is first initialized
    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component of this agent
        startingPosition = transform.localPosition; // Store the initial local position

        if (targetTransform == null)
        {
            Debug.LogError("Target Transform has not been assigned in the Inspector for AgentSphereAgent!");
        }
    }

    // Called every time a new training episode begins
    public override void OnEpisodeBegin()
    {
        // Reset agent's properties
        rb.velocity = Vector3.zero; // Stop any existing movement
        rb.angularVelocity = Vector3.zero; // Stop any existing rotation
        transform.localPosition = startingPosition; // Move agent back to its starting point

        // For now, let's assume the target also resets to a fixed position or a simple random one.
        // Later, we can make the target move more dynamically.
        // Example: Move target to a random position on the floor (assuming floor is at Y=0 and within certain bounds)
        // float floorSizeX = 18f; // Example floor size (half-width)
        // float floorSizeZ = 18f; // Example floor size (half-length)
        // targetTransform.localPosition = new Vector3(Random.Range(-floorSizeX, floorSizeX),
        //                                       0.5f, // Assuming target Y position
        //                                       Random.Range(-floorSizeZ, floorSizeZ));
        // For now, let's keep the target static as placed in the editor, or reset it to its own start.
        // If your target has a script to manage its starting position, that's good too.
        // For simplicity, ensure target is at a known, reachable spot for initial training.
    }

    // What the agent "sees" or "knows" about the environment
    public override void CollectObservations(VectorSensor sensor)
    {
        // Observe the agent's own velocity (helps it understand its current momentum)
        // Normalized velocity can sometimes be better, but let's start simple.
        sensor.AddObservation(rb.velocity.x); // 1 observation
        sensor.AddObservation(rb.velocity.z); // 1 observation (we're mainly moving on XZ plane)

        // Observe the direction and distance to the target
        Vector3 directionToTarget = (targetTransform.localPosition - transform.localPosition).normalized;
        sensor.AddObservation(directionToTarget.x); // 1 observation
        sensor.AddObservation(directionToTarget.z); // 1 observation
        sensor.AddObservation(Vector3.Distance(targetTransform.localPosition, transform.localPosition)); // 1 observation

        // Total observations so far: 2 (velocity) + 3 (direction/distance to target) = 5
        // Raycast observations will be added automatically if you have a RayPerceptionSensor component attached.
    }

    // What happens when the agent receives an action from the neural network (or heuristic input)
    public override void OnActionReceived(ActionBuffers actions)
    {
        // We defined 2 continuous actions (for X and Z force)
        float moveX = actions.ContinuousActions[0]; // Value between -1 and 1
        float moveZ = actions.ContinuousActions[1]; // Value between -1 and 1

        Vector3 force = new Vector3(moveX, 0f, moveZ) * moveForce;
        rb.AddForce(force);

        // --- Rewards ---
        // Small negative reward per step to encourage efficiency
        AddReward(-0.001f);

        // (We'll add rewards for reaching the target and penalties for hitting obstacles later
        // using OnTriggerEnter and OnCollisionEnter)
    }

    // For manual testing: How to control the agent using keyboard input
    // This maps keyboard keys to the same action space the AI will use.
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right keys -> maps to moveX
        continuousActions[1] = Input.GetAxisRaw("Vertical");   // W/S or Up/Down keys -> maps to moveZ
    }

    // Example for collision detection (we'll refine this tomorrow)
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Obstacle") || col.gameObject.CompareTag("Wall"))
        {
            AddReward(-1.0f); // Big penalty for hitting an obstacle or wall
            EndEpisode();     // End the episode
        }
    }

    // Example for reaching the target (remember TargetSphere should be a Trigger)
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Target"))
        {
            AddReward(1.0f);  // Big reward for reaching the target
            EndEpisode();     // End the episode
        }
    }
}