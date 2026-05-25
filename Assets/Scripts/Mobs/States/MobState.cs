/// <summary>
/// Base class for all mob states.
/// Every state inherits from this and overrides the methods it needs.
/// </summary>
public abstract class MobState
{
    protected MobBase mob;

    public MobState(MobBase mob)
    {
        this.mob = mob;
    }

    /// <summary>Called once when entering this state.</summary>
    public virtual void OnEnter() { }

    /// <summary>Called every frame while in this state.</summary>
    public virtual void OnUpdate() { }

    /// <summary>Called once when leaving this state.</summary>
    public virtual void OnExit() { }
}