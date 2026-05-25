using UnityEngine;

/// <summary>
/// Mob stands still and periodically looks around.
/// Transitions to WanderState after a random idle duration.
/// </summary>
public class IdleState : MobState
{
    private float idleDuration;
    private float idleTimer;

    public IdleState(MobBase mob) : base(mob) { }

    public override void OnEnter()
    {
        // Stand still for a random amount of time before wandering
        idleDuration = Random.Range(2f, 5f);
        idleTimer = 0f;
    }

    public override void OnUpdate()
    {
        // Check if player is in range — transition to attack
        if (mob.CanSeePlayer())
        {
            mob.TransitionTo(new AttackState(mob));
            return;
        }
        
        idleTimer += Time.deltaTime;

        if (idleTimer >= idleDuration)
            mob.TransitionTo(new WanderState(mob));
    }
}