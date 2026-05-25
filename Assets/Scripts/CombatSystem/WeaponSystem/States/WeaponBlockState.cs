using UnityEngine;

/// <summary>
/// Weapon raised in block position.
/// Held until block input is released, then transitions to WeaponReturnState.
/// </summary>
public class WeaponBlockState : WeaponState
{
    private float lerpSpeed;

    public WeaponBlockState(WeaponAnimator animator) : base(animator) { }

    public override void OnEnter()
    {
        lerpSpeed = animator.transitionSpeed;
    }

    public override void OnUpdate()
    {
        // Smoothly lerp weapon to block position and rotation
        animator.WeaponTransform.localPosition = Vector3.Lerp(
            animator.WeaponTransform.localPosition,
            animator.blockPosition,
            lerpSpeed * Time.deltaTime
        );

        animator.WeaponTransform.localRotation = Quaternion.Slerp(
            animator.WeaponTransform.localRotation,
            Quaternion.Euler(animator.blockRotation),
            lerpSpeed * Time.deltaTime
        );
    }

    public override void OnExit() { }
}