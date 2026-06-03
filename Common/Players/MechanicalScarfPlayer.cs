using Terraria.ModLoader;
using Terraria;
using Terraria.ID;

namespace SoulSpeech.Common.Players
{
    public class MechanicalScarfPlayer : ModPlayer
    {
        public bool hasMechanicalScarf;

        public override void ResetEffects()
        {
            hasMechanicalScarf = false;
        }

        // 若同时装备了蠕虫围巾，抵消其 17% 减伤，防止双重叠加
        public override void PostUpdateEquips()
        {
            if (!hasMechanicalScarf) return;

            for (int i = 3; i < 10 + Player.extraAccessorySlots; i++)
            {
                if (Player.armor[i]?.type == ItemID.WormScarf)
                {
                    Player.endurance -= 0.17f;
                    break;
                }
            }
        }
    }
}
