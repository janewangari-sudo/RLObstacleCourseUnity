using UnityEngine;

public class AgentController : MonoBehaviour
{
    public float moveSpeed = 10f; // Speed at which the agent moves
    public float turnSpeed = 150f; // Speed at which the agent turns (if you want turning)
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody not found on AgentController's GameObject!");
        }
    }

    // FixedUpdate is called at a fixed interval and is good for physics operations
    void FixedUpdate()
    {
        if (rb == null) return;

        // Get input from horizontal (A/D or Left/Right arrow keys) and vertical (W/S or Up/Down arrow keys) axes
        float horizontalInput = Input.GetAxis("Horizontal"); // For side-to-side or turning
        float verticalInput = Input.GetAxis("Vertical");   // For forward/backward

        // --- Option 1: Direct Force Application (More like the simple ball) ---
        Vector3 moveDirection = new Vector3(horizontalInput, 0f, verticalInput);
        rb.AddForce(moveDirection.normalized * moveSpeed);

        // --- Option 2: Tank-like turning and forward movement (If you prefer this control style) ---
        // Make sure to comment out Option 1 if you use this, and vice-versa.
        // Vector3 forwardForce = transform.forward * verticalInput * moveSpeed;
        // rb.AddForce(forwardForce);
        // float turn = horizontalInput * turnSpeed * Time.fixedDeltaTime;
        // Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        // rb.MoveRotation(rb.rotation * turnRotation);
    }
}