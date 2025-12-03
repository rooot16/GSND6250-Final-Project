using UnityEngine;
using UnityEngine.Audio;

/*
Temperature values:
1. Boundaries
Min Temperature With Ice Pack: 34 (The absolute lowest limit achievable with an ice pack)
Min Temperature: 36 (Normal base body temperature)
Max Temperature: 38 (Maximum upper limit)

2. Above Normal Baseline (36-38 degrees)
Standard Run Rate: 0.2 (Rate of temperature rise while running in normal zone)
Standard Move Rate: 0 (Rate of temperature rise while walking in normal zone)
Standard Stand Rate: -0.1 (Rate of cooling while standing still in normal zone)

3. below  Normal Baseline (34-36 degrees)
Cold Run Rate: 0.35 (Running in cold; body generates heat faster to resist the cold)
Cold Move Rate: 0.15 (Walking in cold; movement starts generating heat now)
Cold Stand Rate: 0.05 (Standing in cold; body slowly recovers heat back to 36 degrees)

4. Ice Path Effect
Ice Pack Cooling Power: 0.2 (The strength of the ice pack; this value is subtracted directly from all the rates above)
*/

public class PlayerTemperature : MonoBehaviour, IResettable
{
    // --- Inspector Settings ---

    [Header("Current Status")]
    [SerializeField]
    private float currentTemperature = 36f;

    [Header("Temperature Boundaries")]
    public float minTemperatureWithIcePack = 34f; // Baseline with With ice.
    public float minTemperature = 36f;            // Normal baseline.
    public float maxTemperature = 38f;            // Up limit.
    public bool isHoldingIcepack = false;

    [Header("Standard Zone Rates (36-38 degrees)")]
    public float standardRunRate = 0.2f;
    public float standardMoveRate = 0f;
    public float standardStandRate = -0.1f;

    [Header("Cold Zone Rates (34-36 degrees)")]
    public float coldRunRate = 0.35f;
    public float coldMoveRate = 0.15f;
    public float coldStandRate = 0.05f;

    [Header("Icepack Power")]
    public float icePackCoolingPower = 0.2f;


    private Player _player;

    void Awake()
    {
        _player = GetComponent<Player>();
        currentTemperature = minTemperature;
    }

    void FixedUpdate()
    {
        updateTemperature();
    }

    private void updateTemperature()
    {
        bool isMoving = _player.IsMoving;
        bool isRunning = _player.IsRunning;
        float finalChangeRate = 0f;

        // Step 1: Passive Recovery (Standing still, no ice pack).
        // logic: slowly drift back to 36.0 without jittering.
        if (!isHoldingIcepack && !isMoving && !isRunning)
        {
            float recoverySpeed = (currentTemperature < 36f) ? coldStandRate : Mathf.Abs(standardStandRate);

            // MoveTowards prevents the value from overshooting 36.
            currentTemperature = Mathf.MoveTowards(currentTemperature, minTemperature, recoverySpeed * Time.fixedDeltaTime);
            return;
        }

        // Step 2: Active State (Moving, Running, or holding Ice Pack).

        // A. Pick the base rate based on current temp zone.
        if (currentTemperature < 36f)
        {
            // Cold Zone Logic (34-36)
            if (isRunning) finalChangeRate = coldRunRate;
            else if (isMoving) finalChangeRate = coldMoveRate;
            else finalChangeRate = coldStandRate; // (Ice pack case)
        }
        else
        {
            // Standard Zone Logic (36-38)
            if (isRunning) finalChangeRate = standardRunRate;
            else if (isMoving) finalChangeRate = standardMoveRate;
            else finalChangeRate = standardStandRate;
        }

        // B. Apply Ice Pack cooling.
        if (isHoldingIcepack)
        {
            finalChangeRate -= icePackCoolingPower;
        }

        // C. Apply the change.
        currentTemperature += finalChangeRate * Time.fixedDeltaTime;



        // Step 3: Clamp limits.
        // If holding ice pack, floor is 34. Otherwise, physics technically allows 34, but logic pushes to 36.
        float absoluteFloor = minTemperatureWithIcePack;
        currentTemperature = Mathf.Clamp(currentTemperature, absoluteFloor, maxTemperature);
    }

    public float GetCurrentTemperature()
    {
        return currentTemperature;
    }

    public void OnReset()
    {
        currentTemperature = minTemperature;
    }
}