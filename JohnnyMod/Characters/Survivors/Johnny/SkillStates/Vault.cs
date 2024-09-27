using EntityStates;
using JohnnyMod.Survivors.Johnny;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace JohnnyMod.Survivors.Johnny.SkillStates
{
    public class Vault : BaseCharacterMain
    {
        public static float duration = 0.5f;
        public static float initialSpeedCoefficient = 4f;
        public static float finalSpeedCoefficient = 2;

        public static string dodgeSoundString = "PlayLeap";
        public static float vaultFOV = global::EntityStates.Commando.DodgeState.dodgeFOV;

        private float vaultSpeed;
        private Vector3 forwardDirection;
        private float upwardVelocity = 28; //28
        private Animator animator;
        private Vector3 previousPosition;
        private float minimumDuration = 0.3f;

        public override void OnEnter()
        {
            base.OnEnter();
            animator = GetModelAnimator();

            if(isAuthority && inputBank && characterDirection)
            {
                forwardDirection = (inputBank.moveVector == Vector3.zeroVector ? characterDirection.forward : inputBank.moveVector).normalized;
            }

            RecalcSpeed();
            Vector3 direction = base.GetAimRay().direction;

            if (characterMotor && characterDirection)
            {
                characterMotor.Motor.ForceUnground(0.2f);
                direction.z *= 0;
                direction.x *= 0;
                //direction.y = Mathf.Max(direction.y, 0.01f);
                Vector3 aimVel = direction.normalized * this.moveSpeedStat;
                Vector3 forVel = new Vector3(forwardDirection.x, 0, forwardDirection.z).normalized * vaultSpeed;
                Vector3 upVel = Vector3.up * upwardVelocity;
                characterMotor.velocity = forVel + upVel + aimVel;
            }

            Vector3 b = characterMotor ? characterMotor.velocity : Vector3.zero;
            previousPosition = transform.position - b;

            PlayAnimation("FullBody, Override", "Vault", "Roll.playbackRate", duration);
            Util.PlaySound(dodgeSoundString, gameObject);

            if (NetworkServer.active)
            {
                characterBody.AddTimedBuff(JohnnyBuffs.armorBuff, 3 * duration);
            }

            base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
        }

        private void RecalcSpeed()
        {
            vaultSpeed = moveSpeedStat * Mathf.Lerp(initialSpeedCoefficient, finalSpeedCoefficient, fixedAge / duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            RecalcSpeed();

            if (characterDirection) characterDirection.forward = forwardDirection;
            if (cameraTargetParams) cameraTargetParams.fovOverride = Mathf.Lerp(vaultFOV, 60f, fixedAge / duration);

            Vector3 normalized = (transform.position - previousPosition).normalized;
            if(characterMotor && characterDirection && normalized != Vector3.zero)
            {
                Vector3 vector = normalized * vaultSpeed;
                float d = Mathf.Max(Vector3.Dot(vector, forwardDirection), 0);
                vector = forwardDirection * d;

                characterMotor.velocity.x = vector.x;
                characterMotor.velocity.z = vector.z;
            }
            base.previousPosition = transform.position;

            if(isAuthority && (fixedAge >= duration) || (fixedAge >= minimumDuration && characterMotor.Motor.GroundingStatus.IsStableOnGround && !base.characterMotor.Motor.LastGroundingStatus.IsStableOnGround))
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            //Util.PlaySound(soundLoopStopEvent, base.gameObject);
            base.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
            base.characterBody.isSprinting = false;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
        }
    }
}