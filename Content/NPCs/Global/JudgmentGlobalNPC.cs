using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.NPCs.Global
{
    // 裁决 buff 的实际效果载体：每秒流失 50hp 的 DoT + 金色粒子特效。
    public class JudgmentGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool judgmentActive;

        public override void ResetEffects(NPC npc)
        {
            judgmentActive = false;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (!judgmentActive)
                return;

            // 抵消自然回血
            if (npc.lifeRegen > 0)
                npc.lifeRegen = 0;

            // lifeRegen 单位为 0.5hp/帧 → -100 即每秒 -50hp
            npc.lifeRegen -= 100;

            // 伤害数字显示（每秒 50）
            if (damage < 50)
                damage = 50;
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (!judgmentActive)
                return;

            // 金色粒子特效 (金色尘 57)
            if (Main.rand.NextBool(3))
            {
                int dust = Dust.NewDust(
                    npc.position, npc.width, npc.height,
                    57, 0f, 0f, 100, default, 1.1f
                );
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.3f;
            }
        }
    }
}
