using UnityEngine;

/// <summary>
/// Drives the Mixamo avatar without giving it control of player movement.
/// The CharacterController remains the single source of movement and collision.
/// </summary>
public sealed class PlayerAvatarVisual : MonoBehaviour
{
    public FirstPersonController controller;
    public Animator animator;

    private Renderer[] renderers;
    private bool wasVisible;

    private void Awake()
    {
        if (controller == null) controller = GetComponentInParent<FirstPersonController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        renderers = GetComponentsInChildren<Renderer>(true);
        SetVisible(false);
    }

    private void Update()
    {
        if (controller == null || animator == null) return;

        // 애니메이션은 V키로 보는 3인칭 시점에서만 보인다.
        bool shouldShow = controller.IsThirdPerson;
        if (shouldShow != wasVisible) SetVisible(shouldShow);

        animator.SetBool("Sprinting", controller.IsSprinting);
        // 별도 Idle 파일 없이도 멈췄을 때 걷기 프레임이 진행되지 않게 한다.
        // X Bot용 달리기 파일은 Remy에 적용하면 팔 동작이 깨지므로,
        // 안정적인 걷기 동작을 달릴 때만 빠르게 재생한다.
        animator.speed = controller.IsMoving ? (controller.IsSprinting ? 1.65f : 1f) : 0f;
    }

    private void SetVisible(bool visible)
    {
        wasVisible = visible;
        if (renderers == null) return;
        foreach (Renderer item in renderers) item.enabled = visible;
    }
}
