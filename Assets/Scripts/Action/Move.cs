using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Move : BaseAction
{
    private bool updateRotation = true;
    private bool forceCompletion = true;
    private bool overrideMovement;
    private float speedMultiplier = 1;

    private List<Profile> targetProfiles = new List<Profile>();
    private float originalSpeed;

    private WaitForSeconds wait = new WaitForSeconds(0.5f);

    public Move(AbilityActionData data) : base(data)
    {
        var updateRotationField = this.data.BoolFields.Find((a) => a.Name == "updateRotation");
        if (updateRotationField != null)
        {
            updateRotation = updateRotationField.Value;
        }

        var speedMultiplierField = this.data.FloatFields.Find((a) => a.Name == "speedMultiplier");
        if (speedMultiplierField != null)
        {
            speedMultiplier = speedMultiplierField.Value;
        }

        var forceCompletionField = this.data.BoolFields.Find((a) => a.Name == "forceCompletion");
        if (forceCompletionField != null)
        {
            forceCompletion = forceCompletionField.Value;
        }

        var overrideMovementField = this.data.BoolFields.Find((a) => a.Name == "overrideMovement");
        if (overrideMovementField != null)
        {
            overrideMovement = overrideMovementField.Value;
        }
    }

    public override IEnumerator Excecute(Ability owner, GameObject target, Vector3 targetPoint)
    {
        var targetProfile = target.GetComponent<Profile>();

        Debug.Log(owner.owner.name + " casting move to " + target.name + " towards " + targetPoint);

        if (targetProfile)
        {
            targetProfiles.Add(targetProfile);

            if (targetProfile.combatController.IsStuned)
            {
                yield return null;
            }

            if (overrideMovement)
            {
                targetProfile.combatController.IsStuned = true;
            }

            originalSpeed = targetProfile.navAgent.speed;
            var speed = originalSpeed * speedMultiplier;
            targetProfile.navAgent.updateRotation = updateRotation;
            targetProfile.navAgent.isStopped = false;
            targetProfile.navAgent.speed = speed;
            targetProfile.navAgent.SetDestination(targetPoint);
            targetProfile.fsm.SetBool("Moving", true);

            if (forceCompletion && targetProfile && targetProfile.navAgent.isOnNavMesh)
            {
                yield return wait;  // Hack. navagent takes some time to start
                while (targetProfile.navAgent.remainingDistance > 3) // NavMeshAgent stop distance   TODO: abstract out constants
                {
                    yield return null;
                }
            }
        }
    }

    public override IEnumerator Reset()
    {
        foreach (var profile in targetProfiles)
        {
            profile.navAgent.isStopped = true;
            profile.navAgent.speed = originalSpeed;
            profile.fsm.SetBool("Moving", false);
            profile.combatController.IsStuned = false;
        }
        targetProfiles.Clear();
        yield return base.Reset();
    }

    public override BaseAction Clone()
    {
        return new Move(data);
    }
}