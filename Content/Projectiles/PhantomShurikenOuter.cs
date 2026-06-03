using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Projectiles
{
    // 魅影手里剑外旋，跟随主手里剑并同步旋转。贴图 120x120
    // 造成 60% 武器伤害，命中不生成制导球。
    // 主手里剑消失后，外旋继承其前冲方向，缓慢降速并逐渐变淡消失。
    internal class PhantomShurikenOuter : ModProjectile
    {
        private const int AppearDelay = 5;  // 扔出后延迟出现的帧数
        private const int FadeInTime = 10;   // 淡入持续帧数

        // ai[0] 存储父手里剑的 whoAmI
        private int ParentIndex => (int)Projectile.ai[0];

        // ai[1] 标记是否进入消亡阶段（父弹幕已消失）
        private bool Dying
        {
            get => Projectile.ai[1] != 0f;
            set => Projectile.ai[1] = value ? 1f : 0f;
        }

        // 出现阶段计时（随弹幕实例独立计数）
        private int appearTimer;

        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;

            Projectile.penetrate = -1;       // 无限穿透
            Projectile.timeLeft = 300;       // 跟随阶段足够长，实际消亡由父弹幕逻辑控制
            Projectile.tileCollide = false;  // 穿墙
            Projectile.ignoreWater = true;

            Projectile.alpha = 255;          // 初始隐藏，延迟后淡入

            // 本地无敌帧，避免与主手里剑共享全局无敌帧
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            // 与主手里剑相同的旋转频率
            Projectile.rotation += 0.25f;

            if (!Dying)
            {
                Projectile parent = Main.projectile[ParentIndex];
                bool parentValid = parent.active
                    && parent.type == ModContent.ProjectileType<PhantomShurikenProj>()
                    && parent.owner == Projectile.owner;

                if (parentValid)
                {
                    // 传送跟随，用 Center 对齐两种不同尺寸的弹幕中心
                    Projectile.Center = parent.Center;
                    Projectile.velocity = Vector2.Zero;

                    // 记录父弹幕飞行速度，供消亡阶段继续前冲
                    Projectile.localAI[0] = parent.velocity.X;
                    Projectile.localAI[1] = parent.velocity.Y;
                }
                else
                {
                    // 父弹幕消失：进入消亡阶段，继承其前冲方向
                    Dying = true;
                    Projectile.friendly = true;
                    Projectile.velocity = new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
                    Projectile.timeLeft = 45; // 渐隐持续时间上限
                }

                // 延迟出现 + 淡入
                appearTimer++;
                if (appearTimer < AppearDelay)
                {
                    Projectile.alpha = 255;       // 完全隐藏
                    Projectile.friendly = false;  // 隐藏期间不造成伤害
                }
                else if (appearTimer < AppearDelay + FadeInTime)
                {
                    float t = (appearTimer - AppearDelay) / (float)FadeInTime;
                    Projectile.alpha = (int)MathHelper.Lerp(255f, 0f, t);
                    Projectile.friendly = true;
                }
                else
                {
                    Projectile.alpha = 0;
                    Projectile.friendly = true;
                }
            }
            else
            {
                // 继续向前但缓慢降速
                Projectile.velocity *= 0.96f;

                // 逐渐变淡
                Projectile.alpha += 5;
                if (Projectile.alpha >= 255)
                {
                    Projectile.alpha = 255;
                    Projectile.Kill();
                    return;
                }
            }

            // 隐藏阶段不发光、不放粒子
            if (Projectile.alpha >= 255)
                return;

            // 蓝紫色光源
            Lighting.AddLight(Projectile.Center, 0.22f, 0.16f, 0.6f);

            // 环绕粒子：沿外圈做切向流动
            if (Main.rand.NextBool(2))
            {
                float ang = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 offset = ang.ToRotationVector2() * 50f;
                int dust = Dust.NewDust(
                    Projectile.Center + offset, 0, 0,
                    DustID.PurpleTorch, 0f, 0f, 100, default, 1.0f
                );
                Main.dust[dust].noGravity = true;
                // 切向速度，形成旋转的光环
                Main.dust[dust].velocity = offset.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero) * 1.2f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // 消散爆散粒子
            for (int i = 0; i < 14; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                int dust = Dust.NewDust(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.PurpleTorch, vel.X, vel.Y, 100, default, 1.2f
                );
                Main.dust[dust].noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;

            // 渐隐透明度
            float opacity = 1f - Projectile.alpha / 255f;

            // 高亮：忽略世界光照
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                Color.White * opacity,
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
