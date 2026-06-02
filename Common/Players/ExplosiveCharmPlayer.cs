using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;
using SoulSpeech.Content.Projectiles;

namespace SoulSpeech.Common.Players
{
    internal class ExplosiveCharmPlayer : ModPlayer
    {
        public bool hasExplosiveCharm;

        public override void ResetEffects()
        {
            hasExplosiveCharm = false;
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (!hasExplosiveCharm)
                return;

            if (Player.whoAmI != Main.myPlayer)
                return;

            // ===== 生成爆炸弹幕 =====
            Projectile.NewProjectile(
                Player.GetSource_Accessory(Player.HeldItem),
                Player.Center,
                Vector2.Zero,
                ModContent.ProjectileType<ExplosiveBlast>(),
                50,
                6f,
                Player.whoAmI
            );

            // ===== 2 秒无敌 =====
            Player.immune = true;
            Player.immuneTime = 120;
        }
    }
}