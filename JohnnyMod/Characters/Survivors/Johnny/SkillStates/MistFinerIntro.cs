using EntityStates;
using JohnnyMod.Survivors.Johnny;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;

namespace JohnnyMod.Survivors.Johnny.SkillStates
{
    public class MistFinerIntro : BaseSkillState
    {
        private Animator animator;

        private float baseDuration;
        private float duration;
        public override void OnEnter()
        {
            base.OnEnter();

            animator = GetModelAnimator();

            PlayAnimation("UpperBody, Override", "MistFinerIntroB");
            animator.SetBool("MistFiner.channeled", true);

            //Replaces Utility skill with Mist Finer Dash
            if (base.skillLocator.utility != null)
            {
                base.skillLocator.utility.SetSkillOverride(gameObject, JohnnyStaticValues.MistFinerDash, GenericSkill.SkillOverridePriority.Contextual);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void OnExit()
        {
            base.OnExit();

        }
    }
}