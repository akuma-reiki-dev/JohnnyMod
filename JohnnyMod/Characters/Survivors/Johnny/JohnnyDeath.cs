using EntityStates;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace JohnnyMod.Survivors.Johnny.SkillStates
{
    public class JohnnyDeath : GenericCharacterDeath
    {
		public override void OnEnter()
		{
			base.OnEnter();

            if (isGrounded)
            {
				TriggerRagdoll(false);
            }
		}

		public void TriggerRagdoll(bool useForce)
        {
			Vector3 vector = Vector3.up * 3f;
			if (base.characterMotor)
			{
				vector += base.characterMotor.velocity;
				base.characterMotor.enabled = false;
			}
			if (base.cachedModelTransform)
			{
				RagdollController ragdollController = base.cachedModelTransform.GetComponent<RagdollController>();
				if (ragdollController)
				{
					ragdollController.BeginRagdoll(useForce ? vector : Vector3.zero);
				}
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (NetworkServer.active && base.fixedAge > 2f)
			{
				EntityState.Destroy(base.gameObject);
			}
		}

        public override void PlayDeathSound()
        {
            base.PlayDeathSound();

			Util.PlaySound("PlayLostVoice", gameObject);
		}

        public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Death;
		}
	}
}
