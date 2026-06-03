using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Projectiles
{
    internal class LongNightAura : ModProjectile
    {
        // 贴图：4 帧横向，每帧 192x179，总尺寸 768x179
        private const int FrameCount = 4;
        private const int SingleFrameWidth = 179;
        private const int SingleFrameHeight = 192;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1; // 手动处理横向帧，禁用纵向自动动画
        }

        public override void SetDefaults()
        {
            Projectile.width = SingleFrameWidth;
            Projectile.height = SingleFrameHeight;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;

            Projectile.penetrate = 6;      // 无限穿透
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false; // 穿墙
            Projectile.ignoreWater = true;

            // 使用本地无敌帧，避免与虚空球的 tick 伤害共享全局无敌帧
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10; // 每个敌人最多每秒被剑气命中一次
        }

        public override void AI()
        {
            // 旋转朝向飞行方向
            if (Projectile.velocity != Vector2.Zero)
                Projectile.rotation = Projectile.velocity.ToRotation();

            // 横向帧动画（每 5 帧切换）
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % FrameCount;
            }

            // 暗紫色光源
            Lighting.AddLight(Projectile.Center, 0.35f, 0.0f, 0.65f);

            // 暗影火焰尾迹粒子
            if (Main.rand.NextBool(2))
            {
                int dust = Dust.NewDust(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.Shadowflame,
                    0f, 0f, 100, default, 1.4f
                );
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.15f;
            }
        }

        // 命中敌人：在敌人位置生成虚空球
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.myPlayer != Projectile.owner)
                return;

            // 以武器原始伤害存入 ai[0]，供虚空球爆炸使用
            Projectile orb = Projectile.NewProjectileDirect(
                Projectile.GetSource_FromThis(),
                target.Center,
                Vector2.Zero,
                ModContent.ProjectileType<VoidOrb>(),
                10,  // 吸附 tick 真实伤害每次 10 点（护甲穿透由 VoidOrb.ModifyHitNPC 处理）
                3f,
                Projectile.owner
            );
            orb.ai[0] = Projectile.damage; // 记录原始伤害用于爆炸
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Rectangle src = new Rectangle(Projectile.frame * SingleFrameWidth, 0, SingleFrameWidth, SingleFrameHeight);
            Vector2 origin = new Vector2(SingleFrameWidth / 2f, SingleFrameHeight / 2f);

            // 向左飞行时纵向翻转，保持刃向正确
            SpriteEffects effects = Projectile.velocity.X < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;

            // 自发光：不受世界光照影响
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                src,
                Color.White,
                Projectile.rotation,
                origin,
                Projectile.scale,
                effects,
                0
            );

            return false;
        }
    }
}
