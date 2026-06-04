using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Projectiles
{
    // 霓裳矢：从天落下，可穿墙、不穿透敌人、带追踪，命中后释放皇家共鸣（PrincessWeapon, ID 950）。
    // 下落方式仿狂星之怒（Star Wrath），追踪使用 localAI 状态机。
    // ai[1] = 光标目标 Y（仿 Star Wrath 的 alpha 淡入阈值）
    // localAI[0] = 追踪状态（0=搜索, 1=追踪）；ai[0] = 追踪目标 NPC 索引
    internal class NeonSkirtProj : ModProjectile
    {
        private const float HomingRange = 400f;
        private const float HomingSpeed = 12f;
        private const int TurnInertia = 15;

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 14;

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;

            // Projectile.penetrate = 1;            // 不穿透敌人
            Projectile.tileCollide = false;       // 可穿墙
            Projectile.ignoreWater = true;

            Projectile.alpha = 255;              // 初始全透明，逐步淡入
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 180;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            // —— 透明度淡入（仿 Star Wrath）——
            // 高于 ai[1]（目标 Y）时保持半透明；低于后全不透明
            Projectile.alpha -= 15;
            int minAlpha = Projectile.Center.Y >= Projectile.ai[1] ? 0 : 150;
            if (Projectile.alpha < minAlpha)
                Projectile.alpha = minAlpha;

            // —— 追踪（localAI[0]=状态, ai[0]=目标索引）——
            if (Projectile.localAI[0] == 0f && Main.myPlayer == Projectile.owner)
            {
                // 搜索最近敌人
                float best = HomingRange;
                int found = -1;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.CanBeChasedBy(Projectile))
                        continue;

                    float dist = Vector2.Distance(npc.Center, Projectile.Center);
                    if (dist < best)
                    {
                        best = dist;
                        found = i;
                    }
                }

                if (found != -1)
                {
                    Projectile.ai[0] = found;
                    Projectile.localAI[0] = 1f; // 切换到追踪状态
                    Projectile.netUpdate = true;
                }
            }

            // 追踪：加权插值转向
            if (Projectile.localAI[0] == 1f)
            {
                int idx = (int)Projectile.ai[0];
                NPC npc = Main.npc[idx];
                if (!npc.active || !npc.CanBeChasedBy(Projectile))
                {
                    Projectile.localAI[0] = 0f; // 目标失效，重新搜索
                    Projectile.ai[0] = 0f;
                    Projectile.netUpdate = true;
                }
                else
                {
                    Vector2 toTarget = npc.Center - Projectile.Center;
                    if (toTarget != Vector2.Zero)
                        toTarget = toTarget.SafeNormalize(Projectile.velocity) * HomingSpeed;

                    Projectile.velocity = (Projectile.velocity * (TurnInertia - 1) + toTarget) / TurnInertia;
                }
            }

            // 旋转对齐飞行方向
            if (Projectile.velocity != Vector2.Zero)
                Projectile.rotation = Projectile.velocity.ToRotation();

            // 粉色粒子拖尾
            if (Projectile.alpha < 170 && Main.rand.NextBool(2))
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 back = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(4f, 12f);
                    Dust dust = Dust.NewDustPerfect(
                        Projectile.Center + back,
                        DustID.PinkTorch,
                        Vector2.Zero, 40, default, 2.0f
                    );
                    dust.noGravity = true;
                    dust.velocity *= 0.2f;
                }
            }

            // 柔光
            Lighting.AddLight(Projectile.Center, 0.7f, 0.2f, 0.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.myPlayer == Projectile.owner)
            {
                // 命中后在命中位置释放霓裳共鸣
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<NeonResonance>(),
                    (int)(Projectile.damage * 0.5f),
                    Projectile.knockBack,
                    Projectile.owner
                );
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;

            // 自发光，忽略世界光照
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                Color.White,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            return false;
        }
    }
}
