using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TurretRangeDisplay : MonoBehaviour
{
    public float rangeRadius = 5f;
    public float viewAngle = 80f;
    // How many slices the fan has (More = smoother).
    public int segments = 20;

    private Mesh mesh;
    private MeshFilter meshFilter;

    void Start()
    {
        // Get the MeshFilter component.
        meshFilter = GetComponent<MeshFilter>();

        // Create a new mesh for the fan shape.
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        // Build the fan shape right away.
        CreateFanMesh();
    }

    // This function builds the fan mesh geometry.
    void CreateFanMesh()
    {
        // Fan's center point is our local origin (0,0,0).
        Vector3 origin = Vector3.zero;

        // Vertices needed: Center + all edge points (segments + 2 total).
        Vector3[] vertices = new Vector3[segments + 2];
        vertices[0] = origin; // First point is the center.

        // Angle change per slice.
        float angleStep = viewAngle / segments;
        // Start angle is half the view angle to the left.
        float currentAngle = -viewAngle / 2;

        for (int i = 0; i <= segments; i++)
        {
            // Convert angle to radians for math.
            float radians = currentAngle * Mathf.Deg2Rad;

            // Calculate X and Z positions on the circle's edge.
            float x = rangeRadius * Mathf.Sin(radians);
            float z = rangeRadius * Mathf.Cos(radians);

            // Store the edge point (Y is 0 for flatness).
            vertices[i + 1] = new Vector3(x, 0f, z);

            // Move to the next angle.
            currentAngle += angleStep;
        }

        // We need 3 indices for each segment/triangle.
        int[] triangles = new int[segments * 3];

        for (int i = 0; i < segments; i++)
        {
            int triangleIndex = i * 3;

            // Every triangle is: Center (0) -> Edge point i -> Edge point i+1.
            triangles[triangleIndex] = 0;
            triangles[triangleIndex + 1] = i + 1;
            triangles[triangleIndex + 2] = i + 2;
        }

        // Apply the new geometry data to the mesh.
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        // Fix lighting and boundaries.
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}