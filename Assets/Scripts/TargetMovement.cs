// This is the TargetMovement.cs script you are currently using (and which I provided earlier today)
using UnityEngine;
using System.Collections.Generic; // Required for using List<T>

public class TargetMovement : MonoBehaviour
{
    [Header("Movement Bounds")]
    public float floorMinX = -18f;
    public float floorMaxX = 18f;
    public float floorMinZ = -18f;
    public float floorMaxZ = 18f;
    public float targetYPosition = 0.75f; // Ensure this matches your target's desired height on the floor

    [Header("Obstacle Avoidance Settings")]
    // Assign your manually placed static obstacles (Obstacle1,2,3) AND your MovingPillar1 here
    public List<Transform> fixedObstaclesToAvoid;
    // How far the target's center should be from a *fixed* obstacle's center. Tune this based on their sizes.
    public float minDistanceFromFixedObstacle = 3.0f; 

    // Radius for OverlapSphere to check against *procedurally spawned* obstacles (tagged "Obstacle").
    // This should be roughly target_radius + spawned_obstacle_radius + small_buffer.
    // Or, more simply, a value that ensures no direct visual overlap.
    public float proceduralObstacleCheckRadius = 2.0f; 
                                                 
    public int maxPlacementAttempts = 50; // Increased attempts to find a clear spot

    // --- MISSING FROM YOUR PASTED SCRIPT, BUT WAS IN MY LAST FULL VERSION ---
    // You need a reference to the agent's spawn point to avoid spawning target there
    public Transform agentSpawnPointTransform; 
    public float minDistanceFromAgentSpawn = 4.0f; 
    // --- END OF MISSING PART ---

    void Start()
    {
        if (fixedObstaclesToAvoid == null)
        {
            fixedObstaclesToAvoid = new List<Transform>();
            Debug.LogWarning("TargetMovement: FixedObstaclesToAvoid list was not assigned in Inspector...", this.gameObject);
        }
        // --- ADD THIS NULL CHECK IF YOU ADD agentSpawnPointTransform ---
        if (agentSpawnPointTransform == null)
        {
            Debug.LogError("TargetMovement: AgentSpawnPointTransform not assigned in Inspector! Target might spawn on agent's start position.", this.gameObject);
        }
        // --- END OF ADDITION ---
        MoveTargetToRandomPosition(); 
    }

    public void MoveTargetToRandomPosition()
    {
        bool placedSuccessfully = false;
        Vector3 potentialTargetPosition = Vector3.zero;

        for (int attempts = 0; attempts < maxPlacementAttempts; attempts++)
        {
            potentialTargetPosition = new Vector3(
                Random.Range(floorMinX, floorMaxX),
                targetYPosition,
                Random.Range(floorMinZ, floorMaxZ)
            );

            bool overlapDetected = false;

            // --- ADD THIS CHECK FIRST ---
            // 1. Check against agent's spawn point
            if (agentSpawnPointTransform != null)
            {
                if (Vector3.Distance(potentialTargetPosition, agentSpawnPointTransform.position) < minDistanceFromAgentSpawn)
                {
                    overlapDetected = true;
                }
            }
            // --- END OF ADDITION ---

            // 2. Check against the explicitly listed 'fixedObstaclesToAvoid' (if no overlap with agent spawn yet)
            if (!overlapDetected && fixedObstaclesToAvoid != null)
            {
                foreach (Transform obsTransform in fixedObstaclesToAvoid)
                {
                    if (obsTransform != null) 
                    {
                        if (Vector3.Distance(potentialTargetPosition, obsTransform.position) < minDistanceFromFixedObstacle)
                        {
                            overlapDetected = true;
                            break; 
                        }
                    }
                }
            }

            if (overlapDetected) { continue; } // Try a new random position

            // 3. Check against any other colliders tagged "Obstacle" nearby (for procedural ones - if no overlap yet)
            if (!overlapDetected) // This check was missing an !overlapDetected in my previous full script, adding it here
            {
                Collider[] hitColliders = Physics.OverlapSphere(potentialTargetPosition, proceduralObstacleCheckRadius, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore); 
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.gameObject != this.gameObject && hitCollider.CompareTag("Obstacle"))
                    {
                        overlapDetected = true;
                        break; 
                    }
                }
            }
            
            if (overlapDetected) { continue; } // Try a new random position

            // If we reach here, no overlaps were detected
            transform.localPosition = potentialTargetPosition;
            placedSuccessfully = true;
            break; 
        }

        if (!placedSuccessfully)
        {
            Debug.LogWarning($"TargetMovement: Could not find a clear spot for target after {maxPlacementAttempts} attempts! Placing randomly (MIGHT OVERLAP). Scene: {this.gameObject.scene.name}", this.gameObject);
            transform.localPosition = new Vector3(Random.Range(floorMinX, floorMaxX), targetYPosition, Random.Range(floorMinZ, floorMaxZ));
        }
    }
}