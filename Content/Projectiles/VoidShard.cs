using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Projectiles
{
    // 虚空球死亡时喷射的暗影附弹（仿星云奥秘 620，自建以统一暗影主题）
    internal class VoidShard : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;

            Projectile.penetrate = 1;       // 命中一次后消失（仿 620）
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // 单次命中即可，配合 penetrate 1
        }

        public override void AI()
        {
            // 轻微重力 + 空气阻力，散射后自然下坠（仿 620 飞行手感）
            Projectile.velocity.Y += 0.12f;
            Projectile.velocity *= 0.99f;

            // 朝向飞行方向
            Projectile.rotation = Projectile.velocity.ToRotation();

            // 暗紫色光源
            Lighting.AddLight(Projectile.Center, 0.3f, 0f, 0.55f);

            // 暗影火焰拖尾
            if (Main.rand.NextBool(2))
            {
                int dust = Dust.NewDust(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.Shadowflame, 0f, 0f, 100, default, 1.1f
                );
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.3f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                int dust = Dust.NewDust(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.Shadowflame, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f,
                    0, default, 1.2f
                );
                Main.dust[dust].noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() * 0.5f;

            // 自发光，不受世界光照影响
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
