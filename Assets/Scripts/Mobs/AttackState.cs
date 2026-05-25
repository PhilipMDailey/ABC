using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Mob moves toward the player when they are within perception range,
/// while applying separation steering to avoid bunching with other mobs.
/// Transitions back to WanderState when the player moves out of range.
/// </summary>
public class AttackState : MobState
{
    private Transform player;
    private MobDebugVisualizer debugVisualizer;

    public AttackState(MobBase mob) : base(mob)
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("AttackState: No GameObject tagged 'Player' found.");

        debugVisualizer = mob.GetComponent<MobDebugVisualizer>();
    }

    public override void OnEnter()
    {
        Debug.Log($"{mob.name} entered AttackState.");
    }

    public override void OnUpdate()
    {
        if (player == null) return;

        if (mob.CanSeePlayer())
        {
            // Calculate chase direction toward player
            Vector3 chaseDirection = (player.position - mob.transform.position);
            chaseDirection.y = 0f;
            chaseDirection.Normalize();

            // Calculate separation force away from nearby mobs
            Vector3 separationForce = CalculateSeparationForce();

            // Blend chase and separation
            Vector3 finalDirection = (chaseDirection + separationForce * mob.separationStrength).normalized;

            // Push steering data to visualizer if present
            if (debugVisualizer != null)
            {
                debugVisualizer.debugChaseDirection = chaseDirection;
                debugVisualizer.debugSeparationForce = separationForce;
                debugVisualizer.debugFinalDirection = finalDirection;
                debugVisualizer.hasSteeringData = true;
            }

            mob.MoveInDirection(finalDirection);
        }
        else
        {
            // Clear steering data when not chasing
            if (debugVisualizer != null)
                debugVisualizer.hasSteeringData = false;

            mob.TransitionTo(new WanderState(mob));
        }
    }

    public override void OnExit()
    {
        if (debugVisualizer != null)
            debugVisualizer.hasSteeringData = false;

        Debug.Log($"{mob.name} exited AttackState.");
    }

    Vector3 CalculateSeparationForce()
    {
        Vector3 force = Vector3.zero;
        int neighbourCount = 0;

        Collider[] nearby = Physics.OverlapSphere(mob.transform.position, mob.separationRadius);

        foreach (var col in nearby)
        {
            if (col.gameObject == mob.gameObject) continue;
            if (col.CompareTag("Player")) continue;

            MobBase nearbyMob = col.GetComponent<MobBase>();
            if (nearbyMob == null) continue;

            Vector3 awayDirection = mob.transform.position - col.transform.position;
            awayDirection.y = 0f;

            float distance = awayDirection.magnitude;

            if (distance < 0.001f)
            {
                awayDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
                distance = 0.001f;
            }

            float weight = 1f - Mathf.Clamp01(distance / mob.separationRadius);
            force += awayDirection.normalized * weight;
            neighbourCount++;
        }

        if (neighbourCount > 0)
            force /= neighbourCount;

        return force;
    }
}