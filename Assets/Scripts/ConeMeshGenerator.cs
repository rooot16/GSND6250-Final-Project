using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ConeMeshGenerator : MonoBehaviour
{
    // Public values that TurretViewDetector uses to set the size of the mesh.
    [HideInInspector] public float radius = 1f;
    [HideInInspector] public float height = 1f;

    [Tooltip("How many sides the cone has. More segments mean a smoother cone, but it costs more performance.")]
    public int segments = 18;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;

    void Awake()
    {
        // Automatically grab the MeshFilter and create a new Mesh instance.
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // Generate the cone mesh for the first time.
        GenerateCone();

        // The cone is built to stretch along the local Z-axis (forward). 
        // We assume the cone's local rotation should be (0, 0, 0) to align with the Turret's forward direction.
        transform.localRotation = Quaternion.Euler(0, 0, 0);
    }

    // Call this function whenever you need to update the cone's size/shape in real-time.
    public void GenerateCone()
    {
        // Vertex count: Tip (1) + Side Base Points (segments) + Side Copies (segments) + Bottom Center (1).
        int vertCount = segments * 2 + 2;

        // Triangle count: Sides (segments * 3) + Base (segments * 3).
        int triCount = segments * 6;

        vertices = new Vector3[vertCount];
        triangles = new int[triCount];
        uvs = new Vector2[vertCount];

        // 1. The tip of the cone is at the local origin (0, 0, 0).
        vertices[0] = Vector3.zero;
        uvs[0] = new Vector2(0.5f, 1f);

        // 2. Set up the points around the base (at Z = height).
        for (int i = 0; i < segments; i++)
        {
            float angle = 2f * Mathf.PI * i / segments;
            float x = radius * Mathf.Cos(angle);
            float y = radius * Mathf.Sin(angle);

            // Position the point: radius in X-Y plane, stretching along local Z-axis.

            // Side base vertex (index 1 to segments)
            vertices[i + 1] = new Vector3(x, y, height);
            uvs[i + 1] = new Vector2((float)i / segments, 0f);

            // Side copy vertex (used for proper UV/Normals, though kept simple here)
            vertices[i + segments + 1] = new Vector3(x, y, height);
            uvs[i + segments + 1] = new Vector2((float)i / segments, 0f);
        }

        // 3. The center point of the base.
        int centerIndex = 2 * segments + 1;
        vertices[centerIndex] = new Vector3(0, 0, height);
        uvs[centerIndex] = new Vector2(0.5f, 0f);

        // 4. Draw the triangles.
        int vert = 1; // Start counting base points from index 1.
        int tri = 0;

        for (int i = 0; i < segments; i++)
        {
            int nextVert = (i + 1) % segments;

            // A. Draw the side triangles.
            // Vertices: (Tip, Current Base Point, Next Base Point) -> Normals face outwards.
            triangles[tri + 0] = 0;              // The Tip (Index 0)
            triangles[tri + 1] = vert + i;       // Current base point
            triangles[tri + 2] = vert + nextVert; // Next base point
            tri += 3;

            // B. Draw the base/cap triangles.
            int currentEdgeIndex = vert + i;
            int nextEdgeIndex = vert + (i + 1) % segments;

            // Vertices: (Center, Next Base Point, Current Base Point) to make normals face towards the tip (outwards).
            triangles[tri + 0] = centerIndex;   // Base center point
            triangles[tri + 1] = nextEdgeIndex;  // Next base point (Flipped order for desired normal direction)
            triangles[tri + 2] = currentEdgeIndex; // Current base point
            tri += 3;
        }

        // 5. Apply the mesh data.
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        // Gotta recalculate these after setting vertices/triangles for correct lighting/culling.
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}