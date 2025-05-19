using UnityEngine;

public class AgentCameraFollow : MonoBehaviour
{
    public Transform agentTransform; // Assign your AgentSphere here in the Inspector
    public Vector3 cameraOffset;     // The offset from the agent to the camera

    // You can set the offset in the Inspector, or calculate it once in Start
    public bool calculateOffsetOnStart = true; 

    void Start()
    {
        if (agentTransform == null)
        {
            Debug.LogError("Agent Transform not assigned to AgentCameraFollow script!", this);
            return;
        }

        if (calculateOffsetOnStart)
        {
            cameraOffset = transform.position - agentTransform.position;
        }
    }

    // LateUpdate is called after all Update functions have been called.
    // This is good for camera updates to ensure the agent has finished its move for the frame.
    void LateUpdate()
    {
        if (agentTransform == null)
        {
            return; // Don't do anything if the agent isn't assigned
        }

        // Update the camera's position to be the agent's position plus the offset
        transform.position = agentTransform.position + cameraOffset;

        // Optional: Make the camera always look at the agent (if it's not a child of the agent)
        transform.LookAt(agentTransform); 
        // Be careful with LookAt if your offset is meant to give a specific viewing angle,
        // as LookAt will override the camera's rotation.
        // For a simple follow from a fixed offset angle, just setting the position is often enough.
    }
}