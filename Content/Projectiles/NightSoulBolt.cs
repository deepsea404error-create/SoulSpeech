using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Projectiles
{
    // 夜魂弹：夜魂发射的法师弹。追踪逻辑仿星云烈焰（NebulaBlaze1 / 射弹 634）：
    // 缓慢转向锁定最近敌人。不穿墙、不穿透。
    // 命中敌人后爆散粒子，并朝 360° 方向释放 3 道暗影焰（原版射弹 496，50% 伤害，
    // 生成方式仿暗影焰妖娃：给定初速 + 逐帧递增的小加速度）。
    internal class NightSoulBolt : ModProjectile
    {
        private const float HomingRange = 400f;  // 追踪检测范围
        private const float HomingSpeed = 18f;    // 追踪速度
        private const int TurnInertia = 10;       // 转向惯性，越大转得越缓

        private const int ShadowFlameCount = 5;       // 命中释放的暗影焰数量
        private const float ShadowFlameDamageFrac = 0.6f; // 暗影焰伤害比例
        private const float ShadowFlameSpeed = 5f;    // 暗影焰初速
        private const float ShadowFlameAccel = 0.04f; // 暗影焰沿方向的基础加速度
        private const float ShadowFlameJitter = 15f;  // 暗影焰方向随机扰动上限（±度）

        private const int FrameCount = 4;       // 纵向 4 帧贴图
        private const int FrameSpeed = 5;        // 每 5 帧切换一次

        // ai[0] = 锁定目标索引；ai[1] = 状态（0=未初始化, 1=搜索, 2=追踪）
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

            Projectile.penetrate = 1;       // 不穿透
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;  // 不穿墙
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            HomingUpdate();

            if (Projectile.velocity != Vector2.Zero)
                Projectile.rotation = Projectile.velocity.ToRotation();

            // 纵向 4 帧动画
            if (++Projectile.frameCounter >= FrameSpeed)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % FrameCount;
            }

            // 暗紫色光照
            Lighting.AddLight(Projectile.Center, 0.45f, 0.1f, 0.55f);

            // 暗影焰拖尾
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

        // 平滑追踪状态机（仿星云烈焰：缓慢加权插值转向，绝不锐角拐弯）
        private void HomingUpdate()
        {
            if (Projectile.ai[1] == 0f)
                Projectile.ai[1] = 1f;

            // 状态 1：搜索最近的可追踪敌人（仅 owner 选靶，再网络同步）
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
                    if (dist < best && Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1))
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
            // 爆散粒子特效
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            for (int i = 0; i < 24; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(2f, 6f);
                int dust = Dust.NewDust(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.Shadowflame, 0f, 0f, 100, default, Main.rand.NextFloat(1.3f, 2.2f)
                );
                Main.dust[dust].velocity = angle.ToRotationVector2() * speed;
                Main.dust[dust].noGravity = true;
            }

            if (Main.myPlayer != Projectile.owner)
                return;

            // 朝 360° 方向释放 3 道暗影焰（原版射弹 496），伤害为本弹的 50%。
            // 生成方式仿暗影焰妖娃：初速沿方向，ai[0]/ai[1] 为逐帧递增的小加速度（含随机抖动）。
            int flameDamage = Math.Max(1, (int)(Projectile.damage * ShadowFlameDamageFrac));
            float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);

            for (int i = 0; i < ShadowFlameCount; i++)
            {
                // 均分基准角 + ±ShadowFlameJitter° 随机扰动，打破规整间隔
                float angle = baseAngle + i * (MathHelper.TwoPi / ShadowFlameCount)
                    + MathHelper.ToRadians(Main.rand.NextFloat(-ShadowFlameJitter, ShadowFlameJitter));
                Vector2 dir = angle.ToRotationVector2();

                Vector2 vel = dir * ShadowFlameSpeed;

                // 沿方向的基础加速度 + 仿妖娃的小幅随机抖动（[-0.03,0.03]）
                float accelX = dir.X * ShadowFlameAccel + Main.rand.Next(-30, 31) * 0.001f;
                float accelY = dir.Y * ShadowFlameAccel + Main.rand.Next(-30, 31) * 0.001f;

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    vel,
                    ProjectileID.ShadowFlame,
                    flameDamage,
                    Projectile.knockBack,
                    Projectile.owner,
                    accelX,
                    accelY
                );
            }
        }
    }
}
