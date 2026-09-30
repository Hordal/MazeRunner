using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    public static float MouseSensitivityMultiplier { get; set; } = 1f;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float moveSpeed = 4.5f;
    [SerializeField] private float lookSensitivity = 0.18f;
    [SerializeField] private float gravity = -20f;
    // 4m 폭 미로 안에서도 벽 바깥으로 나가지 않는 고정 추적 시점이다.
    [SerializeField] private float thirdPersonDistance = 1.3f;
    [SerializeField] private float thirdPersonHeight = 3.45f;
    [SerializeField] private float thirdPersonBasePitch = 43f;

    private CharacterController controller;
    private float verticalVelocity;
    private float cameraPitch;
    private Vector3 startPosition;
    private float speedBoostEndsAt;
    private float speedMultiplier = 1f;
    private float freezeEndsAt;
    private float flashlightEffectEndsAt;
    private float flashlightMultiplier = 1f;
    private Light flashlight;
    private float baseFlashlightIntensity;
    private float nextFootstepAt;
    private float stamina = 100f;
    private float infiniteStaminaEndsAt;
    private bool sprintExhausted;
    private bool isSprinting;
    private bool isMoving;
    private bool isThirdPerson;
    private float firstPersonFieldOfView;
    private Vector3 checkpoint;

    public float Stamina => stamina;
    public bool IsSprinting => isSprinting;
    public bool IsMoving => isMoving;
    public bool IsThirdPerson => isThirdPerson;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (playerCamera == null) playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera != null) firstPersonFieldOfView = playerCamera.fieldOfView;
        flashlight = GetComponentInChildren<Light>();
        if (flashlight != null) baseFlashlightIntensity = flashlight.intensity;
        startPosition = transform.position;
        checkpoint = startPosition;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (MazeGameManager.Instance != null && (MazeGameManager.Instance.HasEscaped || MazeGameManager.Instance.HasFailed)) return;
        if (Time.time >= speedBoostEndsAt) speedMultiplier = 1f;
        if (Time.time >= flashlightEffectEndsAt) flashlightMultiplier = 1f;
        if (flashlight != null) flashlight.intensity = baseFlashlightIntensity * flashlightMultiplier;
        // 바닥 아래로 빠지는 상황이 생겨도 즉시 시작 지점으로 복귀한다.
        if (transform.position.y < -5f)
        {
            controller.enabled = false;
            transform.position = startPosition;
            controller.enabled = true;
            verticalVelocity = 0f;
        }

        if (Cursor.lockState != CursorLockMode.Locked || Keyboard.current == null) return;

        if (Keyboard.current.vKey.wasPressedThisFrame)
        {
            isThirdPerson = !isThirdPerson;
            playerCamera.fieldOfView = isThirdPerson ? 70f : firstPersonFieldOfView;
            playerCamera.transform.localPosition = isThirdPerson
                ? new Vector3(0f, thirdPersonHeight, -thirdPersonDistance)
                : new Vector3(0f, 1.6f, 0f);
        }

        Vector2 look = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
        transform.Rotate(Vector3.up * look.x * lookSensitivity * MouseSensitivityMultiplier);
        cameraPitch = Mathf.Clamp(cameraPitch - look.y * lookSensitivity * MouseSensitivityMultiplier, -80f, 80f);
        float appliedPitch = cameraPitch + (isThirdPerson ? thirdPersonBasePitch : 0f);
        playerCamera.transform.localRotation = Quaternion.Euler(appliedPitch, 0f, 0f);

        Vector2 input = Vector2.zero;
        if (Keyboard.current.wKey.isPressed) input.y += 1f;
        if (Keyboard.current.sKey.isPressed) input.y -= 1f;
        if (Keyboard.current.dKey.isPressed) input.x += 1f;
        if (Keyboard.current.aKey.isPressed) input.x -= 1f;
        bool hasMoveInput = input.sqrMagnitude > 0.01f;
        isMoving = hasMoveInput && Time.time >= freezeEndsAt;
        bool sprintKeyHeld = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
        bool wantsSprint = sprintKeyHeld && hasMoveInput;
        bool infiniteStamina = Time.time < infiniteStaminaEndsAt;
        if (infiniteStamina) sprintExhausted = false;

        // 스태미나가 완전히 소진되면 Shift를 누르고 있어도 강제로 걷는다.
        // Shift를 한 번 놓고 25 이상 회복한 뒤에야 다시 달릴 수 있다.
        if (!infiniteStamina && stamina <= 0.01f) sprintExhausted = true;
        isSprinting = wantsSprint && !sprintExhausted && (infiniteStamina || stamina > 0.01f) && Time.time >= freezeEndsAt;
        if (isSprinting && !infiniteStamina)
        {
            stamina = Mathf.Max(0f, stamina - 25f * Time.deltaTime);
            if (stamina <= 0.01f)
            {
                stamina = 0f;
                sprintExhausted = true;
                isSprinting = false;
            }
        }
        else if (!infiniteStamina)
        {
            stamina = Mathf.Min(100f, stamina + 8f * Time.deltaTime);
        }

        if (sprintExhausted && !sprintKeyHeld && stamina >= 25f) sprintExhausted = false;
        float currentSpeed = Time.time < freezeEndsAt ? 0f : moveSpeed * speedMultiplier * (isSprinting ? 1.75f : 1f);
        Vector3 move = (transform.forward * input.y + transform.right * input.x).normalized * currentSpeed;

        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;
        controller.Move(move * Time.deltaTime);
        if (input.sqrMagnitude > 0.01f && controller.isGrounded && Time.time >= nextFootstepAt)
        {
            if (MazeAudio.Instance != null) MazeAudio.Instance.PlayTone(180f, .06f, .05f);
            nextFootstepAt = Time.time + .42f;
        }
    }

    public void ApplySpeedEffect(float multiplier, float duration)
    {
        speedBoostEndsAt = Mathf.Max(speedBoostEndsAt, Time.time) + duration;
        speedMultiplier = multiplier;
    }

    public void ApplyFlashlightEffect(float multiplier, float duration)
    {
        flashlightEffectEndsAt = Mathf.Max(flashlightEffectEndsAt, Time.time) + duration;
        flashlightMultiplier = multiplier;
    }

    public void ApplyFreeze(float duration) => freezeEndsAt = Mathf.Max(freezeEndsAt, Time.time) + duration;
    public void RestoreStamina(float amount) => stamina = Mathf.Min(100f, stamina + amount);
    public void ApplyInfiniteStamina(float duration) => infiniteStaminaEndsAt = Mathf.Max(infiniteStaminaEndsAt, Time.time) + duration;
    public void SetCheckpoint(Vector3 position) => checkpoint = position;
    public void ReturnToCheckpoint()
    {
        controller.enabled = false;
        transform.position = checkpoint;
        controller.enabled = true;
        stamina = 100f;
    }

}
