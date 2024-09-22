using RoR2;
using JohnnyMod.Modules.Achievements;

namespace JohnnyMod.Survivors.Johnny.Achievements
{
    //automatically creates language tokens "ACHIEVMENT_{identifier.ToUpper()}_NAME" and "ACHIEVMENT_{identifier.ToUpper()}_DESCRIPTION" 
    [RegisterAchievement(identifier, unlockableIdentifier, null, 10, null)]
    public class JohnnyMasteryAchievement : BaseMasteryAchievement
    {
        public const string identifier = JohnnySurvivor.Johnny_PREFIX + "masteryAchievement";
        public const string unlockableIdentifier = JohnnySurvivor.Johnny_PREFIX + "masteryUnlockable";

        public override string RequiredCharacterBody => JohnnySurvivor.instance.bodyName;

        //difficulty coeff 3 is monsoon. 3.5 is typhoon for grandmastery skins
        public override float RequiredDifficultyCoefficient => 3;
    }
}