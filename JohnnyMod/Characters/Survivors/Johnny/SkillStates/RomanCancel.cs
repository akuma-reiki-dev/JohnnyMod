using EntityStates;
using JohnnyMod.Survivors.Johnny;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace JohnnyMod.Survivors.Johnny.SkillStates
{
    public class RomanCancel : GenericCharacterMain
    {
        private float driftTime = 0.1f;
        private Vector3 prevVel;
        private Vector3 prevLastVel;
        private bool unsetPrevVel;
        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();

            if (isAuthority)
            {
                EntityStateMachine Body = EntityStateMachine.FindByCustomName(gameObject, "Body");
                EntityStateMachine Weap = EntityStateMachine.FindByCustomName(gameObject, "Weapon");
                EntityStateMachine Weap2 = EntityStateMachine.FindByCustomName(gameObject, "Weapon2");
                Body.SetState(new Idle());
                Weap.SetState(new Idle());
                Weap2.SetState(new Idle());

                PlayAnimation("FullBody, Override", "RomanCancel");
                GetModelAnimator().Play("RomanCancel");
            }

            EffectData effData = new EffectData
            {
                origin = gameObject.transform.position,
                scale = 2,
            };
            EffectManager.SpawnEffect(JohnnyAssets.romanCancelEffect, effData, transmit: true);

            unsetPrevVel = false;
            duration = 0.35f;
            Util.PlaySound("PlayRC", gameObject);

            if (NetworkServer.active)
            {
                characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 0.15f);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(fixedAge >= duration && isAuthority)
            {
                outer.SetNextState(new RomanIdle());
                // we reset the Body ESM to JohnnyMainState. This is done because we can't exactly nullify the inputs through the secondary IdleState
                EntityStateMachine Body = EntityStateMachine.FindByCustomName(gameObject, "Body");
                Body.SetState(new JohnnyMainState());
            }
        }

        public override bool CanExecuteSkill(GenericSkill skillSlot)
        {
            return false;
        }

        public override void HandleMovements()
        {
            if (!unsetPrevVel)
            {
                //we have this so we dont accidentally set the previous velocity to 0
                prevVel = characterMotor.velocity;
                prevLastVel = characterMotor.lastVelocity;
                unsetPrevVel = true;
            }

            characterMotor.velocity = Vector3.zero;

            if(fixedAge >= driftTime)
            {
                Vector3 moveVec = inputBank.moveVector;

                Vector3 multVel = Vector3.Max(prevVel, prevLastVel);
                Vector3 desiredVel;
                desiredVel.x = multVel.x * 1.05f;
                desiredVel.y = multVel.y * 1.05f;
                desiredVel.z = multVel.z * 1.05f;
                
                characterMotor.velocity = desiredVel;
            }
        }
    }
}
