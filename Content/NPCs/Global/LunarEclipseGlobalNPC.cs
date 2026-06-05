using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.NPCs.Global
{
    // 月蚀 buff 的实际效果载体：复刻原版破晓（Daybroken）DoT。
    // 叠层（最多 8）由苍夜枪命中累加；每层 lifeRegen -= 200（lifeRegen 单位 0.5hp/帧 → 100hp/s/层），
    // 伤害数字 = 25 × 层数。与原版破晓“按插入的破晓之光数量计 DoT”等价（这里改用 buff 叠层计数）。
    public class LunarEclipseGlobalNPC : GlobalNPC
    {
        public const int MaxStacks = 8;

        public override bool InstancePerEntity => true;

        public bool lunarEclipseActive;
        public int lunarEclipseStacks;

        // 苍夜枪命中时调用：叠加一层（封顶 8）
        public void AddStack()
        {
            if (lunarEclipseStacks < MaxStacks)
                lunarEclipseStacks++;
        }

        public override void ResetEffects(NPC npc)
        {
            // 每帧先清标志；若 buff 仍在，LunarEclipseDebuff.Update 会在随后重新置位
            lunarEclipseActive = false;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            // buff 已结束：清空叠层，停止 DoT
            if (!lunarEclipseActive)
            {
                lunarEclipseStacks = 0;
                return;
            }

            int stacks = lunarEclipseStacks < 1 ? 1 : lunarEclipseStacks;

            // 抵消自然回血
            if (npc.lifeRegen > 0)
                npc.lifeRegen = 0;

            // 复刻破晓：每层 -200 lifeRegen → 每层 100hp/s，8 层 = 800hp/s
            npc.lifeRegen -= stacks * 200;

            // 伤害数字显示（每层 25）
            int dot = stacks * 25;
            if (damage < dot)
                damage = dot;
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (!lunarEclipseActive)
                return;

            // 蓝紫色粒子特效
            if (Main.rand.NextBool(2))
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.BlueTorch;
                int dust = Dust.NewDust(
                    npc.position, npc.width, npc.height,
                    dustType, 0f, 0f, 100, default, 1.4f
                );
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.4f;
                Main.dust[dust].velocity.Y -= 0.3f;
            }
        }
    }
}
