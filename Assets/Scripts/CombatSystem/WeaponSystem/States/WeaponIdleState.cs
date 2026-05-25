using UnityEngine;

/// <summary>
/// Weapon rests at the idle position and rotation.
/// Waits for attack or block input to transition.
/// </summary>
public class WeaponIdleState : WeaponState
{
    private float lerpSpeed;

    public WeaponIdleState(WeaponAnimator animator) : base(animator) { }

    public override void OnEnter()
    {
        lerpSpeed = animator.transitionSpeed;
    }

    public override void OnUpdate()
    {
        // Smoothly lerp weapon to idle position and rotation
        animator.WeaponTransform.localPosition = Vector3.Lerp(
            animator.WeaponTransform.localPosition,
            animator.idlePosition,
            lerpSpeed * Time.deltaTime
        );

        animator.WeaponTransform.localRotation = Quaternion.Slerp(
            animator.WeaponTransform.localRotation,
            Quaternion.Euler(animator.idleRotation),
            lerpSpeed * Time.deltaTime
        );
    }
}