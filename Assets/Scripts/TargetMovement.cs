using UnityEngine;

public class TargetMovement : MonoBehaviour
{
    [Header("Movement Bounds")]
    public float floorMinX = -18f; // Minimum X position for the target
    public float floorMaxX = 18f;  // Maximum X position for the target
    public float floorMinZ = -18f; // Minimum Z position for the target
    public float floorMaxZ = 18f;  // Maximum Z position for the target
    public float targetYPosition = 0.5f; // The Y height at which the target should sit

    // This public method will be called by the Agent when it reaches the target
    public void MoveTargetToRandomPosition()
    {
        float randomX = Random.Range(floorMinX, floorMaxX);
        float randomZ = Random.Range(floorMinZ, floorMaxZ);
        transform.localPosition = new Vector3(randomX, targetYPosition, randomZ);
    }

    // Optional: You could also call this in Start() if you want the target
    // to be in a random position when the game first loads.
    void Start()
     {
         MoveTargetToRandomPosition();
     }
}