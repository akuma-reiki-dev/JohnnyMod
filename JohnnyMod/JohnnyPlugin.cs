using BepInEx;
using JohnnyMod.Survivors.Johnny;
using JohnnyMod.Survivors.Johnny.Components;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine.Networking;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

//rename this namespace
namespace JohnnyMod
{
    //[BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin(MODUID, MODNAME, MODVERSION)]
    public class JohnnyPlugin : BaseUnityPlugin
    {
        // if you do not change this, you are giving permission to deprecate the mod-
        //  please change the names to your own stuff, thanks
        //   this shouldn't even have to be said
        public const string MODUID = "com.HasteReapr.JohnnyMod";
        public const string MODNAME = "JohnnyMod";
        public const string MODVERSION = "0.0.1";

        // a prefix for name tokens to prevent conflicts- please capitalize all name tokens for convention
        public const string DEVELOPER_PREFIX = "HASTEREAPR";

        public static JohnnyPlugin instance;

        void Awake()
        {
            instance = this;

            //easy to use logger
            Log.Init(Logger);

            // used when you want to properly set up language folders
            Modules.Language.Init();

            // character initialization
            new JohnnySurvivor().Initialize();

            //hooks into various methods
            Hook();

            // make a content pack and add it. this has to be last
            new Modules.ContentPacks().Initialize();
        }

        private void Hook()
        {
            On.RoR2.MapZone.TryZoneStart += MapZone_TryZoneStart;
            //Run.onClientGameOverGlobal += Run_onClientGameOverGlobal;
            On.RoR2.Run.OnClientGameOver += Run_OnClientGameOver;
        }

        private void Run_OnClientGameOver(On.RoR2.Run.orig_OnClientGameOver orig, Run self, RunReport runReport)
        {
            orig(self, runReport);
            try
            {
                if (NetworkServer.active)
                {
                    //dont jumpscare me please
                    Util.PlaySound("PlayWinVoice", self.gameObject);
                }
            }
            catch (System.Exception)
            {
                Log.Error("Had issue with RunOnClientGameOver call. But seeing this means the vanilla version ran.");
            }
        }

        private void Run_onClientGameOverGlobal(Run arg1, RunReport arg2)
        {
            bool isJohgn = false;
            for (int x = 0; x < arg2.playerInfoCount; x++)
            {
                Log.Message("Scanning for Johgnny");
                if (arg2.playerInfos[x].bodyName.Equals("JohnnyBody"))
                    isJohgn = true;
            }



            if (isJohgn)
            {
                if (arg2.gameEnding.isWin)
                {
                    Util.PlaySound("PlayWinVoice", arg1.gameObject);
                }
                else
                {
                    Util.PlaySound("PlayLostVoice", arg1.gameObject);
                }
            }
            Log.Message("Trying to play the win voice");
            Util.PlaySound("PlayWinVoice", arg1.gameObject);

            if (arg2.gameEnding.isWin)
            {
                Util.PlaySound("PlayWinVoice", arg1.gameObject);
            }
            else
            {
                Util.PlaySound("PlayLostVoice", arg1.gameObject);
            }
        }

        private void MapZone_TryZoneStart(On.RoR2.MapZone.orig_TryZoneStart orig, MapZone self, UnityEngine.Collider other)
        {
            // if we have the card component get out of this method and dont kys
            if (other.GetComponent<CardController>() && other.GetComponent<TeamComponent>().teamIndex != TeamIndex.Player)
            {
                return;
            }

            orig(self, other);
        }
    }
}
