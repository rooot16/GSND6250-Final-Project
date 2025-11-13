using UnityEngine;

public class TurretViewDetector : MonoBehaviour
{
    [Header("View Settings")]
    [Tooltip("The max distance the turret can see.")]
    public float viewDistance = 15f;

    [Tooltip("The half-angle of the cone. E.g., 45 degrees means a total view angle of 90 degrees.")]
    [Range(0, 180)]
    public float viewAngle = 45f;

    [Header("Target and Obstacles")]
    [SerializeField]
    private Transform playerTarget;

    [Tooltip("Layers the raycast should hit to check for walls and obstacles.")]
    public LayerMask obstacleMask;

    private bool isPlayerVisible = false;

    [Header("Visualization")]
    [Tooltip("The Mesh Renderer component on the ViewConeMesh child object.")]
    public MeshRenderer viewMeshRenderer;

    private ConeMeshGenerator meshGenerator;


    void Awake()
    {
        // Try to find the player with the "Player" tag
        GameObject playerObject = GameObject.FindWithTag("Player");

        if (playerObject != null)
        {
            playerTarget = playerObject.transform;
        }
        else
        {
            Debug.LogError("Error: Can't find an object with the 'Player' tag! Make sure your player has the correct tag.");
        }

        // Get the ConeMeshGenerator component from a child object
        meshGenerator = GetComponentInChildren<ConeMeshGenerator>();
        if (meshGenerator == null)
        {
            Debug.LogError("Error: ConeMeshGenerator component not found on a child object!");
        }

        // Set the mesh renderer state.
        if (viewMeshRenderer != null)
        {
            viewMeshRenderer.enabled = false; // Keep the mesh hidden by default.
        }
    }

    void Start()
    {
        // Set the mesh size to match our view settings.
        UpdateViewMeshSize();
        if (viewMeshRenderer != null)
        {
            // Create a unique material instance so changing the color here only affects this turret.
            viewMeshRenderer.material = new Material(viewMeshRenderer.material);
        }
    }

    void Update()
    {
        if (playerTarget == null)
        {
            return;
        }

        isPlayerVisible = CheckForPlayer();

        // Control the mesh visibility
        if (viewMeshRenderer != null)
        {
            viewMeshRenderer.enabled = true; // Always show the view mesh while active.

            // Change the mesh color based on detection status (Red if seen, Yellow if not).
            Color targetColor = isPlayerVisible ? Color.red : new Color(1f, 1f, 0f, 0.2f); // Yellow and transparent
            viewMeshRenderer.material.color = Color.Lerp(viewMeshRenderer.material.color, targetColor, Time.deltaTime * 5f);
        }
    }

    /// Updates the mesh's size and shape based on the script's view settings.
    private void UpdateViewMeshSize()
    {
        if (meshGenerator != null)
        {
            meshGenerator.height = viewDistance;

            // Calculate radius: radius = height * tan(viewAngle)
            float halfAngleRad = viewAngle * Mathf.Deg2Rad;
            meshGenerator.radius = viewDistance * Mathf.Tan(halfAngleRad);

            meshGenerator.GenerateCone();
        }
    }

    // The main function: checks if the player is in range, in view, and not blocked.
    private bool CheckForPlayer()
    {
        if (playerTarget == null) return false;

        Vector3 targetPosition = playerTarget.position;
        Vector3 myPosition = transform.position;
        Vector3 targetDirection = (targetPosition - myPosition).normalized;
        float distanceToTarget = Vector3.Distance(myPosition, targetPosition);

        // 1. Check the distance first.
        if (distanceToTarget > viewDistance) return false;

        // 2. Check the angle (Are they inside the cone?).
        float angleToTarget = Vector3.Angle(transform.forward, targetDirection);
        if (angleToTarget > viewAngle) return false;

        // 3. Check for obstacles blocking the view (Raycast).
        RaycastHit hit;
        // Raycast from the turret to the target, only checking the obstacle layers.
        if (Physics.Raycast(myPosition, targetDirection, out hit, distanceToTarget, obstacleMask))
        {
            // If the ray hits something before the target (since we only check ObstacleMask),
            // it means the target is blocked.
            if (hit.collider.gameObject != playerTarget.gameObject)
            {
                return false; // View is blocked by an obstacle.
            }
        }

        return true; // The player is visible!
    }

        // Gizmos for visualization
    private void OnDrawGizmosSelected()
    {
        if (playerTarget == null) return;

        Gizmos.color = isPlayerVisible ? Color.red : Color.yellow;
        Vector3 position = transform.position;

        // 1. Draw the edge lines of the view cone.
        Quaternion leftRayRotation = Quaternion.AngleAxis(-viewAngle, transform.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(viewAngle, transform.up);

        Vector3 leftRayDirection = leftRayRotation * transform.forward;
        Vector3 rightRayDirection = rightRayRotation * transform.forward;

        Gizmos.DrawRay(position, leftRayDirection * viewDistance);
        Gizmos.DrawRay(position, rightRayDirection * viewDistance);

        // 2. Draw the arc at the far edge of the view cone.
        DrawWireArc(position, transform.forward, viewDistance, viewAngle * 2);

        // 3. Draw a line to the target if they are seen.
        if (isPlayerVisible)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(position, playerTarget.position);
        }
        else
        {
            Gizmos.color = Color.yellow;
        }
    }

    // A helper function to draw a wire arc for the Gizmos.
    private void DrawWireArc(Vector3 center, Vector3 normal, float radius, float angle)
    {
        Vector3 forward = transform.forward;
        Quaternion rotation = Quaternion.LookRotation(forward, normal);
        float halfAngleRad = angle * 0.5f * Mathf.Deg2Rad;
        Vector3 prevPoint = center + (rotation * new Vector3(Mathf.Sin(-halfAngleRad), 0, Mathf.Cos(-halfAngleRad)) * radius);
        int segments = 20;

        for (int i = 1; i <= segments; i++)
        {
            float currentAngle = Mathf.Lerp(-halfAngleRad, halfAngleRad, (float)i / segments);
            Vector3 currentPoint = center + (rotation * new Vector3(Mathf.Sin(currentAngle), 0, Mathf.Cos(currentAngle)) * radius);
            Gizmos.DrawLine(prevPoint, currentPoint);
            prevPoint = currentPoint;
        }
    }
}