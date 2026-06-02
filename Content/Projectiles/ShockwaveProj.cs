using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;

namespace SoulSpeech.Content.Projectiles
{
    internal class ShockwaveProj : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 20;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            // 强制使用纯白色绘制，完全不受环境光影响
            lightColor = Color.White;
            return true;
        }
        public override void AI()
        {
            // 扩散
            Projectile.scale += 0.16f;

            // 淡出
            Projectile.alpha += 5;

            float radius = Projectile.width * Projectile.scale;

            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;

                float distance = Vector2.Distance(npc.Center, Projectile.Center);

                if (distance <= radius && npc.immune[Projectile.owner] <= 0)
                {
                    int hitDirection = npc.Center.X > Projectile.Center.X ? 1 : -1;

                    NPC.HitInfo hit = npc.CalculateHitInfo(
                        Projectile.damage,
                        hitDirection,
                        false,          // 是否暴击
                        0f,             // 击退
                        Projectile.DamageType
                    );

                    npc.StrikeNPC(hit);

                    npc.immune[Projectile.owner] = 10;
                }
            }
        }
    }
}
