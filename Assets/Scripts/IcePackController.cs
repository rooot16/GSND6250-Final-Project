using UnityEngine;
using UnityEngine.InputSystem;

public class IcePackController : MonoBehaviour
{
    [Header("Inventory and Status")]
    public int icePackCount = 3;

    [Tooltip("How long (in seconds) one ice pack lasts when used.")]
    public float icePackDuration = 10f;

    [Tooltip("The remaining time for the currently active ice pack.")]
    [SerializeField]
    private float currentIcePackTimeLeft = 0f;

    private PlayerTemperature _tempController;
    private PlayerInput _playerInput;
    private InputAction _icePackAction;

    void Awake()
    {
        // Get references to required components on the same GameObject
        _tempController = GetComponent<PlayerTemperature>();
        _playerInput = GetComponent<PlayerInput>();

        // Find the "IcePack" action in the Input System
        _icePackAction = _playerInput.actions.FindAction("IcePack");
        if (_icePackAction == null)
        {
            Debug.LogError("IcePack action not found in PlayerInput Action Map!");
            enabled = false;
        }
    }

    void OnEnable()
    {
        // Subscribe to the press event for the IcePack action
        if (_icePackAction != null)
        {
            _icePackAction.performed += OnIcePackPressed;
        }
    }

    void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        if (_icePackAction != null)
        {
            _icePackAction.performed -= OnIcePackPressed;
        }
    }

    void Update()
    {
        // Check if an ice pack is currently active
        if (currentIcePackTimeLeft > 0f)
        {
            currentIcePackTimeLeft -= Time.deltaTime;

            // If time runs out, stop using the ice pack
            if (currentIcePackTimeLeft <= 0f)
            {
                EndIcePackUse();
            }
        }
    }

    private void OnIcePackPressed(InputAction.CallbackContext context)
    {
        // Only use a new one if:
        // 1. Currently have ice packs.
        // 2. Not already using one (currentIcePackTimeLeft <= 0f).
        if (icePackCount > 0 && currentIcePackTimeLeft <= 0f)
        {
            StartIcePackUse();
        }
        else if (currentIcePackTimeLeft > 0f)
        {

            Debug.Log("Ice pack is already active.");
        }
        else
        {
            Debug.Log("No ice packs left!");
        }
    }

    private void StartIcePackUse()
    {
        // 1. Update Inventory
        icePackCount--;

        // 2. Set Timer
        currentIcePackTimeLeft = icePackDuration;

        // 3. Update Temperature Controller State
        _tempController.isHoldingIcepack = true;

        Debug.Log("Ice pack activated. Remaining: " + icePackCount);
    }

    private void EndIcePackUse()
    {
        // Reset time just to be safe
        currentIcePackTimeLeft = 0f;

        // Update Temperature Controller State
        _tempController.isHoldingIcepack = false;

        Debug.Log("Ice pack effect finished.");
    }
}