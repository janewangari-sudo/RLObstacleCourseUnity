using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic; // Needed for List<>

public class AgentSphereAgent : Agent
{
    [Header("Agent Settings")]
    public Transform targetTransform;
    public float moveForce = 10f;

    [Header("Reward Shaping Settings")]
    public float distanceRewardMultiplier = 0.01f;

    [Header("Procedural Obstacle Settings")]
    public GameObject obstacleToSpawnPrefab; // Assign your "SpawnableObstacle_Static" prefab
    public int minObstaclesToSpawn = 2;
    public int maxObstaclesToSpawn = 4;
    public Transform obstaclesParentTransform; // Optional: An empty GameObject to keep spawned obstacles organized

    [Header("Spawning Area Bounds")]
    public float spawnAreaMinX = -15f;
    public float spawnAreaMaxX = 15f;
    public float spawnAreaMinZ = -15f;
    public float spawnAreaMaxZ = 15f;
    public float obstacleSpawnY = 0.5f; // Y position for spawned obstacles (base on floor)
    public float minSpawnDistFromAgentStart = 3f; // Don't spawn too close to agent's fixed start
    // public float minSpawnDistFromTargetStart = 3f; // Less critical if target moves when reached
    public float minDistBetweenSpawnedObstacles = 2.0f; // Min distance between newly spawned obstacles

    private Rigidbody rb;
    private Vector3 startingPosition;
    private float previousDistanceToTarget;
    private List<GameObject> spawnedObstacleList = new List<GameObject>(); // To keep track of spawned obstacles

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        startingPosition = transform.localPosition; // Agent always starts here

        if (rb == null) Debug.LogError("Rigidbody component not found on AgentSphereAgent!", this);
        if (targetTransform == null) Debug.LogError("Target Transform has not been assigned for AgentSphereAgent!", this);
        if (obstacleToSpawnPrefab == null) Debug.LogWarning("ObstacleToSpawnPrefab not assigned. No procedural obstacles will be spawned.", this);
    }

    public override void OnEpisodeBegin()
    {
        // --- Reset Agent ---
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        transform.localPosition = startingPosition;

        // --- Clear Previously Spawned Obstacles ---
        foreach (GameObject obs in spawnedObstacleList)
        {
            Destroy(obs);
        }
        spawnedObstacleList.Clear();

        // --- Spawn New Random Obstacles ---
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
                        foreach (GameObject existingObs in spawnedObstacleList)
                        {
                            if (Vector3.Distance(potentialPosition, existingObs.transform.localPosition) < minDistBetweenSpawnedObstacles)
                            {
                                overlap = true;
                                break;
                            }
                        }
                    }

                    if (!overlap)
                    {
                        GameObject newObstacle = Instantiate(obstacleToSpawnPrefab, potentialPosition, Quaternion.identity);
                        if (obstaclesParentTransform != null) // Organize in hierarchy if a parent is assigned
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

        // --- Initialize/Reset previousDistanceToTarget ---
        // This should be done *after* the agent is at its start and the target is in its initial position for the episode.
        // Since your target moves when reached (not at episode start typically), its position here is its last known position.
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
        if (targetTransform == null) return;

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

        if (rb != null)
        {
            Vector3 force = new Vector3(moveX, 0f, moveZ) * moveForce;
            rb.AddForce(force);
        }

        if (targetTransform != null)
        {
            float currentDistanceToTarget = Vector3.Distance(transform.localPosition, targetTransform.localPosition);
            float distanceDelta = previousDistanceToTarget - currentDistanceToTarget;

            if (distanceDelta > 0)
            {
                AddReward(distanceDelta * distanceRewardMultiplier);
            }
            // Optional penalty for moving away could be added here
            previousDistanceToTarget = currentDistanceToTarget;
        }

        AddReward(-0.001f); // Per-step penalty
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    void OnCollisionEnter(Collision collision)
    {
        // Remember: Your manually placed static obstacles and your MovingPillar1 should also be tagged "Obstacle".
        // The procedurally spawned ones from obstacleToSpawnPrefab will also need the "Obstacle" tag on the prefab itself.
        if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-1.0f);
            EndEpisode();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Target"))
        {
            AddReward(1.0f);
            // Target already moves due to TargetMovement.cs script attached to the target object itself,
            // which is called from this agent script when it was not procedural.
            // Let's ensure the target moves. If TargetMovement.cs is on the target, this is one way:
            TargetMovement targetScript = other.gameObject.GetComponent<TargetMovement>();
            if (targetScript != null)
            {
                targetScript.MoveTargetToRandomPosition();
            }
            EndEpisode();
        }
    }
}