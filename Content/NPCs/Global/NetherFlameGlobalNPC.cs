using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.NPCs.Global
{
    // 冥火 buff 的实际效果载体：每秒流失 50hp 的 DoT + 防御清零 + 移速 ×0.75(Boss 免疫) + 暗紫色粒子特效。
    public class NetherFlameGlobalNPC : GlobalNPC
    {
        private const float SpeedMultiplier = 0.75f;  // 移速 ×0.75

        public override bool InstancePerEntity => true;

        public bool netherFlameActive;

        public override void ResetEffects(NPC npc)
        {
            netherFlameActive = false;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (!netherFlameActive)
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

        // 防御清零：Defense.Base 已注入敌怪防御，乘以 0 使乘数归零 → 有效防御 = 0（StatModifier 乘法为减益）
        public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
        {
            if (!netherFlameActive)
                return;

            modifiers.Defense *= 0f;
        }

        // 移速 ×0.75：在 AI 之后对当帧速度缩放（AI 每帧重新驱动速度，故为稳定 0.75 倍而非衰减到 0）。
        // 对 Boss 及按 Boss 计算的怪（多段 Boss 的躯体/部件）无效。
        public override void PostAI(NPC npc)
        {
            if (!netherFlameActive)
                return;

            if (npc.boss || NPCID.Sets.ShouldBeCountedAsBoss[npc.type])
                return;

            npc.velocity *= SpeedMultiplier;
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (!netherFlameActive)
                return;

            // 暗紫色粒子特效（暗影火焰 Shadowflame）
            if (Main.rand.NextBool(3))
            {
                int dust = Dust.NewDust(
                    npc.position, npc.width, npc.height,
                    DustID.Shadowflame, 0f, 0f, 100, default, 1.1f
                );
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.3f;
            }
        }
    }
}
