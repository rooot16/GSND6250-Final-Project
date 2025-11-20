using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour, Interaction.IInteractor
{
    // --- Movement and Speed Settings ---
    // The force/acceleration applied when moving. Higher value means quicker acceleration.
    public float accelerationFactor = 10f;

    // The actual speed limit when walking.
    public float walkSpeed = 2f;

    // The actual speed limit when sprinting.
    public float runSpeed = 4f;

    // Look sensitivity
    public float lookSensitivityX = 10f;
    public float lookSensitivityY = 10f;

    // --- Monitoring ---
    // This will show up in the Inspector and track our current horizontal speed.
    [SerializeField]
    private float currentHorizontalSpeed = 0f;

    // --- Public Status for PlayerTemperature ---
    public bool IsMoving { get; private set; } = false;
    public bool IsRunning { get; private set; } = false;

    // General settings
    public float interactiveRange = 5f;
    public bool enableJump = false;
    public bool enableCrouch = false;
    public bool enableProne = false;
    public bool isHidden = false;
    public float jumpForce = 5f;

    // respawn settings
    [Header("Respawn Settings")]
    public Transform startingPoint; // StartingPoint
    public Image blackScreenImage;  // black Image 

    private bool isInputLocked = false;

    // Private stuff for tracking state and inputs
    private PlayerInput playerInput;
    private Mouse mouse;
    private float yaw;
    private float pitch;

    private Rigidbody _rigidbody;

    private LayerMask interactableMask;
    private InputAction interactAction;
    private InputAction jumpAction;
    private InputAction crouchAction;
    private InputAction proneAction;
    private InputAction runAction;

    // Stance settings
    public float crouchHeightRatio = 0.5f;
    public float proneHeightRatio = 0.25f;
    private float playerHeight = 0f;
    private int stance = 0; // 0=Standing, 1=Crouching, 2=Prone
    private Vector3 originalScale;
    private Vector3 savedPosition;
    private Vector3 hideSpotPosition;

    public Camera auxiliarCamera;

    [SerializeField] GameObject eye;

    void Awake()
    {
        interactableMask = LayerMask.GetMask("Interactable", "Default");
    }

    void Start()
    {
        originalScale = transform.localScale;
        mouse = Mouse.current;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _rigidbody = GetComponent<Rigidbody>();

        playerInput = GetComponent<PlayerInput>();
        playerHeight = transform.Find("Body").gameObject.GetComponent<CapsuleCollider>().height;

        if (_rigidbody == null) throw new Exception("Rigidbody of Player not assigned!");
        if (playerInput == null) throw new Exception("Player Input not set!");

        interactAction = playerInput.actions.FindAction("Interact");
        jumpAction = playerInput.actions.FindAction("Jump");
        crouchAction = playerInput.actions.FindAction("Crouch");
        proneAction = playerInput.actions.FindAction("Prone");
        runAction = playerInput.actions.FindAction("Run");

        if (auxiliarCamera != null)
        {
            auxiliarCamera.enabled = false;
        }

        // Set black screen to transparent at start
        if (blackScreenImage != null)
        {
            Color c = blackScreenImage.color;
            c.a = 0f;
            blackScreenImage.color = c;
        }
    }

    void Update()
    {
        if (isInputLocked) return;

        if (isHidden)
        {
            if (interactAction.WasPressedThisFrame()) InteractWithObjects();
            return;
        }

        // Camera Look Control 
        float deltaX = mouse.delta.x.ReadValue() * lookSensitivityX * Time.deltaTime;
        float deltaY = mouse.delta.y.ReadValue() * lookSensitivityY * Time.deltaTime;

        yaw += deltaX;
        pitch -= deltaY;
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        eye.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // Actions
        if (interactAction.WasPressedThisFrame()) InteractWithObjects();
        if (jumpAction.WasPressedThisFrame()) jump();

        // Stance switching (Crouch and Prone)
        if (crouchAction.WasPressedThisFrame())
        {
            if (stance == 1) switchStance(0);
            else if (enableCrouch) switchStance(1);
        }

        if (proneAction.WasPressedThisFrame())
        {
            if (stance == 2) switchStance(0);
            else if (enableProne) switchStance(2);
        }
    }


    void FixedUpdate()
    {
        // if input is locked, do not move
        if (isInputLocked)
        {
            _rigidbody.linearVelocity = Vector3.zero; // 彻底停下
            return;
        }

        movePlayer();
    }

    // turret detected respawn sequence
    public void TriggerRespawnSequence()
    {
        if (!isInputLocked) 
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    private IEnumerator RespawnRoutine()
    {
        isInputLocked = true;
        Debug.Log("Player detected! Freezing...");

        // 1. Player isInputLocked

        // 2. wait for 2 seconds
        yield return new WaitForSeconds(2f);

        // 3. turn screen black in 1 second
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime;
            if (blackScreenImage != null)
            {
                Color c = blackScreenImage.color;
                c.a = Mathf.Lerp(0f, 1f, timer / 1f);
                blackScreenImage.color = c;
            }
            yield return null;
        }

        if (blackScreenImage != null)
        {
            Color c = blackScreenImage.color;
            c.a = 1f;
            blackScreenImage.color = c;
        }

        // 4. black screen 保持 2 秒
        yield return new WaitForSeconds(2f);

        // 5.teleport Player
        if (startingPoint != null)
        {
            transform.position = startingPoint.position;
            _rigidbody.linearVelocity = Vector3.zero;
            transform.rotation = startingPoint.rotation;
        }
        else
        {
            Debug.LogError("Starting Point not assigned in Player script!");
        }

        // 6. turn screen back to normal in 0.5 seconds
        timer = 0f;
        while (timer < 0.5f)
        {
            timer += Time.deltaTime;
            if (blackScreenImage != null)
            {
                Color c = blackScreenImage.color;
                c.a = Mathf.Lerp(1f, 0f, timer / 0.5f);
                blackScreenImage.color = c;
            }
            yield return null;
        }

        // unlock input
        isInputLocked = false;
        Debug.Log("Respawn complete.");
    }

    private void movePlayer()
    {
        // --- Determine Current Speed Limits and Acceleration ---
        IsRunning = runAction != null && runAction.IsPressed(); // 更新 IsRunning 属性

        float currentMaxSpeed = IsRunning ? runSpeed : walkSpeed;
        float currentAcceleration = accelerationFactor;

        // --- Movement Application ---
        Vector2 direction = playerInput.actions["Move"].ReadValue<Vector2>().normalized;
        IsMoving = direction.magnitude > 0.1f; // 更新 IsMoving 属性

        Vector3 acceleration = transform.right * direction.x * currentAcceleration + transform.forward * direction.y * currentAcceleration;

        _rigidbody.AddForce(acceleration, ForceMode.Acceleration);

        // Get the horizontal velocity (X and Z components) without the Y component
        Vector3 horizontalVelocity = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z);
        currentHorizontalSpeed = horizontalVelocity.magnitude; // Monitoring current speed

        // Cap the horizontal speed at the max speed
        if (currentHorizontalSpeed > currentMaxSpeed)
        {
            // Calculate the desired capped horizontal velocity vector
            horizontalVelocity = horizontalVelocity.normalized * currentMaxSpeed;

            // Apply the capped horizontal velocity while preserving the vertical (Y) velocity
            _rigidbody.linearVelocity = new Vector3(horizontalVelocity.x, _rigidbody.linearVelocity.y, horizontalVelocity.z);
        }
    }

    private void InteractWithObjects()
    {
        RaycastHit hit;
        if (Physics.Raycast(eye.transform.position, eye.transform.TransformDirection(Vector3.forward), out hit, interactiveRange, interactableMask, QueryTriggerInteraction.Ignore))
        {

            if (hit.collider.gameObject.tag == "HideSpot")
            {
                if (isHidden == false)
                {
                    savedPosition = transform.position;
                    hideSpotPosition = hit.collider.gameObject.transform.position;
                    isHidden = true;
                    Debug.Log("isHidden: " + isHidden);

                    Camera mainCamera = GetComponentInChildren<Camera>();
                    auxiliarCamera.gameObject.SetActive(true);
                    auxiliarCamera.enabled = true;
                    mainCamera.enabled = false;

                    auxiliarCamera.GetComponent<AuxiliarCamera>().SetTarget(hideSpotPosition);
                }
                else if (isHidden == true)
                {
                    isHidden = false;
                    Debug.Log("isHidden: " + isHidden);
                    Camera mainCamera = GetComponentInChildren<Camera>();
                    mainCamera.enabled = true;
                    auxiliarCamera.gameObject.SetActive(false);
                    auxiliarCamera.enabled = false;
                }
            }

            if (hit.rigidbody)
            {
                Interaction.IInteractable target = hit.rigidbody.gameObject.GetComponent<Interaction.IInteractable>();
                ((Interaction.IInteractor)this).Interact((object)target);
            }
        }
    }

    private void jump()
    {
        if (enableJump)
        {
            if (isGrounded())
            {
                _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }
    }

    private bool isGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 0.5f);
    }

    private void switchStance(int st)
    {

        float targetScale = 1f;

        if (st == 0)
        {
            targetScale = 1f;
        }
        else if (st == 1)
        {
            targetScale = crouchHeightRatio;
        }
        else if (st == 2)
        {
            targetScale = proneHeightRatio;
        }

        float oldScale = transform.localScale.y;
        float prevPosY = transform.position.y;

        _rigidbody.isKinematic = true;

        float scaleDifference = (originalScale.y * targetScale - oldScale) * playerHeight;
        transform.position = new Vector3(transform.position.x, prevPosY + scaleDifference * 0.5f, transform.position.z);

        transform.localScale = new Vector3(originalScale.x, originalScale.y * targetScale, originalScale.z);

        _rigidbody.isKinematic = false;

        stance = st;
    }

    public void LateUpdate()
    {
        if (isHidden)
        {
            auxiliarCamera.transform.position = Vector3.Lerp(transform.position, hideSpotPosition, 5 * Time.deltaTime);
        }

    }
}