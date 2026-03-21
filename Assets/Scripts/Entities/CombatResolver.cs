using PathOfPower.Data;
using PathOfPower.Upgrades;

namespace PathOfPower.Entities
{
    public struct CombatResult
    {
        public bool playerWon;
        public float newPower;
        public float powerGained;
        public bool shieldUsed;
        public string resultMessage;
        public int goldGained;
    }

    public static class CombatResolver
    {
        /// <summary>
        /// Resolves player interaction with a dungeon node.
        /// Returns a CombatResult describing what happened and the new player power.
        /// </summary>
        public static CombatResult Resolve(float playerPower, NodeData node, AbilitySystem abilities)
        {
            var result = new CombatResult
            {
                playerWon = true,
                newPower = playerPower,
                powerGained = 0f,
                shieldUsed = false,
                goldGained = 0,
                resultMessage = ""
            };

            switch (node.type)
            {
                case NodeType.Enemy:
                case NodeType.Boss:
                    ResolveEnemy(ref result, playerPower, node, abilities);
                    break;

                case NodeType.Buff:
                    float buffGain = node.power * (1f + abilities.BonusGainRate);
                    result.powerGained = buffGain;
                    result.newPower = playerPower + buffGain;
                    result.resultMessage = $"+{buffGain:F0} Power!";
                    break;

                case NodeType.Multiplier:
                    float before = playerPower;
                    result.newPower = playerPower * node.power;
                    result.powerGained = result.newPower - before;
                    result.resultMessage = $"x{node.power} Power!";
                    break;

                case NodeType.Trap:
                    // node.power < 1 = percentage reduction; node.power >= 1 = flat reduction
                    float trapLoss;
                    if (node.power < 1f)
                        trapLoss = playerPower * node.power;
                    else
                        trapLoss = node.power;

                    result.newPower = System.Math.Max(0.1f, playerPower - trapLoss);
                    result.powerGained = -(playerPower - result.newPower);
                    result.resultMessage = $"Trap! -{trapLoss:F0} Power";
                    break;

                case NodeType.Chest:
                    int baseGold = (int)node.power;
                    result.goldGained = (int)(baseGold * abilities.GreedMultiplier);
                    result.resultMessage = $"+{result.goldGained} Gold!";
                    break;

                case NodeType.Start:
                case NodeType.Exit:
                    result.resultMessage = "Move";
                    break;
            }

            return result;
        }

        private static void ResolveEnemy(ref CombatResult result, float playerPower, NodeData node, AbilitySystem abilities)
        {
            if (playerPower > node.power)
            {
                // Victory — absorb enemy power
                float absorbed = node.power;

                // Vampirism: gain extra % on top
                if (abilities.VampirismRate > 0f)
                    absorbed *= (1f + abilities.VampirismRate);

                // Overkill: gain a portion of margin
                float margin = playerPower - node.power;
                if (abilities.OverkillRate > 0f)
                    absorbed += margin * abilities.OverkillRate;

                // Apply bonus gain rate to absorption
                absorbed *= (1f + abilities.BonusGainRate);

                result.playerWon = true;
                result.powerGained = absorbed;
                result.newPower = playerPower + absorbed;

                string enemyLabel = node.type == NodeType.Boss ? "BOSS" : node.enemyType.ToString();
                result.resultMessage = $"Defeated {enemyLabel}! +{absorbed:F0}";
            }
            else
            {
                // Defeat — check shield
                if (abilities.ShieldCharges > 0)
                {
                    abilities.UseShield();
                    result.playerWon = true;
                    result.shieldUsed = true;
                    result.newPower = 1f;
                    result.powerGained = 0f;
                    result.resultMessage = "Shield saved you! Power = 1";
                }
                else
                {
                    result.playerWon = false;
                    result.newPower = 0f;
                    result.resultMessage = $"Defeated by {node.enemyType}!";
                }
            }
        }
    }
}
