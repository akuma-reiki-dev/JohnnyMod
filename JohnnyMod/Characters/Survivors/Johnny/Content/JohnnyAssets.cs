using RoR2;
using UnityEngine;
using JohnnyMod.Modules;
using System;
using RoR2.Projectile;
using R2API;
using UnityEngine.Networking;
using JohnnyMod.Survivors.Johnny.Components;

namespace JohnnyMod.Survivors.Johnny
{
    public static class JohnnyAssets
    {
        // particle effects
        public static GameObject swordSwingEffect;
        public static GameObject swordHitImpactEffect;

        public static GameObject bombExplosionEffect;

        //projectiles
        public static GameObject bombProjectilePrefab;

        public static GameObject cardProjectile;
        public static GameObject blackCardProjectile;

        private static AssetBundle _assetBundle;

        internal static Shader hotpoo = Resources.Load<Shader>("Shaders/Deferred/HGStandard");

        public static void Init(AssetBundle assetBundle)
        {

            _assetBundle = assetBundle;

            CreateEffects();

            CreateProjectiles();
        }

        #region effects
        private static void CreateEffects()
        {
            CreateBombExplosionEffect();

            swordSwingEffect = _assetBundle.LoadEffect("JohnnySwordSwingEffect", true);
            swordHitImpactEffect = _assetBundle.LoadEffect("ImpactJohnnySlash");
        }

        private static void CreateBombExplosionEffect()
        {
            bombExplosionEffect = _assetBundle.LoadEffect("BombExplosionEffect", "JohnnyBombExplosion");

            if (!bombExplosionEffect)
                return;

            ShakeEmitter shakeEmitter = bombExplosionEffect.AddComponent<ShakeEmitter>();
            shakeEmitter.amplitudeTimeDecay = true;
            shakeEmitter.duration = 0.5f;
            shakeEmitter.radius = 200f;
            shakeEmitter.scaleShakeRadiusWithLocalScale = false;

            shakeEmitter.wave = new Wave
            {
                amplitude = 1f,
                frequency = 40f,
                cycleOffset = 0f
            };

        }
        #endregion effects

        #region projectiles
        private static void CreateProjectiles()
        {
            CreateCardProjectile();
            Content.AddProjectilePrefab(cardProjectile);

            CreateBlackCard();
            Content.AddProjectilePrefab(blackCardProjectile);
        }

        private static void CreateCardProjectile()
        {
            cardProjectile = _assetBundle.LoadAsset<GameObject>("JohnCardWhite").InstantiateClone("JohnnyWhiteCardProj", true);
            cardProjectile.AddComponent<CardController>();

            var cardController = cardProjectile.AddComponent<CardController>();
            cardController.projectileHealthComponent = cardProjectile.GetComponent<HealthComponent>();
            cardController.controller = cardProjectile.GetComponent<ProjectileController>();

            cardProjectile.GetComponent<HurtBox>().hurtBoxGroup = cardProjectile.GetComponent<HurtBoxGroup>();

            cardProjectile.GetComponent<HurtBoxGroup>().bullseyeCount = 1;
        }

        private static void CreateBlackCard()
        {
            blackCardProjectile = _assetBundle.LoadAsset<GameObject>("JohnCardBlack").InstantiateClone("JohnnyBlackCardProj", true);
            //we disable gravity for when its initially spawned.
            blackCardProjectile.GetComponent<Rigidbody>().useGravity = false;

            blackCardProjectile.AddComponent<CardController>();

            var cardController = blackCardProjectile.AddComponent<CardController>();
            cardController.projectileHealthComponent = blackCardProjectile.GetComponent<HealthComponent>();
            cardController.controller = blackCardProjectile.GetComponent<ProjectileController>();

            blackCardProjectile.GetComponent<HurtBox>().hurtBoxGroup = blackCardProjectile.GetComponent<HurtBoxGroup>();

            blackCardProjectile.GetComponent<HurtBoxGroup>().bullseyeCount = 1;
        }

        private static void CreateBombProjectile()
        {
            //highly recommend setting up projectiles in editor, but this is a quick and dirty way to prototype if you want
            bombProjectilePrefab = Asset.CloneProjectilePrefab("CommandoGrenadeProjectile", "JohnnyBombProjectile");

            //remove their ProjectileImpactExplosion component and start from default values
            UnityEngine.Object.Destroy(bombProjectilePrefab.GetComponent<ProjectileImpactExplosion>());
            ProjectileImpactExplosion bombImpactExplosion = bombProjectilePrefab.AddComponent<ProjectileImpactExplosion>();
            
            bombImpactExplosion.blastRadius = 16f;
            bombImpactExplosion.blastDamageCoefficient = 1f;
            bombImpactExplosion.falloffModel = BlastAttack.FalloffModel.None;
            bombImpactExplosion.destroyOnEnemy = true;
            bombImpactExplosion.lifetime = 12f;
            bombImpactExplosion.impactEffect = bombExplosionEffect;
            bombImpactExplosion.lifetimeExpiredSound = Content.CreateAndAddNetworkSoundEventDef("JohnnyBombExplosion");
            bombImpactExplosion.timerAfterImpact = true;
            bombImpactExplosion.lifetimeAfterImpact = 0.1f;

            ProjectileController bombController = bombProjectilePrefab.GetComponent<ProjectileController>();

            if (_assetBundle.LoadAsset<GameObject>("JohnnyBombGhost") != null)
                bombController.ghostPrefab = _assetBundle.CreateProjectileGhostPrefab("JohnnyBombGhost");
            
            bombController.startSound = "";
        }
        #endregion projectiles

        #region depends
        private static GameObject CreateGhostPrefab(string ghostName)
        {
            GameObject ghostPrefab = _assetBundle.LoadAsset<GameObject>(ghostName).InstantiateClone(ghostName);
            ghostPrefab.AddComponent<NetworkIdentity>();
            ghostPrefab.AddComponent<ProjectileGhostController>();

            ConvertAllRenderersToHopooShader(ghostPrefab);

            return ghostPrefab;
        }

        internal static void ConvertAllRenderersToHopooShader(GameObject objectToConvert)
        {
            foreach (Renderer i in objectToConvert.GetComponentsInChildren<Renderer>())
            {
                if (i)
                {
                    if (i.material)
                    {
                        i.material.shader = hotpoo;
                    }
                }
            }
        }
        #endregion
    }
}
