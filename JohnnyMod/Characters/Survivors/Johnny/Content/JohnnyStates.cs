using JohnnyMod.Survivors.Johnny.SkillStates;

namespace JohnnyMod.Survivors.Johnny
{
    public static class JohnnyStates
    {
        public static void Init()
        {
            Modules.Content.AddEntityState(typeof(SlashCombo));

            Modules.Content.AddEntityState(typeof(MistFiner));

            Modules.Content.AddEntityState(typeof(Roll));

            Modules.Content.AddEntityState(typeof(Deal));
        }
    }
}
