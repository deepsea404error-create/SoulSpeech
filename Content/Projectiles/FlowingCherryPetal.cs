using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Projectiles
{
    // 流樱花瓣：两阶段 AI。
    // Phase 1（减速阶段）：向外缓慢移动至速度归零，此期间不造成伤害。
    // Phase 2（追踪阶段）：立即追踪周围最近敌人。
    // 仿原版 FlowerWhipPetal（AI_196）的两阶段模式。
    internal class FlowingCherryPetal : ModProjectile
    {
        private const float Deceleration = 0.88f;      // 每帧乘以该系数减速
        private const float DecelPhaseTime = 25f;       // 减速阶段持续帧数
        private const float HomingRange = 350f;          // 追踪搜敌范围
        private const float HomingSpeed = 8f;            // 追踪速度
        private const int TurnInertia = 12;              // 转向惯性

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;

            Projectile.penetrate = 1;
            Projectile.tileCollide = false;    // 花瓣穿墙
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 120;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            // —— Phase 1：向外减速移动 ——
            if (Projectile.ai[0] < DecelPhaseTime)
            {
                Projectile.ai[0] += 1f;
                Projectile.velocity *= Deceleration;
                Projectile.friendly = false; // 减速期间不造成伤害

                // 花瓣旋转跟随速度方向（贴图为左上角花瓣）
                if (Projectile.velocity != Vector2.Zero)
                    Projectile.rotation = Projectile.velocity.ToRotation();
                return;
            }

            // 过渡到 Phase 2：启用伤害
            if (Projectile.ai[0] == DecelPhaseTime)
            {
                Projectile.ai[0] += 1f; // 只触发一次
                Projectile.friendly = true;
            }

            // —— Phase 2：追踪最近敌人 ——
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
                // 加权插值转向追踪
                Vector2 toTarget = Main.npc[found].Center - Projectile.Center;
                if (toTarget != Vector2.Zero)
                    toTarget = toTarget.SafeNormalize(Projectile.velocity) * HomingSpeed;

                Projectile.velocity = (Projectile.velocity * (TurnInertia - 1) + toTarget) / TurnInertia;
            }
            else
            {
                // 无目标时缓慢飘移
                Projectile.velocity *= 0.97f;
            }

            // 旋转对齐飞行方向
            if (Projectile.velocity != Vector2.Zero)
                Projectile.rotation = Projectile.velocity.ToRotation();

            // 粉色柔光
            Lighting.AddLight(Projectile.Center, 0.5f, 0.15f, 0.3f);
        }

        public override void OnKill(int timeLeft)
        {
            // 销毁时的小粒子效果
            for (int i = 0; i < 4; i++)
            {
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.PinkTorch,
                    Main.rand.NextVector2Circular(1.5f, 1.5f),
                    100, default, 0.8f
                );
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;

            // 花瓣自发光，粉色色调
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
