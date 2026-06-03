using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Projectiles
{
    internal class VoidOrb : ModProjectile
    {
        // 贴图：6 帧横向，每帧 72x70，总尺寸 432x70
        private const int FrameCount = 6;
        private const int SingleFrameWidth = 70;
        private const int SingleFrameHeight = 72;

        private const float SuctionRadius = 120f;   // 吸附半径（像素）
        private const float SuctionForce = 0.3f;   // 吸附力
        private const float EscapeSpeedThreshold = 16f; // 超过此速度的敌人可逃逸

        // 伤害比例（统一从武器伤害派生）
        public const float TickFraction = 0.25f;  // 吸附 tick 伤害 = 剑气伤害 × 0.25（由 LongNightAura 写入 Projectile.damage）
        public const float ShardFraction = 0.6f;  // 每颗附弹伤害 = 完整伤害 × 0.6
        public const int ShardCount = 6;           // 死亡喷射附弹数量

        // 追踪参数（比星云奥秘更慢更"黏"）
        private const float HomingRange = 700f;    // 锁敌范围
        private const float HomingSpeed = 3f;      // 转向目标速度（星云为 4）
        private const int HomingSmoothing = 12;    // 插值平滑系数（星云为 8）

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = SingleFrameWidth;
            Projectile.height = SingleFrameHeight;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;      // 1.5 秒后死亡并喷射附弹（追踪+吸附持续更久）
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10; // 吸附 tick 伤害每 10 帧一次（每秒最多 6 次）
        }

        // ai[0] 存储完整剑气伤害（由 LongNightAura.OnHitNPC 写入），供附弹派生
        private int WeaponDamage => (int)Projectile.ai[0];
        // ai[1] 存储锁定的 NPC 索引 + 1（0 表示未锁定）

        public override void AI()
        {
            HomeTowardNearest();

            // 横向帧动画（每 5 帧切换）
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % FrameCount;
            }

            // 脉冲紫色发光
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.2f + 0.8f;
            Lighting.AddLight(Projectile.Center, 0.45f * pulse, 0f, 0.85f * pulse);

            // 周围暗影火焰粒子
            if (Main.rand.NextBool(4))
            {
                int dust = Dust.NewDust(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.Shadowflame,
                    Main.rand.NextFloat(-1f, 1f),
                    Main.rand.NextFloat(-1f, 1f),
                    100, default, 1.1f
                );
                Main.dust[dust].noGravity = true;
            }

            Suction();
        }

        // 缓速追踪最近的敌人（移植自星云奥秘 aiStyle 118，调慢）
        private void HomeTowardNearest()
        {
            // 未锁定：在范围内寻找最近、可被追踪、有视线的敌人
            if (Projectile.ai[1] == 0f)
            {
                float best = HomingRange;
                int target = -1;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.CanBeChasedBy(Projectile))
                        continue;

                    float dist = Projectile.Distance(npc.Center);
                    if (dist < best && Collision.CanHit(Projectile.Center, 1, 1, npc.Center, 1, 1))
                    {
                        best = dist;
                        target = i;
                    }
                }

                if (target >= 0)
                {
                    Projectile.ai[1] = target + 1;
                    Projectile.netUpdate = true;
                }
            }

            // 已锁定：校验目标有效并插值转向
            if (Projectile.ai[1] != 0f)
            {
                int idx = (int)Projectile.ai[1] - 1;
                NPC npc = Main.npc[idx];
                if (npc.active && npc.CanBeChasedBy(Projectile, true) && Projectile.Distance(npc.Center) < 1000f)
                {
                    Vector2 dir = (npc.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * HomingSpeed;
                    Projectile.velocity = (Projectile.velocity * (HomingSmoothing - 1) + dir) / HomingSmoothing;
                }
                else
                {
                    Projectile.ai[1] = 0f; // 目标失效，下一帧重新寻敌
                }
            }
        }

        // 吸附敌人（速度低于逃逸阈值才会被拉近）
        private void Suction()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;

                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist > SuctionRadius || dist < 5f)
                    continue;

                if (npc.velocity.Length() >= EscapeSpeedThreshold)
                    continue; // 速度过快，逃逸

                Vector2 pullDir = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero);
                float strength = SuctionForce * (1f - dist / SuctionRadius);
                npc.velocity += pullDir * strength;

                // 防止吸附力使速度超出阈值
                if (npc.velocity.Length() > EscapeSpeedThreshold)
                    npc.velocity = npc.velocity.SafeNormalize(Vector2.Zero) * EscapeSpeedThreshold;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 爆炸音效
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

            // 爆炸粒子
            for (int i = 0; i < 35; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(3f, 9f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;

                int dust = Dust.NewDust(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.Shadowflame, vel.X, vel.Y, 0, default, Main.rand.NextFloat(1.5f, 2.8f)
                );
                Main.dust[dust].noGravity = true;
            }

            // 死亡喷射暗影附弹（仿星云 617 死亡喷 620）
            // 仅由弹幕所有者执行，避免多人重复计算
            if (Main.myPlayer != Projectile.owner)
                return;

            int shardDamage = Math.Max(1, (int)(WeaponDamage * ShardFraction));
            for (int i = 0; i < ShardCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(4f, 8f);
                if (vel.Y > 0.2f)
                    vel.Y *= -1f; // 略带向上偏移

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    vel,
                    ModContent.ProjectileType<VoidShard>(),
                    shardDamage,
                    Projectile.knockBack * 0.8f,
                    Projectile.owner
                );
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Rectangle src = new Rectangle(Projectile.frame * SingleFrameWidth, 0, SingleFrameWidth, SingleFrameHeight);
            Vector2 origin = new Vector2(SingleFrameWidth / 2f, SingleFrameHeight / 2f);

            // 自发光，不受世界光照影响
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                src,
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
