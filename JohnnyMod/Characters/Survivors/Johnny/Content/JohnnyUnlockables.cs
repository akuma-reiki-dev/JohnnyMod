using JohnnyMod.Survivors.Johnny.Achievements;
using RoR2;
using UnityEngine;

namespace JohnnyMod.Survivors.Johnny
{
    public static class JohnnyUnlockables
    {
        public static UnlockableDef characterUnlockableDef = null;
        public static UnlockableDef masterySkinUnlockableDef = null;

        public static void Init()
        {
            masterySkinUnlockableDef = Modules.Content.CreateAndAddUnlockbleDef(
                JohnnyMasteryAchievement.unlockableIdentifier,
                Modules.Tokens.GetAchievementNameToken(JohnnyMasteryAchievement.identifier),
                JohnnySurvivor.instance.assetBundle.LoadAsset<Sprite>("texMasteryAchievement"));
        }
    }
}
