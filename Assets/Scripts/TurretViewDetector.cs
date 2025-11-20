using UnityEngine;
using System.Collections;

public class TurretViewDetector : MonoBehaviour
{
    [Header("View Settings")]
    public float viewDistance = 15f;
    [Range(0, 180)]
    public float viewAngle = 45f;

    [Header("Detection Thresholds")]
    [Tooltip("Player temperature must be ABOVE this value to be detected.")]
    public float minDetectionTemperature = 36.5f;

    [Header("Target References")]
    [SerializeField] private Transform playerTarget;
    public LayerMask obstacleMask; 

    public LayerMask targetLayerMask = ~0;

    [Header("Visualization")]
    public MeshRenderer viewMeshRenderer;
    private ConeMeshGenerator meshGenerator;

    private PlayerTemperature playerTempScript;
    private Player playerScript;

    [Header("Detection Materials")]
    public Material defaultMaterial;
    public Material detectedMaterial;

    // 用于跟踪上一帧的状态
    private bool wasPlayerVisibleLastFrame = false;

    void Awake()
    {
        Debug.Log("TURRET: TurretViewDetector script started on " + gameObject.name);

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            Debug.Log("TURRET: Player object found successfully: " + playerObject.name);
            playerTarget = playerObject.transform;
            playerTempScript = playerObject.GetComponent<PlayerTemperature>();
            playerScript = playerObject.GetComponent<Player>();

            if (playerScript == null)
            {
                Debug.LogError("TURRET ERROR: Found Player Object, but could NOT find 'Player' script component on it! 'turretsSeeingMe' will NOT update.");
            }
            else
            {
                Debug.Log("TURRET: Successfully linked to Player script.");
            }
        }
        else
        {
            Debug.LogError("TURRET ERROR: Player object NOT found! Please check the Player Tag in Inspector.");
        }

        meshGenerator = GetComponentInChildren<ConeMeshGenerator>();
        if (viewMeshRenderer != null) viewMeshRenderer.enabled = false;
    }

    void Start()
    {
        UpdateViewMeshSize();

        if (viewMeshRenderer != null && defaultMaterial != null)
        {
            viewMeshRenderer.material = defaultMaterial;
        }
    }

    void Update()
    {
        // not find player
        if (playerTarget == null) return;

        bool isCurrentlyVisible = IsTargetVisible(playerTarget);

        if (isCurrentlyVisible != wasPlayerVisibleLastFrame)
        {
            Debug.Log($"TURRET LOG: Visibility Changed -> {isCurrentlyVisible}");

            if (playerScript != null)
            {
                Debug.Log("TURRET LOG: Calling playerScript.UpdateTurretVisibility...");
                playerScript.UpdateTurretVisibility(isCurrentlyVisible);
            }
            else
            {
                Debug.LogError("TURRET ERROR: Cannot notify Player because playerScript reference is NULL.");
            }

            wasPlayerVisibleLastFrame = isCurrentlyVisible;
        }

        CheckPlayerLogic();

        // 2. Zombies 
        CheckZombieLogic();

        // view + temprature
        bool isPlayerSeen = isCurrentlyVisible && IsPlayerHotEnough();

        if (viewMeshRenderer != null)
        {
            viewMeshRenderer.enabled = true;

            if (isPlayerSeen)
            {
                if (detectedMaterial != null && viewMeshRenderer.material != detectedMaterial)
                {
                    viewMeshRenderer.material = detectedMaterial;
                }
            }
            else
            {
                if (defaultMaterial != null && viewMeshRenderer.material != defaultMaterial)
                {
                    viewMeshRenderer.material = defaultMaterial;
                }
            }

            // 淡入淡出
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

        if (IsTargetVisible(playerTarget) && IsPlayerHotEnough())
        {
            if (playerScript != null)
            {
                playerScript.TriggerRespawnSequence();
            }
        }
    }

    private bool IsTargetVisible(Transform target)
    {
        if (target == null) return false;

        // 1. Target Center
        Vector3 targetCenter = target.position;
        Collider targetCollider = target.GetComponentInChildren<Collider>();
        if (targetCollider != null)
        {
            targetCenter = targetCollider.bounds.center;
        }
        else
        {
            targetCenter += Vector3.up * 1.5f;
        }

        Vector3 myPosition = transform.position;
        Vector3 targetDirection = (targetCenter - myPosition).normalized;
        float distanceToTarget = Vector3.Distance(myPosition, targetCenter);

        // 2. 距离
        if (distanceToTarget > viewDistance)
        {
            Debug.Log("FAIL: Player Too Far."); 
            return false;
        }

        // 3. 角度
        float angleToTarget = Vector3.Angle(transform.forward, targetDirection);
        if (angleToTarget > viewAngle)
        {
            Debug.Log("FAIL: Player Out of View Angle."); 
            return false;
        }

        // 4. Raycast obstacles
        RaycastHit hit;
         if (Physics.Raycast(myPosition, targetDirection, out hit, distanceToTarget, obstacleMask))
        {
            Debug.DrawLine(myPosition, hit.point, Color.red);

            if (hit.transform != target && !hit.transform.IsChildOf(target))
            {
                Debug.Log("FAIL: Blocked by " + hit.transform.name);
                return false;
            }
        }

        Debug.DrawLine(myPosition, targetCenter, Color.green);
        Debug.Log("SUCCESS: Player is visible and unblocked.");
        return true;
    }

    private void CheckZombieLogic()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, viewDistance, targetLayerMask);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Zombie"))
            {
                if (IsTargetVisible(hit.transform))
                {

                }
            }
        }
    }

    private bool IsPlayerHotEnough()
    {
        if (playerTempScript == null) return false;
        return playerTempScript.GetCurrentTemperature() > minDetectionTemperature;
    }

    private void UpdateViewMeshSize()
    {
        if (meshGenerator != null)
        {
            meshGenerator.height = viewDistance;
            float halfAngleRad = viewAngle * Mathf.Deg2Rad;
            meshGenerator.radius = viewDistance * Mathf.Tan(halfAngleRad);
            meshGenerator.GenerateCone();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (playerTarget == null) return;

        bool isDetected = CheckForPlayer() && IsPlayerHotEnough();
        Gizmos.color = isDetected ? Color.red : Color.yellow;

        Vector3 position = transform.position;

        Quaternion leftRayRotation = Quaternion.AngleAxis(-viewAngle, transform.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(viewAngle, transform.up);
        Vector3 leftRayDirection = leftRayRotation * transform.forward;
        Vector3 rightRayDirection = rightRayRotation * transform.forward;

        Gizmos.DrawRay(position, leftRayDirection * viewDistance);
        Gizmos.DrawRay(position, rightRayDirection * viewDistance);

        // Draw Arc
        DrawWireArc(position, transform.forward, viewDistance, viewAngle * 2);

        // Draw Line to Target
        if (isDetected)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(position, playerTarget.position);
        }
    }

    private bool CheckForPlayer()
    {
        if (playerTarget == null) return false;
        return IsTargetVisible(playerTarget);
    }

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