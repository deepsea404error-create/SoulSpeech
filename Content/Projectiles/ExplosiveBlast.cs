using System;
using Microsoft.Xna.Framework;
using Terraria.Audio;
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
            Projectile.timeLeft = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.hide = true;
        }

        public override void AI()
        {
            if (Projectile.timeLeft == 2)
            {
                SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

                // Gore碎片
                for (int i = 0; i < 5; i++)
                {
                    Vector2 goreVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 7f);
                    Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, goreVel, GoreID.Smoke1 + Main.rand.Next(3));
                }
            }

            // 内圈：高速火焰粒子
            int fireCount = Main.rand.Next(30, 50);
            for (int i = 0; i < fireCount; i++)
            {
                float angle = Main.rand.NextFloat(0, MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(4f, 12f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;

                int fire = Dust.NewDust(
                    Projectile.Center - new Vector2(20),
                    40, 40,
                    DustID.Torch,
                    vel.X, vel.Y,
                    0, default,
                    Main.rand.NextFloat(1.8f, 3.5f)
                );
                Main.dust[fire].noGravity = true;
            }

            // 外圈：慢速烟雾粒子（受重力，自然飘散）
            int smokeCount = Main.rand.Next(20, 35);
            for (int i = 0; i < smokeCount; i++)
            {
                float angle = Main.rand.NextFloat(0, MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(1f, 4f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;

                int smoke = Dust.NewDust(
                    Projectile.position,
                    Projectile.width, Projectile.height,
                    DustID.Smoke,
                    vel.X, vel.Y,
                    150, default,
                    Main.rand.NextFloat(1f, 2f)
                );
                Main.dust[smoke].noGravity = false;
            }
        }
    }
}