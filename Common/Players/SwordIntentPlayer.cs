using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;
using SoulSpeech.Content.Projectiles;
using Terraria.ID;

namespace SoulSpeech.Common.Players
{
    public class SwordIntentPlayer : ModPlayer
    {
        public bool hasSwordIntentScroll;

        // 剑意弹幕冷却（单位：tick，60 tick = 1 秒）
        private int swordIntentCooldown;

        public override void ResetEffects()
        {
            hasSwordIntentScroll = false;
        }

        public override void PostUpdate()
        {
            // 冷却递减
            if (swordIntentCooldown > 0)
                swordIntentCooldown--;
        }

        public override void PostItemCheck()
        {
            if (!hasSwordIntentScroll)
                return;

            Item item = Player.HeldItem;

            // 玩家是否正在使用物品
            if (!Player.ItemAnimationActive)
                return;

            // 只对挥砍类近战生效
            if (item.DamageType != DamageClass.Melee || item.useStyle != ItemUseStyleID.Swing)
                return;

            // 冷却中则不触发
            if (swordIntentCooldown > 0)
                return;

            FireSwordIntent(item);

            // 设置冷却（示例：每 25 tick 触发一次）
            swordIntentCooldown = 30;
        }

        private void FireSwordIntent(Item item)
        {
            Vector2 direction = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.UnitX);

            int damage = (int)(item.damage * 0.5f);

            Projectile.NewProjectile(
                Player.GetSource_FromThis(),
                Player.Center,
                direction * 18f,
                ModContent.ProjectileType<SwordIntentProj>(),
                damage,
                2f,
                Player.whoAmI
            );
        }
    }
}
