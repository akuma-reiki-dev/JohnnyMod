using RoR2;
using RoR2.Orbs;
using RoR2.Projectile;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace JohnnyMod.Survivors.Johnny.Components
{
    public class CardController : NetworkBehaviour, IOnIncomingDamageServerReceiver
    {
        public HealthComponent projectileHealthComponent;
        public ProjectileController controller;
        public JohnnyStanceComponent JohnnyStandee;

        private DamageInfo damageInfo;

        private bool gravityStop = false;
        private bool gravityStarted = false;
        private float gravityCD = 2;

        public void OnIncomingDamageServer(DamageInfo damageInfo)
        {
            if (damageInfo.attacker &&
               (damageInfo.attacker.TryGetComponent<JohnnyStanceComponent>(out _) ||
                damageInfo.attacker.TryGetComponent<CardController>(out _)))
            {
                RicochetBullet(damageInfo);
            }
            else damageInfo.rejected = true;
        }

        private void FixedUpdate()
        {
            gravityCD -= Time.fixedDeltaTime;

            if(gravityCD <= 0)
            {
                gravityStop = true;
            }

            if (gravityStop && !gravityStarted)
            {
                this.GetComponent<Rigidbody>().velocity = Vector3.zero;
                this.GetComponent<Rigidbody>().useGravity = true;
                this.gravityStarted = true;
            }
        }

        private void OnEnter()
        {
            gravityCD = 2;
            gravityStarted = false;
            gravityStop = true;

            this.GetComponent<TeamFilter>().teamIndex = TeamIndex.Neutral;
        }

        public void RicochetBullet(DamageInfo damageInfo)
        {
            if (this.damageInfo != null)
            {
                this.damageInfo.damage += damageInfo.damage * 0.5f;
                return;
            }
            this.damageInfo = damageInfo;
            this.damageInfo.procCoefficient = 0f;
            this.damageInfo.damageColorIndex = DamageColorIndex.WeakPoint;

            BlastAttack explode = new BlastAttack();
            explode.baseDamage = damageInfo.damage;
            explode.radius = 20f;
            explode.crit = damageInfo.crit;
            explode.procCoefficient = damageInfo.procCoefficient;
            explode.attacker = damageInfo.attacker;
            explode.damageType = damageInfo.damageType;
            explode.damageColorIndex = DamageColorIndex.WeakPoint;
            explode.procChainMask = damageInfo.procChainMask;

            explode.position = this.transform.position;

            explode.Fire();
        }
    }
}