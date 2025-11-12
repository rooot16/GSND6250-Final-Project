using UnityEngine;

public class TurretSpin : MonoBehaviour
{
    // How fast it spins (degrees/second).
    public float rotationSpeed = 60f;

    void Update()
    {
        // Get the time passed this frame.
        float deltaTime = Time.deltaTime;

        // Calculate the angle to turn.
        float angleToRotate = rotationSpeed * deltaTime;

        // Spin around our own Y-axis (transform.up).
        transform.Rotate(transform.up, angleToRotate, Space.Self);
    }
}