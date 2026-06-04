using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Projectiles
{
    // 天羽射弹：天羽书发射的法师弹。沿用原版鸟妖羽毛（HarpyFeather / 射弹 38）的贴图与手感。
    // 不穿透、不穿墙；起手直飞，稍后开始轻微下坠形成羽落弧线（仿原版 aiStyle 1）。
    internal class SkyFeatherProj : ModProjectile
    {
        private const float GravityDelay = 15f; // 起手直飞帧数，之后开始下坠
        private const float Gravity = 0.1f;      // 每帧下坠加速度
        private const float MaxFallSpeed = 16f;  // 下坠速度封顶

        public override void SetDefaults()
        {
            // 与原版鸟妖羽毛一致的尺寸
            Projectile.width = 14;
            Projectile.height = 14;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;

            Projectile.penetrate = 1;       // 不穿透
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;  // 不穿墙
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            // 起手直飞，GravityDelay 帧后开始轻微下坠（仿原版 HarpyFeather 的羽落手感）
            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] >= GravityDelay)
            {
                Projectile.ai[0] = GravityDelay;
                Projectile.velocity.Y += Gravity;
                if (Projectile.velocity.Y > MaxFallSpeed)
                    Projectile.velocity.Y = MaxFallSpeed;
            }

            // 贴图弹尖朝上，需 +90° 才能对齐飞行方向（与原版射弹 38 的旋转一致）
            if (Projectile.velocity != Vector2.Zero)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // 柔和的白色云絮拖尾，营造羽毛飘散的粒子感
            if (Main.rand.NextBool(2))
            {
                int dust = Dust.NewDust(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.Cloud, 0f, 0f, 120, default, 0.9f
                );
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.2f;
            }

            // 淡色光照
            Lighting.AddLight(Projectile.Center, 0.3f, 0.3f, 0.4f);
        }
    }
}
