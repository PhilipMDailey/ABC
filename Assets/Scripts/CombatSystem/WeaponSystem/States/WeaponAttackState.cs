// using UnityEngine;

// /// <summary>
// /// Weapon follows the attack arc with hilt and tip locked to their offsets.
// /// Driven by the arc progress supplied by PlayerBasicAttack.
// /// Transitions to WeaponReturnState when the attack completes.
// /// </summary>
// public class WeaponAttackState : WeaponState
// {
//     private bool attackComplete = false;

//     public WeaponAttackState(WeaponAnimator animator) : base(animator) { }

//     public override void OnEnter()
//     {
//         attackComplete = false;
//     }

//     public override void OnUpdate()
//     {
//         if (attackComplete)
//         {
//             animator.TransitionTo(new WeaponReturnState(animator));
//             return;
//         }

//         // Position and rotation are driven externally by SetArcTransform
//         // called each frame from PlayerBasicAttack during the arc routine
//     }

//     public override void OnExit() { }

//     /// <summary>
//     /// Called each frame by PlayerBasicAttack to drive the weapon transform
//     /// to match the current arc position.
//     /// </summary>
//     public void SetArcTransform(Vector3 hiltWorld, Vector3 tipWorld)
//     {
//         if (animator.WeaponTransform == null) return;

//         // Position weapon at hilt
//         animator.WeaponTransform.position = hiltWorld;

//         // Orient weapon so it points from hilt to tip
//         Vector3 bladeDirection = (tipWorld - hiltWorld).normalized;
//         if (bladeDirection != Vector3.zero)
//             animator.WeaponTransform.rotation = Quaternion.LookRotation(bladeDirection);
//     }

//     /// <summary>
//     /// Called by PlayerBasicAttack when the arc completes.
//     /// </summary>
//     public void CompleteAttack()
//     {
//         attackComplete = true;
//     }
// }





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

    public WeaponAttackState(WeaponAnimator animator, AttackPath path) : base(animator)
    {
        this.path = path;
    }

    public override void OnEnter()
    {
        progress = 0f;
        attackComplete = false;
        animator.CreateIndicator();
    }

    public override void OnUpdate()
    {
        if (attackComplete)
        {
            animator.TransitionTo(new WeaponReturnState(animator));
            return;
        }

        if (path == null) return;

        // Get current hilt and tip positions from the path
        // Progress is updated externally by PlayerBasicAttack via SetProgress
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