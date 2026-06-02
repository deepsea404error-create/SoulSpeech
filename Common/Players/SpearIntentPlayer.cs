using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using SoulSpeech.Content.Projectiles;

namespace SoulSpeech.Common.Players
{
    internal class SpearIntentPlayer : ModPlayer
    {
        public bool hasSpearIntentScroll;
        private int spearIntentCooldown;

        public override void ResetEffects()
        {
            hasSpearIntentScroll = false;
        }

        public override void PostUpdate()
        {
            if (spearIntentCooldown > 0)
                spearIntentCooldown--;
        }

        public override void PostItemCheck()
        {
            if (!hasSpearIntentScroll)
                return;

            Item item = Player.HeldItem;

            // 玩家必须在使用物品
            if (!Player.ItemAnimationActive)
                return;

            // 只对长矛类武器生效
            if (item.DamageType != DamageClass.Melee || item.useStyle != ItemUseStyleID.Shoot)
                return;

            // 必须是“长矛弹幕”
            if (item.shoot <= ProjectileID.None)
                return;

            // 冷却
            if (spearIntentCooldown > 0)
                return;

            FireSpearIntent(item);
            spearIntentCooldown = 30; // 0.5 秒
        }

        private void FireSpearIntent(Item item)
        {
            Vector2 direction = (Main.MouseWorld - Player.Center).SafeNormalize(Vector2.UnitX);

            int damage = (int)(item.damage * 0.5f);

            Projectile.NewProjectile(
                Player.GetSource_FromThis(),
                Player.Center,
                direction * 15f,
                ModContent.ProjectileType<SpearIntentProj>(),
                damage,
                2f,
                Player.whoAmI
            );
        }
    }
}