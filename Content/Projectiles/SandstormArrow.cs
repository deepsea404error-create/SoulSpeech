using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Projectiles
{
    // 沙暴矢：直飞箭矢，命中敌人在命中点产生 2 秒沙暴（仿原版远古风暴友方版，可悬空）。
    internal class SandstormArrow : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.arrow = true;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;

            Projectile.penetrate = 3;       // 穿透 3
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;

            Projectile.timeLeft = 600;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;

            // Projectile.aiStyle = 1; // 仿原版箭矢 AI（重力+旋转+粘墙）
        }

        public override void AI()
        {
            //旋转：贴图为纵向、箭头朝下 -Y，需 -90° 让其对齐飞行方向
            if (Projectile.velocity != Vector2.Zero)
                Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            // 沙黄色拖尾
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 8f),
                    DustID.Sand,
                    Vector2.Zero, 100, default, 0.8f
                );
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.myPlayer != Projectile.owner)
                return;

            // 在命中点产生 2 秒沙暴
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                Vector2.Zero,
                ModContent.ProjectileType<Sandstorm>(),
                Projectile.damage,
                Projectile.knockBack,
                Projectile.owner
            );
        }
    }
}
