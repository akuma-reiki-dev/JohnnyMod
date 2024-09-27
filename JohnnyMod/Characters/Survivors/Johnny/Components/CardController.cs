using RoR2;
using RoR2.Orbs;
using RoR2.Projectile;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace JohnnyMod.Survivors.Johnny.Components
{
    public class CardController : NetworkBehaviour, IOnIncomingDamageServerReceiver, IProjectileImpactBehavior
    {
        public HealthComponent projectileHealthComponent;
        public JohnnyTensionController JohnnyStandee;

        private bool gravityStop = false;
        private bool gravityStarted = false;
        private bool startFuse = false;
        private float gravityCD = 0.5f;
        private float fuseTime = 0.1f;
        private float babyBoomFuse = 0.6f;
        private bool popBabies = false;
        private int boomCount = 0;
        private bool inAir = true;

        private DamageInfo dmgInfo = null;

        private ProjectileSimple projSimp;
        private Rigidbody rigidBody;

        public void OnIncomingDamageServer(DamageInfo damageInfo)
        {
            if (damageInfo.attacker &&
               (damageInfo.attacker.TryGetComponent<JohnnyTensionController>(out _) ||
                damageInfo.attacker.TryGetComponent<CardController>(out _)) && damageInfo.inflictor != this.gameObject)
            {
                PopCard(damageInfo);
            }
            else damageInfo.rejected = true;
        }

        private void FixedUpdate()
        {
            gravityCD -= Time.fixedDeltaTime;

            //Check for gravityStarted so we can turn this off as soon as it collides with something
            if(gravityCD <= 0 && !gravityStarted)
            {
                gravityStop = true;
            }

            if (gravityStop && !gravityStarted && !startFuse)
            {
                projSimp.desiredForwardSpeed = 0;
                rigidBody.velocity = Vector3.zero;
                rigidBody.isKinematic = false;
                rigidBody.mass = 1;
                rigidBody.useGravity = true;
                var quat = transform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(90, quat.y, quat.z);
                this.gravityStarted = true;
            }

            if (startFuse)
            {
                fuseTime -= Time.fixedDeltaTime;
            }

            if (popBabies)
            {
                babyBoomFuse -= Time.fixedDeltaTime;
            }

            if(fuseTime <= 0 && !popBabies)
            {
                Kaboom(dmgInfo);
            }
            
            if(babyBoomFuse <= 0 && popBabies)
            {
                BabyKaboom(dmgInfo);
            }
        }

        private void OnEnter()
        {
            gravityCD = 0.5f;
            gravityStarted = false;
            gravityStop = true;

            fuseTime = 0.1f;
            startFuse = false;
            dmgInfo = null;
            boomCount = 0;
            popBabies = false;

            inAir = true;

            //this.GetComponent<TeamFilter>().teamIndex = TeamIndex.Neutral;
            //disable gravity when we are initially spawned, will be re-enabled later
            rigidBody = this.GetComponent<Rigidbody>();
            rigidBody.useGravity = false;
            rigidBody.mass = 0;

            projSimp = this.GetComponent<ProjectileSimple>();

            this.GetComponent<TeamComponent>().teamIndex = TeamIndex.Neutral;
            this.GetComponent<TeamFilter>().teamIndex = TeamIndex.Neutral;
        }

        private void Start()
        {
            rigidBody = this.GetComponent<Rigidbody>();
            projSimp = this.GetComponent<ProjectileSimple>();
        }

        public void PopCard(DamageInfo damageInfo)
        {
            if (damageInfo.attacker && dmgInfo == null)
            {
                dmgInfo = damageInfo;
                startFuse = true;
            }
        }

        public void Kaboom(DamageInfo damageInfo)
        {
            float dmgMult = inAir ? 1.75f : 1.25f;

            popBabies = true;
            BlastAttack explode = new BlastAttack();
            explode.baseDamage = damageInfo.damage * dmgMult;
            explode.radius = 10;
            explode.crit = damageInfo.crit;
            explode.procCoefficient = damageInfo.procCoefficient;
            explode.attacker = damageInfo.attacker;
            explode.inflictor = base.gameObject;
            explode.damageType = damageInfo.damageType;
            explode.damageColorIndex = DamageColorIndex.WeakPoint;
            explode.teamIndex = damageInfo.attacker.GetComponent<TeamComponent>().teamIndex;
            explode.procChainMask = damageInfo.procChainMask;
            explode.falloffModel = BlastAttack.FalloffModel.Linear;

            explode.position = transform.position;

            explode.Fire();

            //make the card stop so the baby pops dont fall down :pensive:
            rigidBody.velocity = Vector3.zero;
            rigidBody.mass = 0;
            rigidBody.useGravity = false;

            EffectData effectData = new EffectData
            {
                origin = transform.position,
                scale = 1f
            };
            EffectManager.SpawnEffect(JohnnyAssets.cardPopEffect, effectData, transmit: true);
            Util.PlaySound("PlayCardPop", gameObject);
        }

        public void BabyKaboom(DamageInfo damageInfo)
        {
            BlastAttack explode = new BlastAttack();
            explode.baseDamage = damageInfo.damage * 0.025f;
            explode.radius = 10f;
            explode.crit = damageInfo.crit;
            explode.procCoefficient = 1;
            explode.attacker = damageInfo.attacker;
            explode.inflictor = base.gameObject;
            explode.damageType = damageInfo.damageType;
            explode.damageColorIndex = DamageColorIndex.WeakPoint;
            explode.teamIndex = damageInfo.attacker.GetComponent<TeamComponent>().teamIndex;
            explode.procChainMask = damageInfo.procChainMask;
            explode.falloffModel = BlastAttack.FalloffModel.None;

            explode.position = transform.position;

            explode.Fire();

            boomCount++;
            babyBoomFuse = 0.05f;

            if (boomCount > 10)
            {
                Destroy(base.gameObject);
            }
        }

        public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        {
            inAir = false;
        }
    }
}