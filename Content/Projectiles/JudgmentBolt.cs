using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SoulSpeech.Content.Buffs;

namespace SoulSpeech.Content.Projectiles
{
    // 裁决弹：每第 4 发裁决手枪射击伴随生成。
    // 平滑追踪参考原版叶绿弹 (type 207/634)：用加权插值微调方向，绝不锐角拐弯。
    // 命中施加 3s 裁决 buff 并产生范围爆炸（爆炸伤害为本弹的 50%）。
    internal class JudgmentBolt : ModProjectile
    {
        private const float HomingRange = 800f; // 追踪范围
        private const float HomingSpeed = 9f;   // 追踪速度
        private const int TurnInertia = 12;     // 转向惯性，越小转得越灵活（原为 30，过于笨重）

        // ai[0] 锁定目标索引；ai[1] 状态机（0=未初始化, 1=搜索, 2=追踪）
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;

            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false; // 穿墙
            Projectile.ignoreWater = true;

            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            HomingUpdate();

            // 朝向飞行方向
            if (Projectile.velocity != Vector2.Zero)
                Projectile.rotation = Projectile.velocity.ToRotation();

            // 金色光照
            Lighting.AddLight(Projectile.Center, 0.9f, 0.75f, 0.2f);

            // 金色尾迹粒子 (金色尘 57)
            if (Main.rand.NextBool(2))
            {
                int dust = Dust.NewDust(
                    Projectile.position, Projectile.width, Projectile.height,
                    57, 0f, 0f, 100, default, 1.2f
                );
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.2f;
            }
        }

        private void HomingUpdate()
        {
            // 状态 0：初始化
            if (Projectile.ai[1] == 0f)
            {
                Projectile.ai[1] = 1f;
            }

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
                    // 目标失效，回到搜索
                    Projectile.ai[1] = 1f;
                    Projectile.ai[0] = 0f;
                    Projectile.netUpdate = true;
                    return;
                }

                Vector2 toTarget = npc.Center - Projectile.Center;
                if (toTarget != Vector2.Zero)
                    toTarget = toTarget.SafeNormalize(Projectile.velocity) * HomingSpeed;

                // 加权插值：每帧只微调方向，形成平滑弧线，无锐角拐弯
                Projectile.velocity = (Projectile.velocity * (TurnInertia - 1) + toTarget) / TurnInertia;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<JudgmentDebuff>(), 180); // 3s

            if (Main.myPlayer != Projectile.owner)
                return;

            // 范围爆炸：伤害为本弹的 50%
            int explosionDamage = (int)(Projectile.damage * 0.5f);
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                target.Center,
                Vector2.Zero,
                ModContent.ProjectileType<JudgmentExplosion>(),
                explosionDamage,
                Projectile.knockBack,
                Projectile.owner
            );
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
