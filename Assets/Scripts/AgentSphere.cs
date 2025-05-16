using UnityEngine;
using Unity.MLAgents;         // Core ML-Agents namespace
using Unity.MLAgents.Sensors; // For observations like RayPerceptionSensor
using Unity.MLAgents.Actuators; // For defining actions

public class AgentSphereAgent : Agent // IMPORTANT: Your class must inherit from "Agent"
{
    [Header("Agent Settings")]
    public Transform targetTransform; // Assign your TargetSphere to this in the Inspector
    public float moveForce = 10f;    // How much force to apply for movement
    
    [Header("Reward Shaping Settings")]
    public float distanceRewardMultiplier = 0.01f; // Multiplier for the reward when getting closer
    // Optional: public float awayPenaltyMultiplier = 0.005f; // Multiplier for penalty when moving away

    private Rigidbody rb;
    private Vector3 startingPosition;       // To reset the agent's position
    private float previousDistanceToTarget; // To store distance from the last step for reward shaping

    // Called once when the agent is first initialized
    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component of this agent
        startingPosition = transform.localPosition; // Store the initial local position

        if (rb == null)
        {
            Debug.LogError("Rigidbody component not found on AgentSphereAgent. Please add one.", this);
        }
        if (targetTransform == null)
        {
            Debug.LogError("Target Transform has not been assigned in the Inspector for AgentSphereAgent!", this);
        }
    }

    // Called every time a new training episode begins
    public override void OnEpisodeBegin()
    {
        // Reset agent's physical properties
        if (rb != null)
        {
            rb.velocity = Vector3.zero;         // Stop any existing movement
            rb.angularVelocity = Vector3.zero;  // Stop any existing rotation
        }
        transform.localPosition = startingPosition; // Move agent back to its starting point

        // --- Target Repositioning (Example: Randomly place target) ---
        // You might have more sophisticated logic for this, or a fixed target for now.
        // If the target moves, ensure previousDistanceToTarget is calculated *after* it moves.
        if (targetTransform != null)
        {
            // Example: Move target to a random position on a hypothetical floor plane
            // float floorRangeX = 18f; // Example half-width of your traversable area
            // float floorRangeZ = 18f; // Example half-length of your traversable area
            // targetTransform.localPosition = new Vector3(Random.Range(-floorRangeX, floorRangeX),
            //                                           0.5f, // Assuming target Y position is 0.5
            //                                           Random.Range(-floorRangeZ, floorRangeZ));

            // Ensure the agent doesn't spawn on top of the target, or target too close initially.
            // This might require more checks if you have complex spawning.

            // Initialize/reset previousDistanceToTarget based on new positions
            previousDistanceToTarget = Vector3.Distance(transform.localPosition, targetTransform.localPosition);
        }
        else
        {
            // Fallback or error if target is not set
            previousDistanceToTarget = float.MaxValue; 
        }
    }

    // What the agent "sees" or "knows" about the environment
    public override void CollectObservations(VectorSensor sensor)
    {
        if (targetTransform == null) return; // Don't collect if target is missing

        // Observe the agent's own velocity (helps it understand its current momentum)
        // Normalizing velocity can be useful: sensor.AddObservation(rb.velocity.normalized);
        sensor.AddObservation(rb.velocity.x); // 1 observation
        sensor.AddObservation(rb.velocity.z); // 1 observation

        // Observe the direction and distance to the target
        Vector3 directionToTarget = (targetTransform.localPosition - transform.localPosition).normalized;
        sensor.AddObservation(directionToTarget.x); // 1 observation
        sensor.AddObservation(directionToTarget.z); // 1 observation
        sensor.AddObservation(Vector3.Distance(transform.localPosition, targetTransform.localPosition)); // 1 observation

        // Total manual observations: 2 (velocity) + 3 (direction/distance to target) = 5
        // Raycast observations (from RayPerceptionSensor3D) are added automatically by the sensor component.
    }

    // What happens when the agent receives an action from the neural network (or heuristic input)
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Get actions (assuming 2 continuous actions for X and Z force)
        float moveX = actions.ContinuousActions[0]; // Value between -1 and 1
        float moveZ = actions.ContinuousActions[1]; // Value between -1 and 1

        // Apply force to the agent
        if (rb != null)
        {
            Vector3 force = new Vector3(moveX, 0f, moveZ) * moveForce;
            rb.AddForce(force);
        }

        // --- Reward Shaping: Getting closer to target ---
        if (targetTransform != null)
        {
            float currentDistanceToTarget = Vector3.Distance(transform.localPosition, targetTransform.localPosition);

            float distanceDelta = previousDistanceToTarget - currentDistanceToTarget; // Positive if we got closer

            if (distanceDelta > 0) // We moved closer
            {
                AddReward(distanceDelta * distanceRewardMultiplier);
            }
            // Optional: Penalty for moving further away
            // else if (distanceDelta < 0) // We moved further away
            // {
            //     // distanceDelta is negative, so this product will also be negative (a penalty)
            //     AddReward(distanceDelta * awayPenaltyMultiplier); 
            // }

            previousDistanceToTarget = currentDistanceToTarget; // Update for the next step
        }

        // Small negative reward per step to encourage efficiency (and finishing the episode)
        AddReward(-0.001f);
    }

    // For manual testing: How to control the agent using keyboard input
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right keys
        continuousActions[1] = Input.GetAxisRaw("Vertical");   // W/S or Up/Down keys
    }

    // Called when this collider/rigidbody has begun touching another rigidbody/collider.
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-1.0f); // Significant penalty for hitting an obstacle or wall
            EndEpisode();     // End the episode
        }
    }

    // Called when the Collider other enters the trigger. (Target should be a trigger)
    void OnTriggerEnter(Collider other)
{
    if (other.gameObject.CompareTag("Target"))
    {
        AddReward(1.0f);  // Reward for reaching the target

        // --- NEW: Get the TargetMovement script and move the target ---
        TargetMovement targetScript = other.gameObject.GetComponent<TargetMovement>();
        if (targetScript != null)
        {
            targetScript.MoveTargetToRandomPosition();
        }
        else
        {
            Debug.LogWarning("TargetSphere does not have TargetMovement script attached!");
        }
        // --- END OF NEW CODE ---

        EndEpisode();     // End the episode (agent will reset, target is now in a new spot)
    }
}
}