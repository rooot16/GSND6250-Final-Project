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
    public float accelerationFactor = 10f;
    public float walkSpeed = 2f;
    public float runSpeed = 4f;

    // Look sensitivity
    public float lookSensitivityX = 10f;
    public float lookSensitivityY = 10f;

    // --- Monitoring ---
    [SerializeField]
    private float currentHorizontalSpeed = 0f;

    // --- Public Status for PlayerTemperature ---
    public bool IsMoving { get; private set; } = false;
    public bool IsRunning { get; private set; } = false;

    // General settings
    [Header("Interaction Settings")]
    public float interactiveRange = 2f; 
    public LayerMask interactableMask; 

    public bool enableJump = false;
    public bool enableCrouch = false;
    public bool enableProne = false;
    public bool isHidden = false;
    public float jumpForce = 5f;
    public float currentTemperature;

    // respawn settings
    [Header("Respawn Settings")]
    public Transform startingPoint; 
    public Image blackScreenImage;   

    [Header("Turret Detection Status")]
    [SerializeField]
    private int turretsSeeingMe = 0;

    [Header("Audio Components")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfx_dead;
    [SerializeField] private AudioSource audioSource_agitated;
    [SerializeField] private AudioSource audioSource_heartbeat;

    public bool IsVisibleToTurret
    {
        get { return turretsSeeingMe > 0; }
    }

    public void UpdateTurretVisibility(bool isNowVisible)
    {
        if (isNowVisible) turretsSeeingMe++;
        else if (turretsSeeingMe > 0) turretsSeeingMe--;
    }

    private bool isInputLocked = false;

    private PlayerInput playerInput;
    private Mouse mouse;
    private float yaw;
    private float pitch;

    private Rigidbody _rigidbody;
    private InputAction interactAction;
    private InputAction jumpAction;
    private InputAction crouchAction;
    private InputAction proneAction;
    private InputAction runAction;

    // Stance settings
    public float crouchHeightRatio = 0.5f;
    public float proneHeightRatio = 0.25f;
    private float playerHeight = 0f;
    private int stance = 0; 
    private Vector3 originalScale;
    private Vector3 savedPosition;
    private Vector3 hideSpotPosition;

    public Camera auxiliarCamera;
    public PlayerTemperature playerTemperature;

    [SerializeField] GameObject eye;

    void Awake()
    {
        // 删除原来的 interactableMask = ... 代码，改在 Inspector 里设置
    }

    void Start()
    {
        originalScale = transform.localScale;
        mouse = Mouse.current;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _rigidbody = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        playerTemperature = GetComponent<PlayerTemperature>();

        audioSource_agitated.enabled = false;
        audioSource_heartbeat.enabled = false;

        if (transform.Find("Body") != null)
        {
            playerHeight = transform.Find("Body").gameObject.GetComponent<CapsuleCollider>().height;
        }

        interactAction = playerInput.actions.FindAction("Interact");
        jumpAction = playerInput.actions.FindAction("Jump");
        crouchAction = playerInput.actions.FindAction("Crouch");
        proneAction = playerInput.actions.FindAction("Prone");
        runAction = playerInput.actions.FindAction("Run");

        if (auxiliarCamera != null) auxiliarCamera.enabled = false;

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

        // Camera
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

        // Stance
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

        PlayHighTemperatureSFX();
    }

    void FixedUpdate()
    {
        if (isInputLocked)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            return;
        }
        movePlayer();
    }

    public void TriggerRespawnSequence()
    {
        if(audioSource != null && sfx_dead != null)
        {
            audioSource.clip = sfx_dead;
            audioSource.Play();
        }

        if (!isInputLocked) StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        IsRunning = false;
        IsMoving = false;
        isInputLocked = true;
        yield return new WaitForSeconds(2f);

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
            Color c = blackScreenImage.color; c.a = 1f; blackScreenImage.color = c;
        }

        GameManager.ResetLevel(); // 确保你有这个类，否则注释掉
        yield return new WaitForSeconds(2f);

        if (startingPoint != null)
        {
            transform.position = startingPoint.position;
            _rigidbody.linearVelocity = Vector3.zero;
            transform.rotation = startingPoint.rotation;
        }

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
        if (blackScreenImage != null)
        {
            Color c = blackScreenImage.color; c.a = 0f; blackScreenImage.color = c;
        }
        isInputLocked = false;
    }

    private void movePlayer()
    {
        IsRunning = runAction != null && runAction.IsPressed();
        float currentMaxSpeed = IsRunning ? runSpeed : walkSpeed;
        float currentAcceleration = accelerationFactor;

        Vector2 direction = playerInput.actions["Move"].ReadValue<Vector2>().normalized;
        IsMoving = direction.magnitude > 0.1f;

        Vector3 acceleration = transform.right * direction.x * currentAcceleration + transform.forward * direction.y * currentAcceleration;
        _rigidbody.AddForce(acceleration, ForceMode.Acceleration);

        Vector3 horizontalVelocity = new Vector3(_rigidbody.linearVelocity.x, 0f, _rigidbody.linearVelocity.z);
        currentHorizontalSpeed = horizontalVelocity.magnitude;

        if (currentHorizontalSpeed > currentMaxSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * currentMaxSpeed;
            _rigidbody.linearVelocity = new Vector3(horizontalVelocity.x, _rigidbody.linearVelocity.y, horizontalVelocity.z);
        }
    }

    private void InteractWithObjects()
    {
        RaycastHit hit;
        // 关键修复：QueryTriggerInteraction.Collide 确保能打到 Trigger
        if (Physics.Raycast(eye.transform.position, eye.transform.TransformDirection(Vector3.forward), out hit, interactiveRange, interactableMask, QueryTriggerInteraction.Collide))
        {
            // 调试信息：看看到底打到了谁
            Debug.Log("Raycast Hit: " + hit.collider.gameObject.name);

            if (hit.collider.gameObject.tag == "HideSpot")
            {
                if (!isHidden)
                {
                    savedPosition = transform.position;
                    hideSpotPosition = hit.collider.gameObject.transform.position;
                    isHidden = true;
                    Camera mainCamera = GetComponentInChildren<Camera>();
                    if (auxiliarCamera != null)
                    {
                        auxiliarCamera.gameObject.SetActive(true);
                        auxiliarCamera.enabled = true;
                    }
                    if (mainCamera != null) mainCamera.enabled = false;
                }
                else
                {
                    isHidden = false;
                    Camera mainCamera = GetComponentInChildren<Camera>();
                    if (mainCamera != null) mainCamera.enabled = true;
                    if (auxiliarCamera != null)
                    {
                        auxiliarCamera.gameObject.SetActive(false);
                        auxiliarCamera.enabled = false;
                    }
                }
            }

            // 优先检测 Rigidbody (大部分交互物体都有 RB)
            if (hit.rigidbody)
            {
                Interaction.IInteractable target = hit.rigidbody.gameObject.GetComponent<Interaction.IInteractable>();
                if (target != null)
                {
                    ((Interaction.IInteractor)this).Interact((object)target);
                }
            }
            // 备用检测 Collider (以防物体没有 RB)
            else
            {
                Interaction.IInteractable target = hit.collider.gameObject.GetComponent<Interaction.IInteractable>();
                if (target != null)
                {
                    ((Interaction.IInteractor)this).Interact((object)target);
                }
            }
        }
    }

    private void jump()
    {
        if (enableJump && isGrounded())
            _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private bool isGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 0.5f);
    }

    private void switchStance(int st)
    {
        float targetScale = 1f;
        if (st == 0) targetScale = 1f;
        else if (st == 1) targetScale = crouchHeightRatio;
        else if (st == 2) targetScale = proneHeightRatio;

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
        if (isHidden && auxiliarCamera != null)
        {
            auxiliarCamera.transform.position = Vector3.Lerp(transform.position, hideSpotPosition, 5 * Time.deltaTime);
        }
    }

    public void PlayHighTemperatureSFX()
    {
        currentTemperature = playerTemperature.GetCurrentTemperature();

        if (currentTemperature >= 37f)
        {
            if (audioSource_agitated != null)
            {
                audioSource_agitated.enabled = true;
            }
            if (audioSource_heartbeat != null)
            {
                audioSource_heartbeat.enabled = true;
            }
        }
        if (currentTemperature < 37f)
        {
            if (audioSource_agitated != null)
            {
                audioSource_agitated.enabled = false;
            }
            if (audioSource_heartbeat != null)
            {
                audioSource_heartbeat.enabled = false;
            }
        }
    }
}