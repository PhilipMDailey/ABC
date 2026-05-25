using UnityEngine;

/// <summary>
/// Weapon follows the attack path with hilt and tip locked to their offsets.
/// Drives both the weapon model and visual indicator through WeaponAnimator.
/// Transitions to WeaponReturnState when the attack completes.
/// </summary>
public class WeaponAttackState : WeaponState
{
    private AttackPath path;
    private bool attackComplete = false;
    private float progress = 0f;

    private bool isTransitioning = true;
    private float transitionTimer = 0f;
    private float transitionDuration = 0.15f;
    private Vector3 transitionStartPosition;
    private Quaternion transitionStartRotation;

    public WeaponAttackState(WeaponAnimator animator, AttackPath path) : base(animator)
    {
        this.path = path;
    }

    public override void OnEnter()
    {
        progress = 0f;
        attackComplete = false;
        isTransitioning = true;
        transitionTimer = 0f;

        // Capture weapon's current transform for blending
        if (animator.WeaponTransform != null)
        {
            transitionStartPosition = animator.WeaponTransform.position;
            transitionStartRotation = animator.WeaponTransform.rotation;
        }

        animator.CreateIndicator();
    }

    public override void OnUpdate()
    {
        if (path == null) return;

        if (isTransitioning)
        {
            transitionTimer += Time.deltaTime;
            float t = Mathf.Clamp01(transitionTimer / transitionDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            Vector3 arcStartHilt = path.GetHiltPosition(0f, animator.transform.parent);
            Vector3 arcStartTip = path.GetTipPosition(0f, animator.transform.parent);

            if (animator.WeaponTransform != null)
            {
                Vector3 bladeDir = (arcStartTip - arcStartHilt).normalized;

                // Calculate where the weapon origin needs to be so the
                // hilt marker lands on the arc start point
                Vector3 targetWeaponPosition = arcStartHilt;
                WeaponMarkers markers = animator.WeaponTransform.GetComponent<WeaponMarkers>();
                if (markers != null && markers.hiltMarker != null && bladeDir != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(bladeDir);
                    // Temporarily apply target rotation to get correct marker offset
                    Quaternion previousRotation = animator.WeaponTransform.rotation;
                    animator.WeaponTransform.rotation = targetRotation;
                    Vector3 markerOffset = animator.WeaponTransform.position 
                        - markers.hiltMarker.position;
                    animator.WeaponTransform.rotation = previousRotation;
                    targetWeaponPosition = arcStartHilt + markerOffset;
                }

                animator.WeaponTransform.position = Vector3.Lerp(
                    transitionStartPosition, targetWeaponPosition, t);

                if (bladeDir != Vector3.zero)
                    animator.WeaponTransform.rotation = Quaternion.Slerp(
                        transitionStartRotation, Quaternion.LookRotation(bladeDir), t);
            }

            if (t >= 1f)
                isTransitioning = false;

            return;
        }

        if (attackComplete)
        {
            animator.TransitionTo(new WeaponReturnState(animator));
            return;
        }

        Vector3 hiltWorld = path.GetHiltPosition(progress, animator.transform.parent);
        Vector3 tipWorld = path.GetTipPosition(progress, animator.transform.parent);
        animator.UpdateAttackTransforms(hiltWorld, tipWorld, progress);
    }

    public override void OnExit()
    {
        animator.DestroyIndicator();
    }

    /// <summary>
    /// Called each frame by PlayerBasicAttack to keep the animation in sync with combat.
    /// </summary>
    public void SetProgress(float progress)
    {
        this.progress = progress;
    }

    /// <summary>
    /// Called by PlayerBasicAttack when the attack completes.
    /// </summary>
    public void CompleteAttack()
    {
        attackComplete = true;
    }
}