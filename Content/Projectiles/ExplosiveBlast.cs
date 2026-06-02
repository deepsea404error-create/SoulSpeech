using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace SoulSpeech.Content.Projectiles
{
    internal class ExplosiveBlast : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 160;
            Projectile.height = 160;

            Projectile.friendly = true;
            Projectile.hostile = false;

            Projectile.DamageType = DamageClass.Melee;
            Projectile.damage = 50;
            Projectile.knockBack = 6f;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 2; // 极短存在时间
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.hide = true; // 不显示贴图
        }
        public override void AI()
        {
            int dustCount = Main.rand.Next(50, 80); // 粒子数量
            for (int i = 0; i < dustCount; i++)
            {
                int[] dustTypes = new int[] { DustID.Smoke, DustID.Torch, DustID.FireflyHit, DustID.GoldFlame };
                int dustType = dustTypes[Main.rand.Next(dustTypes.Length)];

                float angle = Main.rand.NextFloat(0, MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(2f, 8f);
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;

                int dustIndex = Dust.NewDust(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    dustType,
                    velocity.X,
                    velocity.Y,
                    100,
                    Color.White,
                    Main.rand.NextFloat(1.2f, 2.5f)
                );

                Dust dust = Main.dust[dustIndex];
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
                dust.alpha = 100;
            }
        }
        //public override void AI()
        //{
        //    // 爆炸白色粒子
        //    for (int i = 0; i < 20; i++)
        //    {
        //        int dust = Dust.NewDust(
        //            Projectile.position,
        //            Projectile.width,
        //            Projectile.height,
        //            DustID.Smoke,
        //            Main.rand.NextFloat(-3f, 3f),
        //            Main.rand.NextFloat(-3f, 3f),
        //            100,
        //            Color.White,
        //            1.8f
        //        );

        //        Main.dust[dust].noGravity = true;
        //    }
        //}
    }
}