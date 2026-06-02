using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;

namespace SoulSpeech.Content.Projectiles
{
    internal class DemonSlayerProj : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;

            Projectile.friendly = true;   // 现在是真正攻击弹幕
            Projectile.hostile = false;

            Projectile.DamageType = DamageClass.Melee;
            Projectile.damage = 0; // 实际伤害由武器决定（可后续传入）

            Projectile.timeLeft = 20;     // 存在时间短
            Projectile.penetrate = -1;    // 无限穿透

            Projectile.tileCollide = false; // 穿墙
            Projectile.ignoreWater = true;

            Projectile.light = 0.9f; // 自带亮度
        }

        public override void AI()
        {
            Projectile.spriteDirection = Projectile.direction;

            // 旋转跟随飞行方向
            Projectile.rotation = Projectile.velocity.ToRotation();

            // ===== 渐隐处理 =====
            int maxTime = 20; // 要和 SetDefaults 里的 timeLeft 一致
            Projectile.alpha = (int)(255f * (1f - Projectile.timeLeft / (float)maxTime));

            // 当朝左时，额外修正旋转（关键）
            if (Projectile.spriteDirection == -1)
            {
                Projectile.rotation += MathHelper.Pi;
            }

            // 白色粒子
            if (Main.rand.NextBool(2))
            {
                int dust = Dust.NewDust(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.WhiteTorch,
                    0f, 0f, 100,
                    Color.White,
                    1.6f
                );

                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.1f;
            }
        }

        // 不受环境光影响
        public override bool PreDraw(ref Color lightColor)
        {
            lightColor = Color.White;
            return true;
        }
    }
}