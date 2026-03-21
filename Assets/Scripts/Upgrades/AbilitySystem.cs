using PathOfPower.Core;

namespace PathOfPower.Upgrades
{
    /// <summary>
    /// Holds all ability/passive values for a single run, derived from meta upgrades.
    /// Pure C# — no Unity dependencies.
    /// </summary>
    public class AbilitySystem
    {
        public float OverkillRate    { get; private set; }   // bonus % of overkill margin
        public float VampirismRate   { get; private set; }   // bonus % of enemy power absorbed
        public float BonusGainRate   { get; private set; }   // % bonus to all power gains
        public float GreedMultiplier { get; private set; }   // gold multiplier from chests
        public int   ShieldCharges   { get; private set; }   // remaining shield uses this run

        public AbilitySystem(PlayerData data)
        {
            OverkillRate    = GetUpgradeValue(data, "overkill");
            VampirismRate   = GetUpgradeValue(data, "vampirism");
            BonusGainRate   = GetUpgradeValue(data, "bonus_gains");
            GreedMultiplier = 1f + GetUpgradeValue(data, "greed") - 1f; // greed values are multipliers
            ShieldCharges   = data.HasUpgrade("iron_will") ? 1 : 0;

            // Normalize greed: stored as 2, 3, 5 etc (raw multiplier)
            // Upgrade values are the multiplier itself, not additive
            float greedVal = GetRawUpgradeValue(data, "greed");
            GreedMultiplier = greedVal > 0f ? greedVal : 1f;
        }

        public void UseShield()
        {
            if (ShieldCharges > 0) ShieldCharges--;
        }

        private static float GetUpgradeValue(PlayerData data, string id)
        {
            int level = data.GetUpgradeLevel(id);
            if (level <= 0) return 0f;

            // Values are defined in upgrades.json — we look them up via UpgradeManager
            // Since AbilitySystem is constructed before UpgradeManager finishes loading,
            // we delegate lookups through a static accessor
            return UpgradeManager.GetValueForLevel(id, level);
        }

        private static float GetRawUpgradeValue(PlayerData data, string id)
        {
            int level = data.GetUpgradeLevel(id);
            if (level <= 0) return 0f;
            return UpgradeManager.GetValueForLevel(id, level);
        }
    }
}
