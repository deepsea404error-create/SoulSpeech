using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Projectiles
{
    // 魅影制导球，由主手里剑命中敌人时生成。
    // 行为参考幽魂面具套装射弹 SpectreWrath (aiStyle 59)：
    //   在敌人中心以随机方向、速度 4 生成，先向外飞离一段时间（不造成伤害），
    //   随后随机锁定附近一个敌人并以极平缓的插值曲线飞回追踪。
    //   范围内无可追踪目标时自我消亡。造成 60% 武器伤害，可暴击。
    internal class PhantomHomingOrb : ModProjectile
    {
        private const int ChargeTime = 60;     // 飞离阶段的更新次数（幽魂的激活延迟为 60）
        private const float HomingRange = 400f; // 索敌半径
        private const float HomingSpeed = 4f;   // 制导基础速度
        private const int TurnInertia = 30;     // 转向惯性，越大转得越平缓（幽魂为 30）

        // ai[0] 锁定的目标 NPC 索引（-1 表示未锁定）
        // ai[1] 蓄力计时
        private int Target
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;

            Projectile.penetrate = 1;        // 命中一次后消失
            Projectile.timeLeft = 720;       // 配合 extraUpdates=3 约 3 秒
            Projectile.tileCollide = false;  // 穿墙
            Projectile.ignoreWater = true;

            Projectile.extraUpdates = 3;     // 与幽魂一致：高频更新，移动快且轨迹顺滑
            Projectile.light = 0.6f;
        }

        public override void OnSpawn(IEntitySource source)
        {
            Target = -1; // 初始未锁定

            // 生成爆散粒子（在中心聚集）
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2.5f, 2.5f);
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center, DustID.PurpleTorch, vel, 100, default, 1.1f
                );
                dust.noGravity = true;
            }
        }

        public override void AI()
        {
            Projectile.ai[1] += 1f;
            Projectile.rotation += 0.3f;

            if (Projectile.ai[1] < ChargeTime)
            {
                // 飞离阶段：保持初始随机速度向外飞，不造成伤害（幽魂在激活前不修改速度）
                Projectile.friendly = false;
            }
            else
            {
                // 制导阶段：随机锁定并曲线飞回追踪
                Projectile.friendly = true;
                HomingUpdate();
                if (!Projectile.active)
                    return;
            }

            // 幽魂式拖尾：在中心聚集铺开数颗蓝紫色粒子（沿速度反方向）
            for (int k = 0; k < 5; k++)
            {
                Vector2 off = new Vector2(-Projectile.velocity.X * 0.2f * k, Projectile.velocity.Y * 0.2f * k);
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + off, DustID.PurpleTorch, Vector2.Zero, 100, default, 1.1f
                );
                dust.noGravity = true;
            }

            // 脉冲蓝紫色发光 (#5340ed 风格)
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.5f + 0.5f;
            Lighting.AddLight(Projectile.Center, 0.4f * pulse, 0.3f * pulse, 1.0f * pulse);
        }

        private void HomingUpdate()
        {
            int target = Target;

            // 仅由所有者负责选靶，再通过网络同步，避免多人不一致
            if (Main.myPlayer == Projectile.owner
                && (target < 0 || !Main.npc[target].CanBeChasedBy(Projectile)))
            {
                target = PickRandomTarget();

                if (target < 0)
                {
                    // 范围内无可追踪目标：消亡
                    Projectile.Kill();
                    return;
                }

                Target = target;
                Projectile.netUpdate = true;
            }

            if (target < 0)
                return;

            // 极平缓的插值追踪，形成幽魂式曲线轨迹
            NPC npc = Main.npc[target];
            Vector2 dir = (npc.Center - Projectile.Center).SafeNormalize(Projectile.velocity) * HomingSpeed;
            Projectile.velocity = (Projectile.velocity * (TurnInertia - 1) + dir) / TurnInertia;
        }

        // 从范围内可追踪敌人中随机选取一个（幽魂的选靶方式）
        private int PickRandomTarget()
        {
            int[] candidates = new int[Main.maxNPCs];
            int count = 0;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy(Projectile))
                    continue;

                if (Vector2.Distance(Projectile.Center, npc.Center) < HomingRange)
                {
                    candidates[count] = i;
                    count++;
                }
            }

            if (count == 0)
                return -1;

            return candidates[Main.rand.Next(count)];
        }

        public override void OnKill(int timeLeft)
        {
            // 消亡爆散粒子（在中心聚集）
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2.5f, 2.5f);
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center, DustID.PurpleTorch, vel, 100, default, 1.1f
                );
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;

            // 高亮：忽略世界光照
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
