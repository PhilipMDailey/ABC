using UnityEngine;

/// <summary>
/// Smoothly lerps the weapon back to idle position and rotation
/// after an attack or block completes.
/// Transitions to WeaponIdleState once close enough to idle.
/// </summary>
public class WeaponReturnState : WeaponState
{
    private float lerpSpeed;
    private float arrivalThreshold = 0.01f;

    public WeaponReturnState(WeaponAnimator animator) : base(animator) { }

    public override void OnEnter()
    {
        lerpSpeed = animator.transitionSpeed;
    }

    public override void OnUpdate()
    {
        // Lerp toward idle position
        animator.WeaponTransform.localPosition = Vector3.Lerp(
            animator.WeaponTransform.localPosition,
            animator.idlePosition,
            lerpSpeed * Time.deltaTime
        );

        // Slerp toward idle rotation
        animator.WeaponTransform.localRotation = Quaternion.Slerp(
            animator.WeaponTransform.localRotation,
            Quaternion.Euler(animator.idleRotation),
            lerpSpeed * Time.deltaTime
        );

        // Transition to idle once close enough
        float positionDelta = Vector3.Distance(
            animator.WeaponTransform.localPosition,
            animator.idlePosition
        );

        if (positionDelta <= arrivalThreshold)
            animator.TransitionTo(new WeaponIdleState(animator));
    }
}