using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace SoulSpeech.Content.Projectiles
{
    internal class LavaScytheProj : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;

            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            // ⭐ 关键
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override void AI()
        {
            // ai[0] 用作阶段计时器
            Projectile.ai[0]++;

            // ===== 阶段 1：高速飞出（0 ~ 30 帧）=====
            if (Projectile.ai[0] <= 30f)
            {
                // 不减速，保持高速
            }
            // ===== 阶段 2：开始减速（30 ~ 60 帧）=====
            else if (Projectile.ai[0] <= 50f)
            {
                Projectile.velocity *= 0.98f;
            }
            // ===== 阶段 3：悬停 + 旋转（60+）=====
            else
            {
                Projectile.velocity *= 0.85f;
            }

            // 始终旋转
            Projectile.rotation += 0.4f * Projectile.direction;

            // 火焰粒子（更明显）

            Dust d = Dust.NewDustDirect(
                Projectile.position,
                Projectile.width,
                Projectile.height,
                DustID.Torch,
                Projectile.velocity.X * 0.2f,
                Projectile.velocity.Y * 0.2f,
                500,
                default,
                1.4f
            );
            d.noGravity = true;
            
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 240);
        }
    }
}