using UnityEngine;

public class PlayerTemperature : MonoBehaviour, IResettable
{
    // --- Inspector Settings ---
    [Header("Temperature Monitoring")]
    [SerializeField]
    private float currentTemperature = 36f;

    [Header("Boundary and State")]
    public float minTemperatureWithIcePack = 34f;
    public float minTemperature = 36f;
    public float maxTemperature = 38f;
    public bool isHoldingIcepack = false;

    [Header("Temperature Change Rates (Per Second)")]
    public float tempChangeStandStill = -0.1f; 
    public float tempGainMoving = 0f;
    public float tempGainRunning = 0.2f;

    [Header("Icepack Modifier")]
    public float tempChangeIcepackModifier = -0.2f; 

    private Player _player; // get the Player script here to check if they're moving/running.

    void Awake()
    {
        _player = GetComponent<Player>();

        // Start the player at the minimum healthy temp.
        currentTemperature = minTemperature;
    }

    void FixedUpdate()
    {
        updateTemperature();
    }

    private void updateTemperature()
    {
        float baseChangePerSecond = 0f;
        bool isMoving = _player.IsMoving;
        bool isRunning = _player.IsRunning;

        // 1. Calculate the BASE rate (without ice pack).
        if (!isMoving)
        {
            baseChangePerSecond = tempChangeStandStill;
        }
        else if (isRunning)
        {
            baseChangePerSecond = tempGainRunning;
        }
        else
        {
            baseChangePerSecond = tempGainMoving;
        }

        // 2. Apply the Ice Pack correction (simple addition/subtraction).
        float finalChangePerSecond = baseChangePerSecond;

        if (isHoldingIcepack)
        {
            // Apply the fixed modifier to the base rate
            finalChangePerSecond += tempChangeIcepackModifier;
        }

        // 3. Apply the change and make sure it stays between min and max.
        currentTemperature += finalChangePerSecond * Time.fixedDeltaTime;
 
        if (isHoldingIcepack)
        {
            currentTemperature = Mathf.Clamp(currentTemperature, minTemperatureWithIcePack, maxTemperature);
        }
        else
        {
            currentTemperature = Mathf.Clamp(currentTemperature, minTemperature, maxTemperature);
        }

    }

    // for Zombies scripts to check the current temperature.
    public float GetCurrentTemperature()
    {
        return currentTemperature;
    }

    public void OnReset() {
        currentTemperature = minTemperature;
    }
}