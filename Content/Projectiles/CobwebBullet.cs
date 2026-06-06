using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Projectiles
{
    // 蛛网弹：直飞小弹，不穿透。命中敌人后产生持续 1 秒的蛛网（每 20 帧 1 段，共 3 段，每段 50% 子弹伤害）。
    // 销毁时（命中/撞墙/超时）均释放蛛丝粒子。
    internal class CobwebBullet : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;

            Projectile.penetrate = 1;       // 不穿透
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;

            Projectile.timeLeft = 600;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            // 弹体对齐飞行方向
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // 银色拖尾
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 6f),
                    DustID.Silver,
                    Vector2.Zero, 100, default, 0.6f
                );
                dust.noGravity = true;
                dust.velocity *= 0.2f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.myPlayer != Projectile.owner)
                return;

            // 在命中点产生蛛网（伤害为本弹的 50%，每 20 帧 1 段，共 3 段）
            int cobwebDamage = (int)(Projectile.damage * 0.5f);
            if (cobwebDamage < 1) cobwebDamage = 1;

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                target.Center,
                Vector2.Zero,
                ModContent.ProjectileType<Cobweb>(),
                cobwebDamage,
                Projectile.knockBack,
                Projectile.owner
            );
        }

        public override void OnKill(int timeLeft)
        {
            // 销毁时的蛛丝粒子（命中/撞墙/超时均触发）
            for (int i = 0; i < 8; i++)
            {
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                Dust dust = Dust.NewDustPerfect(
                    pos,
                    DustID.Silver,
                    Main.rand.NextVector2Circular(1.5f, 1.5f),
                    100, default, 0.7f
                );
                dust.noGravity = true;
            }
        }
    }
}
