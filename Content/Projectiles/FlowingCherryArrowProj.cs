using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Projectiles
{
    // 流樱箭射弹：不受重力影响，直飞，不穿透不穿墙。命中施加 3 秒酸性毒液，并在四角释放追踪花瓣。
    internal class FlowingCherryArrowProj : ModProjectile
    {
        private const float PetalDamageFrac = 0.3f;
        private const float PetalStartSpeed = 9f;
        private const float PetalSpawnOffset = 30f;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;

            Projectile.arrow = true;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;

            Projectile.penetrate = 1;         // 不穿透敌人
            Projectile.tileCollide = true;     // 不穿墙
            Projectile.ignoreWater = true;

            Projectile.timeLeft = 600;
            
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            // 旋转对齐飞行方向
            if (Projectile.velocity != Vector2.Zero)
                Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            // 粉色花瓣拖尾
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 8f),
                    DustID.PinkTorch,
                    Vector2.Zero, 100, default, 0.8f
                );
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }

            Lighting.AddLight(Projectile.Center, 0.5f, 0.15f, 0.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 施加 3 秒酸性毒液（180 帧）
            target.AddBuff(BuffID.Venom, 180);

            if (Main.myPlayer != Projectile.owner)
                return;

            // 在命中位置四角释放花瓣（左上、右上、左下、右下），伤害为 30%
            int petalDamage = (int)(Projectile.damage * PetalDamageFrac);
            if (petalDamage < 1) petalDamage = 1;

            // 四个对角线方向
            Vector2[] offsets = {
                new Vector2(-1, -1), // 左上
                new Vector2( 1, -1), // 右上
                new Vector2(-1,  1), // 左下
                new Vector2( 1,  1), // 右下
            };

            foreach (Vector2 dir in offsets)
            {
                Vector2 vel = dir.SafeNormalize(Vector2.Zero) * PetalStartSpeed;
                Vector2 spawnPos = Projectile.Center + dir * PetalSpawnOffset;
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos,
                    vel,
                    ModContent.ProjectileType<FlowingCherryPetal>(),
                    petalDamage,
                    Projectile.knockBack,
                    Projectile.owner
                );
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 销毁时的小粒子效果
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.PinkTorch,
                    Main.rand.NextVector2Circular(2f, 2f),
                    100, default, 1.0f
                );
                dust.noGravity = true;
            }
        }
    }
}
