using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Projectiles
{
    // 冥灯虚空剑：由虚空弹命中触发，从天空俯冲砸向被命中的敌人（仿星怒 / ExampleShootingSword）。
    // 暗紫色高亮，伤害为触发它的虚空弹的 60%。带小角度追踪以提高命中率。
    internal class NetherVoidSword : ModProjectile
    {
        private const float HomingRange = 650f; // 追踪搜敌范围（覆盖整个俯冲过程，修正预判残差）
        private const int TurnInertia = 16;     // 转向惯性，越大偏移角度越小（仅微调俯冲方向，不锐角拐弯）

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 140;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;

            Projectile.penetrate = 3;
            Projectile.timeLeft = 70; // 下落更快、寿命相对缩短
            Projectile.tileCollide = false; // 穿墙俯冲，确保稳定命中
            Projectile.ignoreWater = true;

            // 本地无敌帧：多把剑同时扑同一敌人各自独立结算；冷却 -1 表示单把剑对每个敌人整生命周期只打一次。
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
        }

        public override void AI()
        {
            SmallAngleHoming();

            // 剑尖朝向俯冲方向（贴图为纵向、剑尖朝上 -Y，需 +90° 对齐飞行方向）
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            // 暗紫色光照
            Lighting.AddLight(Projectile.Center, 0.45f, 0f, 0.7f);

            // 暗紫色拖尾：沿俯冲反方向短距铺开
            for (int i = 0; i < 2; i++)
            {
                Vector2 back = -Projectile.velocity.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(0f, 10f);
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + back,
                    DustID.Shadowflame,
                    Vector2.Zero, 80, default, 1.3f
                );
                dust.noGravity = true;
                dust.velocity *= 0.1f;
            }
        }

        // 小角度追踪：朝最近敌人微调方向，并保持俯冲速度不变（偏移角度小、不影响"从天落下"的观感）
        private void SmallAngleHoming()
        {
            float speed = Projectile.velocity.Length();
            if (speed < 0.01f)
                return;

            float best = HomingRange;
            int target = -1;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy(Projectile))
                    continue;

                float dist = Projectile.Distance(npc.Center);
                if (dist < best)
                {
                    best = dist;
                    target = i;
                }
            }

            if (target < 0)
                return;

            Vector2 desired = (Main.npc[target].Center - Projectile.Center).SafeNormalize(Projectile.velocity) * speed;
            // 加权插值：每帧只微调方向（高惯性 = 小角度），再重置回原速度保持下落手感
            Projectile.velocity = (Projectile.velocity * (TurnInertia - 1) + desired) / TurnInertia;
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * speed;
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
