using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace SoulSpeech.Content.Projectiles
{
    internal class SwordIntentProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Sword Intent");
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;

            Projectile.friendly = true;
            Projectile.hostile = false;

            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            //弹幕存在时间XX/60来计算
            Projectile.timeLeft = 600;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            // 旋转方向
            float rotationSpeed = Projectile.velocity.X < 0 ? -0.35f : 0.35f;
            Projectile.rotation += rotationSpeed;

            // 发光（偏黄色）
            Lighting.AddLight(Projectile.Center, 0.9f, 0.8f, 0.3f);

            // 粒子效果
            int dust = Dust.NewDust(
                Projectile.position,
                Projectile.width,
                Projectile.height,
                DustID.GoldFlame,
                Projectile.velocity.X * 0.1f,
                Projectile.velocity.Y * 0.1f,
                100,
                Color.Gold,
                1.8f
            );

            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity *= 0.3f;
            
        }
        //public override void OnKill(int timeLeft)
        //{
        //    int newDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 244, 0f, 0f, 100, default(Color), 2.5f);
        //    Main.dust[newDust].scale *= 1.5f;
        //    Main.dust[newDust].noGravity = true;
        //    Main.dust[newDust].velocity *= 1.5F;
        //}
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 12; i++)
            {
                int dust = Dust.NewDust(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.GoldFlame,
                    Main.rand.NextFloat(-2f, 2f),
                    Main.rand.NextFloat(-2f, 2f),
                    100,
                    Color.Gold,
                    2.2f
                );

                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 1.5f;
            }
        }
    }
}
