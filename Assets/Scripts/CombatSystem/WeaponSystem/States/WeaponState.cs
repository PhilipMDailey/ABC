using UnityEngine;

/// <summary>
/// Base class for all weapon animation states.
/// Mirrors the mob state machine pattern for consistency.
/// </summary>
public abstract class WeaponState
{
    protected WeaponAnimator animator;

    public WeaponState(WeaponAnimator animator)
    {
        this.animator = animator;
    }

    /// <summary>Called once when entering this state.</summary>
    public virtual void OnEnter() { }

    /// <summary>Called every frame while in this state.</summary>
    public virtual void OnUpdate() { }

    /// <summary>Called once when leaving this state.</summary>
    public virtual void OnExit() { }
}