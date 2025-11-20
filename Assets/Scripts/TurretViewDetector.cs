using UnityEngine;

public class TurretViewDetector : MonoBehaviour
{
    [Header("View Settings")]
    public float viewDistance = 15f;
    [Range(0, 180)]
    public float viewAngle = 45f;

    [Header("Detection Thresholds")]
    [Tooltip("Player temperature must be ABOVE this value to be detected.")]
    public float minDetectionTemperature = 36.5f; // lower threshold for detection

    [Header("Target References")]
    [SerializeField] private Transform playerTarget;
    public LayerMask obstacleMask;

    // zombie detection layer
    public LayerMask targetLayerMask = ~0; // default to everything

    [Header("Visualization")]
    public MeshRenderer viewMeshRenderer;
    private ConeMeshGenerator meshGenerator;

    private PlayerTemperature playerTempScript;
    private Player playerScript;

    [Header("Detection Materials")]
    [Tooltip("The material to use when the player is NOT detected.")]
    public Material defaultMaterial;
    [Tooltip("The material to use when the player IS detected.")]
    public Material detectedMaterial;


    void Awake()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTarget = playerObject.transform;
            playerTempScript = playerObject.GetComponent<PlayerTemperature>();
            playerScript = playerObject.GetComponent<Player>();
        }

        meshGenerator = GetComponentInChildren<ConeMeshGenerator>();
        if (viewMeshRenderer != null) viewMeshRenderer.enabled = false;
    }

    void Start()
    {
        UpdateViewMeshSize();

        // 初始化材质
        if (viewMeshRenderer != null && defaultMaterial != null)
        {
            viewMeshRenderer.material = defaultMaterial;
        }
    }

    void Update()
    {
        // 1. Player
        CheckPlayerLogic();

        // 2. Zombies
        CheckZombieLogic();

        // 可视化网格颜色控制
        bool isPlayerSeen = IsTargetVisible(playerTarget) && IsPlayerHotEnough();

        if (viewMeshRenderer != null)
        {
            viewMeshRenderer.enabled = true;

            if (isPlayerSeen)
            {
                // 如果检测到 Player
                if (detectedMaterial != null && viewMeshRenderer.material != detectedMaterial)
                {
                    viewMeshRenderer.material = detectedMaterial;
                }
            }
            else
            {
                // 如果未检测到 Player
                if (defaultMaterial != null && viewMeshRenderer.material != defaultMaterial)
                {
                    viewMeshRenderer.material = defaultMaterial;
                }
            }

            Color targetColor = isPlayerSeen ? Color.red : new Color(1f, 1f, 0f, 0.2f);
            if (viewMeshRenderer.material != null && viewMeshRenderer.material.HasProperty("_Color"))
            {
                viewMeshRenderer.material.color = Color.Lerp(viewMeshRenderer.material.color, targetColor, Time.deltaTime * 5f);
            }
        }
    }

    private void CheckPlayerLogic()
    {
        if (playerTarget == null) return;

        if (IsTargetVisible(playerTarget))
        {
            if (IsPlayerHotEnough())
            {
                // activate respawn sequence
                if (playerScript != null)
                {
                    playerScript.TriggerRespawnSequence();
                }
            }
        }
    }

    private void CheckZombieLogic()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, viewDistance, targetLayerMask);

        foreach (var hit in hits)
        {
            // 3. zombie
            if (hit.CompareTag("Zombie"))
            {
                // in detection range, now check visibility
                if (IsTargetVisible(hit.transform))
                {
                    ZombieMono zombie = hit.GetComponent<ZombieMono>();
                    if (zombie != null)
                    {
                        zombie.FreezeAndDie();
                    }
                }
            }
        }
    }

    private bool IsPlayerHotEnough()
    {
        if (playerTempScript == null) return false;
        return playerTempScript.GetCurrentTemperature() > minDetectionTemperature;
    }

    // determine if a target is visible based on distance, angle, and obstacles
    private bool IsTargetVisible(Transform target)
    {
        if (target == null) return false;

        Vector3 targetPosition = target.position;
        Vector3 myPosition = transform.position;
        Vector3 targetDirection = (targetPosition - myPosition).normalized;
        float distanceToTarget = Vector3.Distance(myPosition, targetPosition);

        if (distanceToTarget > viewDistance) return false;


        float angleToTarget = Vector3.Angle(transform.forward, targetDirection);
        if (angleToTarget > viewAngle) return false;

        // cover obstacles
        RaycastHit hit;
        if (Physics.Raycast(myPosition, targetDirection, out hit, distanceToTarget * 0.99f, obstacleMask))
        {
            if (hit.transform != target)
            {
                return false;
            }
        }

        return true;
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

    // This function is kept for completeness, assuming CheckPlayerLogic is the main flow.
    private bool CheckForPlayer()
    {
        if (playerTarget == null) return false;
        return IsTargetVisible(playerTarget);
    }

    // Gizmos for visualization
    private void OnDrawGizmosSelected()
    {
        if (playerTarget == null) return;

        bool isDetected = IsTargetVisible(playerTarget) && IsPlayerHotEnough();
        Gizmos.color = isDetected ? Color.red : Color.yellow;

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
        if (isDetected)
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