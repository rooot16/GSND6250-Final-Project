using UnityEngine;

public class TurretSpin : MonoBehaviour
{
    // How fast this turret spins (in degrees per second).
    public float rotationSpeed = 60f;

    void Update()
    {
        // Get the time that passed since the last frame.
        float deltaTime = Time.deltaTime;

        // Figure out how much we need to turn this frame.
        float angleToRotate = rotationSpeed * deltaTime;

        // Spin around the turret's Y-axis (transform.up).
        transform.Rotate(transform.up, angleToRotate, Space.Self);
    }
}