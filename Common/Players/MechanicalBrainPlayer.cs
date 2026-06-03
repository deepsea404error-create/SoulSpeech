using System;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace SoulSpeech.Common.Players
{
    public class MechanicalBrainPlayer : ModPlayer
    {
        public bool mechanicalBrainEquipped;

        // 自定义冷却计数器，避免依赖 BrainOfConfusionBuff 作为判断依据
        private int mechBrainCooldown;

        public override void ResetEffects()
        {
            mechanicalBrainEquipped = false;
        }

        public override void PreUpdate()
        {
            if (mechBrainCooldown > 0)
                mechBrainCooldown--;
        }

        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            TryDodge(ref modifiers);
        }

        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            TryDodge(ref modifiers);
        }

        private void TryDodge(ref Player.HurtModifiers modifiers)
        {
            if (!mechanicalBrainEquipped)
                return;

            // 若同时装备混沌大脑，为其添加 1 帧 BrainOfConfusionBuff
            // 使原版混沌大脑的闪避检查（同样依赖该 Buff 作为冷却标记）被跳过
            for (int i = 3; i < 10 + Player.extraAccessorySlots; i++)
            {
                if (Player.armor[i]?.type == ItemID.BrainOfConfusion)
                {
                    Player.AddBuff(BuffID.BrainOfConfusionBuff, 1);
                    break;
                }
            }

            if (mechBrainCooldown > 0)
                return;

            if (Main.rand.NextBool(4))
            {
                modifiers.SetMaxDamage(0);
                mechBrainCooldown = 180; // 3 秒冷却
                Player.AddBuff(BuffID.BrainOfConfusionBuff, 180); // 视觉提示
                Player.BrainOfConfusionDodge();
            }
        }

        // 受击时对附近敌人施加混乱 Debuff（照搬原版混沌大脑逻辑）
        public override void OnHurt(Player.HurtInfo info)
        {
            if (!mechanicalBrainEquipped)
                return;

            if (Player.whoAmI != Main.myPlayer)
                return;

            int damage = info.Damage;

            // 触发几率：基础 3/5，每点伤害 +1/250，上限 100%
            float chance = 0.6f + damage / 250f;
            chance = Math.Min(1f, chance);

            if (Main.rand.NextFloat() >= chance)
                return;

            // 持续时间（帧）：随机取 [90+damage/3, 300+damage/2]
            int minDur = 90 + damage / 3;
            int maxDur = 300 + damage / 2;
            int duration = Main.rand.Next(minDur, maxDur + 1);

            // 影响范围（像素）
            float minRange, maxRange;

            if (damage <= 600)
                minRange = damage / 2f + 200f;
            else if (damage <= 1333)
                minRange = 0.75f * (damage / 2f) + 275f;
            else if (damage <= 2200)
                minRange = 0.375f * (damage / 2f) + 487.5f;
            else
                minRange = (3f / 32f) * (damage / 2f) + 796.875f;

            if (damage <= 100)
                maxRange = 2f * damage + 300f;
            else if (damage <= 233)
                maxRange = 1.5f * damage + 350f;
            else if (damage <= 500)
                maxRange = 0.75f * damage + 525f;
            else
                maxRange = (3f / 16f) * damage + 806.25f;

            float range = Main.rand.NextFloat(minRange, maxRange);

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.boss)
                    continue;

                if (Vector2.Distance(Player.Center, npc.Center) <= range)
                    npc.AddBuff(BuffID.Confused, duration);
            }
        }
    }
}
