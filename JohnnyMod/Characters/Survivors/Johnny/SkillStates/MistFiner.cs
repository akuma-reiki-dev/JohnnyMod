using EntityStates;
using JohnnyMod.Survivors.Johnny;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;

namespace JohnnyMod.Survivors.Johnny.SkillStates
{
    public class MistFiner : BaseSkillState
    {
        public static float damageCoefficient = JohnnyStaticValues.mistFinerDamageCoeffecient;
        public static float procCoefficient = 1f;
        public static float baseDuration = 0.6f;
        //delay on firing is usually ass-feeling. only set this if you know what you're doing
        public static float firePercentTime = 0.0f;
        public static float recoil = 3f;
        public static float range = 32f;
        public static GameObject tracerEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerGoldGat");

        private float duration;
        private float fireTime;
        private bool hasFired;
        private string muzzleString;

        private Animator animator;
        private ChildLocator childLoc;

        private float lvl1time = 1f;
        private float lvl2time = 2.5f;

        private bool tier1 = false;
        private bool tier2 = false;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            lvl1time = 1 / attackSpeedStat;
            lvl2time = 3 / attackSpeedStat;
            fireTime = firePercentTime * duration;
            characterBody.SetAimTimer(2f);
            muzzleString = "KatanaHilt";

            childLoc = GetModelChildLocator();
            childLoc.FindChild("GhostHilt").gameObject.SetActive(false);
            childLoc.FindChild("KatanaHilt").gameObject.SetActive(true);
            childLoc.FindChild("KatanaBlade").gameObject.SetActive(true);
            childLoc.FindChild("SwordSimp").gameObject.SetActive(true);

            animator = GetModelAnimator();

            PlayAnimation("UpperBody, Override", "MistFinerIntro");
            animator.SetBool("MistFiner.channeled", true);

            //Replaces Utility skill with Mist Finer Dash
            if (base.skillLocator.utility != null)
            {
                base.skillLocator.utility.SetSkillOverride(gameObject, JohnnyStaticValues.MistFinerDash, GenericSkill.SkillOverridePriority.Contextual);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            animator.SetBool("MistFiner.channeled", false);
            childLoc.FindChild("GhostHilt").gameObject.SetActive(true);
            childLoc.FindChild("KatanaHilt").gameObject.SetActive(false);
            childLoc.FindChild("KatanaBlade").gameObject.SetActive(false);
            childLoc.FindChild("SwordSimp").gameObject.SetActive(false);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= lvl1time && !tier1)
            {
                tier1 = true;
                AkSoundEngine.SetRTPCValue("MFLevel", 0, gameObject);
                Util.PlaySound("PlayMistFinerLvlUp", gameObject);
                //spawn the level up effect on the sheath
                EffectData effectData = new EffectData
                {
                    origin = childLoc.FindChild("SheenSpawn").transform.position,
                    scale = 1f,
                };
                EffectManager.SpawnEffect(JohnnyAssets.mistFinerLvlUp, effectData, transmit: true);
            }
            if (fixedAge >= lvl2time && tier1 && !tier2)
            {
                tier2 = true;
                AkSoundEngine.SetRTPCValue("MFLevel", 1, gameObject);
                Util.PlaySound("PlayMistFinerLvlUp", gameObject);
                //spawn the level up effect on the sheath
                EffectData effectData = new EffectData
                {
                    origin = childLoc.FindChild("SheenSpawn").transform.position,
                    scale = 1f,
                };
                EffectManager.SpawnEffect(JohnnyAssets.mistFinerLvlUp, effectData, transmit: true);
            }

            if (fixedAge >= fireTime && inputBank.skill2.justReleased)
            {
                //we dont unreplace the skill until we fire, if its in OnExit you can't use stepdash
                //unreplaces the skill
                if (base.skillLocator.utility != null)
                {
                    base.skillLocator.utility.UnsetSkillOverride(gameObject, JohnnyStaticValues.MistFinerDash, GenericSkill.SkillOverridePriority.Contextual);
                    animator.SetBool("MistFiner.channeled", false);
                }
                //we dont fire until the key is released, it should make it fire 
                Fire();
            }

            if (fixedAge >= duration && isAuthority && hasFired)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        private void Fire()
        {
            if (!hasFired)
            {
                hasFired = true;

                characterBody.AddSpreadBloom(1.5f);
                EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, gameObject, muzzleString, false);

                animator.SetBool("MistFiner.channeled", false);
                PlayAnimation("UpperBody, Override", "MistFinerFire");

                if (isAuthority)
                {
                    Ray aimRay = GetAimRay();

                    float damage = damageCoefficient * damageStat;
                    if (tier1) damage *= 1.25f;
                    if (tier2) damage *= 1.75f; //m,ake this 2.5 :itwouldseemtroll:
                    //Mathf.Lerp(damage, damage * 2.5f, lvl2time)

                    new BulletAttack
                    {
                        bulletCount = 1,
                        aimVector = aimRay.direction,
                        origin = aimRay.origin,
                        damage = damage,
                        damageColorIndex = DamageColorIndex.Default,
                        damageType = DamageType.Generic,
                        falloffModel = BulletAttack.FalloffModel.None,
                        maxDistance = range,
                        hitMask = LayerIndex.CommonMasks.bullet,
                        minSpread = 0f,
                        maxSpread = 0f,
                        isCrit = RollCrit(),
                        owner = gameObject,
                        muzzleName = muzzleString,
                        smartCollision = true,
                        procChainMask = default,
                        procCoefficient = procCoefficient,
                        radius = 0.9f,
                        sniper = false,
                        stopperMask = LayerIndex.world.collisionMask,
                        //tracerEffectPrefab = JohnnyAssets.mistFinerZap,
                        weapon = null,
                        spreadPitchScale = 1f,
                        spreadYawScale = 1f,
                        queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                        hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.hitEffectPrefab,
                    }.Fire();

                    EffectData effectData = new EffectData
                    {
                        origin = aimRay.origin,
                        scale = 1f,
                        rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                    };
                    GameObject mistEffect = JohnnyAssets.mistFinerZap;
                    if (tier1) mistEffect = JohnnyAssets.mistFinerZap2;
                    if (tier2) mistEffect = JohnnyAssets.mistFinerZap3;
                    EffectManager.SpawnEffect(mistEffect, effectData, transmit: true);
                    Util.PlaySound("PlayMistFiner", gameObject);
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Stun;
        }
    }
}