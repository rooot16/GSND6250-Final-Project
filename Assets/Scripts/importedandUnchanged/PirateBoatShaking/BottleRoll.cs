using UnityEngine;

public class BottleRoll : MonoBehaviour
{
    // Reference to the boat shaking script for time synchronization
    public BoatShaking timeSource;
    // Amplitude of the bottle's translational movement on the X-axis
    public float positionAmplitude = 0.05f;
    // Factor to convert X-axis movement distance to Y-axis rotation angle
    public float rollFactor = 300f;

    // Independent time scale to slow down the bottle's movement (0.0f to 1.0f)
    public float bottleTimeScale = 0.5f;

    private float xOffsetStart;
    private Vector3 originalLocalPosition;

    // Tracks the scaled time from the last frame
    private float lastBottleTime;

    // Saves the X position from the last frame to calculate displacement
    private float lastLocalXPosition;

    void Awake()
    {
        if (timeSource == null)
        {
            Debug.LogError("BottleRoll: Missing BoatShaking reference!");
            enabled = false;
            return;
        }

        originalLocalPosition = transform.localPosition;
        xOffsetStart = Random.Range(0f, 100f);

        lastLocalXPosition = transform.localPosition.x;
        lastBottleTime = timeSource.CurrentTimeT * bottleTimeScale;
    }

    void LateUpdate()
    {
        if (timeSource == null) return;

        // Use the original time for the Perlin noise input to maintain the correct movement cycle
        float timeForNoise = timeSource.CurrentTimeT;

        // Calculate scaled time for smoothed movement speed
        float scaledTime = timeForNoise * bottleTimeScale;
        float deltaTimeScaled = scaledTime - lastBottleTime;
        lastBottleTime = scaledTime;

        // --- 1. X-axis Translation (Driven by Perlin Noise) ---

        // Calculate the target X position based on Perlin noise
        float noiseX = (Mathf.PerlinNoise(xOffsetStart + timeForNoise, 0f) * 2f - 1f);
        float targetLocalXPosition = originalLocalPosition.x + noiseX * positionAmplitude;

        // Smoothly move towards the target position. Slower deltaTimeScaled results in slower movement.
        float newLocalXPosition = Mathf.Lerp(lastLocalXPosition, targetLocalXPosition, 10f * deltaTimeScaled);

        // Calculate the distance moved this frame (delta distance)
        float deltaXPosition = newLocalXPosition - lastLocalXPosition;

        // Apply position (Translation only on X-axis)
        Vector3 newPosition = new Vector3(newLocalXPosition, originalLocalPosition.y, originalLocalPosition.z);
        transform.localPosition = newPosition;

        // Update last position
        lastLocalXPosition = newLocalXPosition;

        // --- 2. Y-axis Rotation (Driven by Translation Distance) ---

        // Rotation angle is proportional to the translation distance (deltaXPosition).
        // Removed the negative sign for correct rolling direction (or kept positive for Y-axis rotation).
        float rotateAngle = deltaXPosition * rollFactor;

        // Rotate around the object's local Y-axis
        transform.Rotate(0f, rotateAngle, 0f, Space.Self);
    }
}