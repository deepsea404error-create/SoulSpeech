using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using SoulSpeech.Content.Buffs;

namespace SoulSpeech.Content.Projectiles
{
    // 虚空弹：冥灯发射的法师弹。发射方式与裂天剑（SkyFracture / 射弹 660）一致，含起手"白圈"特效（本 mod 改暗紫）。
    // 平滑追踪参考裁决弹（JudgmentBolt），但转向惯性更高、更迟钝（降低拐弯性能）。
    // 穿墙、穿透 1；命中后从天落下 3 发冥灯虚空剑（伤害为本弹的 60%），并施加 3s 冥火。
    internal class NetherVoidBolt : ModProjectile
    {
        private const float HomingRange = 800f;  // 追踪范围
        private const float HomingSpeed = 9f;    // 追踪速度
        private const int TurnInertia = 24;      // 转向惯性，越大转得越迟钝（裁决弹为 12）

        private const int SwordCountMin = 1;     // 命中落剑数量下限
        private const int SwordCountMax = 2;     // 命中落剑数量上限
        private const float SwordDamageFraction = 0.6f; // 落剑伤害比例
        private const float SwordFallSpeed = 34f; // 落剑俯冲速度（更快下落）
        private const float SwordSpawnHeight = 700f; // 落剑生成高度（略上移）
        private const float SwordMaxLead = 260f;     // 预判提前量上限，防止击退/瞬移导致的异常速度过度预判
        private const float SwordSideMinOffset = 160f; // 侧向生成最小水平偏移（小于此为"正上方禁区"）
        private const float SwordSideMaxOffset = 340f; // 侧向生成最大水平偏移
        private const float SwordSideMoveThreshold = 0.1f; // 判定敌人左右移动的速度阈值（以下视为静止）

        // 贴图：横向 4 帧，每帧 40 宽 × 24 高（整图 160×24）
        private const int FrameCount = 4;
        private const int FrameWidth = 40;
        private const int FrameHeight = 24;

        // ai[0] 锁定目标索引；ai[1] 状态机（0=未初始化, 1=搜索, 2=追踪）
        // localAI[0] 起手白圈特效已播放标记
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = FrameCount;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;

            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false; // 穿墙
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;         // 起手隐形，首帧爆出白圈后现形

            Projectile.extraUpdates = 1;
            
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            SpawnBurst();
            HomingUpdate();

            // 横向帧动画（每 5 帧切换）
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % FrameCount;
            }

            // 朝向飞行方向（贴图为横向长条、弹尖朝 +X）
            if (Projectile.velocity != Vector2.Zero)
                Projectile.rotation = Projectile.velocity.ToRotation();

            // 暗紫色光照
            Lighting.AddLight(Projectile.Center, 0.4f, 0f, 0.6f);

            // 暗紫色拖尾
            if (Main.rand.NextBool(2))
            {
                int dust = Dust.NewDust(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.Shadowflame, 0f, 0f, 100, default, 1.2f
                );
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.2f;
            }
        }

        // 起手"白圈"特效（仿裂天剑射弹 660 首帧）：本 mod 改为暗紫色环。
        private void SpawnBurst()
        {
            if (Projectile.localAI[0] != 0f)
                return;

            Projectile.localAI[0] = 1f;
            Projectile.alpha = 0; // 现形

            SoundEngine.PlaySound(SoundID.Item9, Projectile.Center);

            const float points = 16f;
            for (int i = 0; i < points; i++)
            {
                // 纵向拉伸的圆环（1:4），再旋转对齐飞行方向
                Vector2 offset = -Vector2.UnitY.RotatedBy(i * (MathHelper.TwoPi / points)) * new Vector2(1f, 4f);
                offset = offset.RotatedBy(Projectile.velocity.ToRotation());

                int dust = Dust.NewDust(Projectile.Center, 0, 0, DustID.Shadowflame);
                Main.dust[dust].scale = 1.5f;
                Main.dust[dust].noGravity = true;
                Main.dust[dust].position = Projectile.Center + offset;
                Main.dust[dust].velocity = offset.SafeNormalize(Vector2.UnitY) * 1f;
            }
        }

        // 平滑追踪（仿裁决弹状态机，加权插值，绝不锐角拐弯；TurnInertia 调高使转向迟钝）
        private void HomingUpdate()
        {
            // 状态 0：初始化
            if (Projectile.ai[1] == 0f)
                Projectile.ai[1] = 1f;

            // 状态 1：搜索目标（仅 owner 选靶，再网络同步）
            if (Projectile.ai[1] == 1f && Main.myPlayer == Projectile.owner)
            {
                int found = -1;
                float best = HomingRange;
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
                    Projectile.ai[1] = 2f;
                    Projectile.netUpdate = true;
                }
            }

            // 状态 2：平滑追踪锁定目标
            if (Projectile.ai[1] == 2f)
            {
                int idx = (int)Projectile.ai[0];
                NPC npc = Main.npc[idx];
                if (!npc.active || !npc.CanBeChasedBy(Projectile))
                {
                    Projectile.ai[1] = 1f; // 目标失效，回到搜索
                    Projectile.ai[0] = 0f;
                    Projectile.netUpdate = true;
                    return;
                }

                Vector2 toTarget = npc.Center - Projectile.Center;
                if (toTarget != Vector2.Zero)
                    toTarget = toTarget.SafeNormalize(Projectile.velocity) * HomingSpeed;

                // 加权插值：每帧只微调方向，形成平滑弧线
                Projectile.velocity = (Projectile.velocity * (TurnInertia - 1) + toTarget) / TurnInertia;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<NetherFlameDebuff>(), 180); // 3s 冥火

            if (Main.myPlayer != Projectile.owner)
                return;

            // 从天落下 1~2 发冥灯虚空剑，伤害为本弹的 60%
            int swordDamage = Math.Max(1, (int)(Projectile.damage * SwordDamageFraction));
            int swordCount = Main.rand.Next(SwordCountMin, SwordCountMax + 1);

            // 生成侧：敌人向左移动→左区间生成；向右移动或静止→右区间生成。正上方为禁区。
            float sideSign = target.velocity.X < -SwordSideMoveThreshold ? -1f : 1f;

            for (int i = 0; i < swordCount; i++)
            {
                float height = SwordSpawnHeight + 100f * i;

                // 预判落点：俯冲耗时 ≈ 高度 / 下落速度，按敌人当前速度推算其未来位置，
                // 让剑落在敌人"将要到达"的点位而非命中瞬间的旧位置（提前量封顶防异常速度过度预判）。
                float leadTime = height / SwordFallSpeed;
                Vector2 lead = target.velocity * leadTime;
                if (lead.Length() > SwordMaxLead)
                    lead = lead.SafeNormalize(Vector2.Zero) * SwordMaxLead;
                Vector2 predicted = target.Center + lead;

                // 在预判点的左/右侧上方生成（跳过正上方禁区），斜向俯冲砸向预判点
                float sideOffset = sideSign * Main.rand.NextFloat(SwordSideMinOffset, SwordSideMaxOffset);
                Vector2 spawn = predicted + new Vector2(sideOffset, -height);
                Vector2 vel = (predicted - spawn).SafeNormalize(Vector2.UnitY) * SwordFallSpeed;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawn,
                    vel,
                    ModContent.ProjectileType<NetherVoidSword>(),
                    swordDamage,
                    Projectile.knockBack,
                    Projectile.owner
                );
            }
        }

        // 消失时（命中或超时）的暗紫色爆散粒子特效
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);

            for (int i = 0; i < 18; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(2f, 6f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                int dust = Dust.NewDust(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.Shadowflame, vel.X, vel.Y, 100, default, Main.rand.NextFloat(1.3f, 2.2f)
                );
                Main.dust[dust].noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 起手隐形阶段不绘制
            if (Projectile.localAI[0] == 0f)
                return false;

            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Rectangle src = new Rectangle(Projectile.frame * FrameWidth, 0, FrameWidth, FrameHeight);
            Vector2 origin = new Vector2(FrameWidth / 2f, FrameHeight / 2f);

            // 自发光，忽略世界光照
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
