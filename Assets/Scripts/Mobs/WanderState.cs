using UnityEngine;

/// <summary>
/// Mob picks a random nearby destination and walks to it.
/// Transitions back to IdleState upon arrival.
/// </summary>
public class WanderState : MobState
{
    private Vector3 destination;
    private float wanderRadius = 10f;
    private float stuckTimer = 0f;
    private float stuckTimeout = 5f; // Give up and idle if stuck for this long

    public WanderState(MobBase mob) : base(mob) { }

    public override void OnEnter()
    {
        destination = PickDestination();
        stuckTimer = 0f;
    }

    public override void OnUpdate()
    {
        // Check if player is in range — transition to attack
        if (mob.CanSeePlayer())
        {
            mob.TransitionTo(new AttackState(mob));
            return;
        }
        
        stuckTimer += Time.deltaTime;

        bool arrived = mob.MoveToward(destination, stoppingDistance: 1f);

        if (arrived || stuckTimer >= stuckTimeout)
            mob.TransitionTo(new IdleState(mob));
    }

    Vector3 PickDestination()
    {
        // Pick a random point within wanderRadius of current position
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        Vector3 candidate = mob.transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);
        return candidate;
    }
}