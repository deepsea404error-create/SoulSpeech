using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace SoulSpeech.Common.Players
{
    public class MechanicalBrainPlayer : ModPlayer
    {
        // 是否装备了机械大脑
        public bool mechanicalBrainEquipped;

        public override void ResetEffects()
        {
            mechanicalBrainEquipped = false;
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
            // 没装备机械大脑，直接跳过
            if (!mechanicalBrainEquipped)
                return;

            // 原版“控脑术”Buff，存在时不能再次闪避
            if (Player.HasBuff(BuffID.BrainOfConfusionBuff))
                return;

            // 1/4 概率触发（25%）
            if (Main.rand.NextBool(4))
            {
                // 完全免疫本次伤害
                modifiers.SetMaxDamage(0);

                // 添加控脑术 Buff（4 秒）
                Player.AddBuff(BuffID.BrainOfConfusionBuff, 240);

                // 可选：添加闪避音效
                Player.BrainOfConfusionDodge();

            }
        }
    }
}
