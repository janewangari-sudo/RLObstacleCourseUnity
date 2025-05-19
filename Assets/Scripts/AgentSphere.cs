using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic; // Needed for List<>

public class AgentSphereAgent : Agent
{
    [Header("Agent Settings")]
    public Transform targetTransform;
    public float moveForce = 10f; // You had tuned this to 5f, remember to set it in Inspector!

    [Header("Reward Shaping Settings")]
    public float distanceRewardMultiplier = 0.01f;
    // Add this with your other public variables, e.g., under [Header("Reward Shaping Settings")]
    public float wrongDirectionPenalty = -0.02f; // Penalty amount (should be negative)
    public float wrongDirectionThreshold = -0.5f; // Dot product threshold (e.g., -0.5 means moving more than 120 degrees away from target)

    [Header("Procedural Obstacle Settings")]
    public GameObject obstacleToSpawnPrefab;
    public int minObstaclesToSpawn = 2;
    public int maxObstaclesToSpawn = 4;
    public Transform obstaclesParentTransform; // Optional

    [Header("Fixed Obstacles References")] // To avoid spawning procedural obstacles on these
    public List<Transform> fixedObstaclesList; // Assign your Obstacle1,2,3 and MovingPillar1 here

    [Header("Spawning Area Bounds")]
    public float spawnAreaMinX = -15f;
    public float spawnAreaMaxX = 15f;
    public float spawnAreaMinZ = -15f;
    public float spawnAreaMaxZ = 15f;
    public float obstacleSpawnY = 0.75f; // Corrected value from our previous discussion
    public float minSpawnDistFromAgentStart = 3f;
    public float minSpawnDistFromFixedObstacles = 3.0f; // Tune this distance as needed
    public float minDistBetweenSpawnedObstacles = 2.0f;

    private Rigidbody rb;
    private Vector3 startingPosition;
    private float previousDistanceToTarget;
    private List<GameObject> spawnedObstacleList = new List<GameObject>();

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        startingPosition = transform.localPosition;

        if (rb == null) Debug.LogError("Rigidbody component not found on AgentSphereAgent!", this);
        if (targetTransform == null) Debug.LogError("Target Transform has not been assigned for AgentSphereAgent!", this);
        if (obstacleToSpawnPrefab == null) Debug.LogWarning("ObstacleToSpawnPrefab not assigned. No procedural obstacles will be spawned.", this);
        if (fixedObstaclesList == null || fixedObstaclesList.Count == 0) Debug.LogWarning("FixedObstaclesList is not assigned or is empty. Procedural obstacles might spawn on top of fixed ones.", this);
    }

    public override void OnEpisodeBegin()
    {
        // Reset Agent
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        transform.localPosition = startingPosition;

        // Clear Previously Spawned Obstacles
        foreach (GameObject obs in spawnedObstacleList)
        {
            Destroy(obs);
        }
        spawnedObstacleList.Clear();

        // Spawn New Random Obstacles
        if (obstacleToSpawnPrefab != null)
        {
            int numberOfObstacles = Random.Range(minObstaclesToSpawn, maxObstaclesToSpawn + 1);
            for (int i = 0; i < numberOfObstacles; i++)
            {
                int placementAttempts = 0;
                bool placedSuccessfully = false;
                while (!placedSuccessfully && placementAttempts < 20) // Try up to 20 times
                {
                    placementAttempts++;
                    Vector3 potentialPosition = new Vector3(
                        Random.Range(spawnAreaMinX, spawnAreaMaxX),
                        obstacleSpawnY,
                        Random.Range(spawnAreaMinZ, spawnAreaMaxZ)
                    );

                    bool overlap = false;
                    // Check against agent's fixed starting position
                    if (Vector3.Distance(potentialPosition, startingPosition) < minSpawnDistFromAgentStart)
                    {
                        overlap = true;
                    }

                    // Check against other newly spawned obstacles in this episode
                    if (!overlap)
                    {
                        foreach (GameObject existingSpawnedObs in spawnedObstacleList)
                        {
                            if (Vector3.Distance(potentialPosition, existingSpawnedObs.transform.localPosition) < minDistBetweenSpawnedObstacles)
                            {
                                overlap = true;
                                break;
                            }
                        }
                    }

                    // --- NEW CHECK: Against manually placed fixed/moving obstacles ---
                    if (!overlap && fixedObstaclesList != null)
                    {
                        foreach (Transform fixedObsTransform in fixedObstaclesList)
                        {
                            if (fixedObsTransform != null) // Check if the slot in the list is filled
                            {
                                if (Vector3.Distance(potentialPosition, fixedObsTransform.position) < minSpawnDistFromFixedObstacles)
                                {
                                    overlap = true;
                                    break;
                                }
                            }
                        }
                    }
                    // --- END OF NEW CHECK ---

                    if (!overlap)
                    {
                        GameObject newObstacle = Instantiate(obstacleToSpawnPrefab, potentialPosition, Quaternion.identity);
                        if (obstaclesParentTransform != null)
                        {
                            newObstacle.transform.SetParent(obstaclesParentTransform);
                        }
                        spawnedObstacleList.Add(newObstacle);
                        placedSuccessfully = true;
                    }
                }
                // if (!placedSuccessfully) Debug.LogWarning("Could not place an obstacle without overlap after attempts.");
            }
        }

        // Initialize/Reset previousDistanceToTarget
        if (targetTransform != null)
        {
            previousDistanceToTarget = Vector3.Distance(transform.localPosition, targetTransform.localPosition);
        }
        else
        {
            previousDistanceToTarget = float.MaxValue;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (targetTransform == null || rb == null) return;

        sensor.AddObservation(rb.velocity.x);
        sensor.AddObservation(rb.velocity.z);

        Vector3 directionToTarget = (targetTransform.localPosition - transform.localPosition).normalized;
        sensor.AddObservation(directionToTarget.x);
        sensor.AddObservation(directionToTarget.z);
        sensor.AddObservation(Vector3.Distance(transform.localPosition, targetTransform.localPosition));
    }

    public override void OnActionReceived(ActionBuffers actions)
{
    float moveX = actions.ContinuousActions[0];
    float moveZ = actions.ContinuousActions[1];

    Vector3 intendedMoveDirection = Vector3.zero; // To store the direction the agent intends to move

    if (rb != null)
    {
        intendedMoveDirection = new Vector3(moveX, 0f, moveZ); // This is the direction vector from actions
        Vector3 forceToApply = intendedMoveDirection.normalized * moveForce; // Normalize for consistent force magnitude if moveForce is main speed control
                                                                         // If you want varying force based on action magnitude, use:
                                                                         // Vector3 forceToApply = intendedMoveDirection * moveForce; 
                                                                         // (The version we had before)
                                                                         // For this penalty, using the normalized intendedMoveDirection is good.
        rb.AddForce(forceToApply);
    }

    if (targetTransform != null)
    {
        // --- Proximity Reward (Getting closer to target) ---
        float currentDistanceToTarget = Vector3.Distance(transform.localPosition, targetTransform.localPosition);
        float distanceDelta = previousDistanceToTarget - currentDistanceToTarget; // Positive if we got closer

        if (distanceDelta > 0) // We moved closer
        {
            AddReward(distanceDelta * distanceRewardMultiplier);
        }
        // Optional: Penalty for moving further away (could be added here too)
        // else if (distanceDelta < -0.01f) // Moved significantly further (example threshold)
        // {
        //     AddReward(distanceDelta * someOtherPenaltyMultiplier); // distanceDelta is negative
        // }
        previousDistanceToTarget = currentDistanceToTarget; // Update for the next step

        // --- NEW: Penalty for moving significantly opposite to the target ---
        if (intendedMoveDirection.sqrMagnitude > 0.01f) // Only penalize if there's actual intended movement
        {
            Vector3 directionToTargetActual = (targetTransform.localPosition - transform.localPosition).normalized;
            float alignment = Vector3.Dot(intendedMoveDirection.normalized, directionToTargetActual);

            // If alignment is less than threshold (e.g., -0.5), agent is moving >120 degrees away from target
            if (alignment < wrongDirectionThreshold)
            {
                AddReward(wrongDirectionPenalty); // Add the negative reward
                // You could also print a debug message here when testing to see when it triggers
                // Debug.Log($"Wrong direction penalty applied! Alignment: {alignment}");
            }
        }
        // --- END OF NEW PENALTY ---
    }

    // Existing small negative reward per step (to encourage efficiency)
    AddReward(-0.001f);

    // Optional: Fell off platform check
    if (transform.localPosition.y < -1f) // Adjust -1f to be below your floor
    {
        AddReward(-0.001f); // Add a penalty if you like
        EndEpisode();
    }
}

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.1f); //was -0.1
            EndEpisode();     //commented out
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Target"))
        {
            AddReward(2.0f); //was 5.0f
            TargetMovement targetScript = other.gameObject.GetComponent<TargetMovement>();
            if (targetScript != null)
            {
                targetScript.MoveTargetToRandomPosition();
            }
            EndEpisode();
        }
    }
}