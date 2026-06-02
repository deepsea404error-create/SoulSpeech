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
    internal class MoltenSpearProj : ModProjectile
    {
        private bool firedScythes = false;

        public override void SetDefaults()
        {
            Projectile.width = 150;
            Projectile.height = 150;
            Projectile.penetrate = -1;
            //Projectile.timeLeft = 90;

            Projectile.CloneDefaults(ProjectileID.MythrilHalberd);
            AIType = ProjectileID.MythrilHalberd;
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            // 以当前命中框中心为基准放大
            int expandX = hitbox.Width / 4;   // ≈ 1.5 倍
            int expandY = hitbox.Height / 4;

            hitbox.Inflate(expandX, expandY);
        }

        public override void AI()
        {
            //Projectile.tileCollide = false;
            Player player = Main.player[Projectile.owner];

            // 火焰粒子（模仿火山剑）

            Dust d = Dust.NewDustDirect(
                Projectile.position,
                Projectile.width,
                Projectile.height,
                DustID.Torch,
                0f, 0f, 150, default, 1.4f);
            d.noGravity = true;
            

            // === 在刺到最前端时发射熔岩镰刀 ===
            // ai[0]：刺出进度（原版逻辑）
            if (!firedScythes && Projectile.ai[0] > 0f)
            {
                firedScythes = true;

                Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                Vector2 spawnPos = Projectile.Center + dir * 65f;

                float[] angles = { -20f, 0f, 20f };

                foreach (float angle in angles)
                {
                    Vector2 vel = dir.RotatedBy(MathHelper.ToRadians(angle)) * 9f;

                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        spawnPos,
                        vel,
                        ModContent.ProjectileType<LavaScytheProj>(),
                        (int)(Projectile.damage * 0.7f),
                        2f,
                        Projectile.owner
                    );
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 着火
            target.AddBuff(BuffID.OnFire, 240);

            // 爆炸（ID 978）
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                target.Center,
                Vector2.Zero,
                978,
                damageDone,
                0f,
                Projectile.owner
            );
        }
    }
}
