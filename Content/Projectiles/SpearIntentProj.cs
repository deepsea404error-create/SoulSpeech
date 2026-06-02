using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace SoulSpeech.Content.Projectiles
{
    internal class SpearIntentProj : ModProjectile
    {
        private const int MaxBounces = 3;
        private int bounceCount;

        private float baseScale;

        public override void SetDefaults()
        {
            Projectile.width = 21;
            Projectile.height = 21;

            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;

            Projectile.DamageType = DamageClass.Melee;

            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;

            Projectile.alpha = 90; // 初始略透明
        }

        public override void OnSpawn(IEntitySource source)
        {
            baseScale = Projectile.scale * 2f;
        }

        public override void AI()
        {
            // ===== 修正方向 =====
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(45f);

            // ===== 波动参数 =====
            float wave = (float)Math.Sin(Main.GameUpdateCount * 0.15f);

            // ===== 缩放轻微波动 =====
            Projectile.scale = baseScale + wave * 0.1f;

            // ===== 透明度波动 =====
            Projectile.alpha = 95 + (int)(wave * 20f);

            // ===== 黄色粒子 =====
            Dust dust = Dust.NewDustDirect(
                Projectile.position,
                Projectile.width,
                Projectile.height,
                DustID.GoldFlame,   // 黄色火焰
                0f,
                0f,
                100,
                default,
                2f
            );

            dust.noGravity = true;
            
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            bounceCount++;

            if (bounceCount >= MaxBounces)
            {
                Projectile.Kill();
                return false;
            }

            // ===== 反弹逻辑 =====
            if (Projectile.velocity.X != oldVelocity.X)
                Projectile.velocity.X = -oldVelocity.X;

            if (Projectile.velocity.Y != oldVelocity.Y)
                Projectile.velocity.Y = -oldVelocity.Y;

            Projectile.velocity *= 0.85f; // 每次反弹略减速

            return false;
        }

        /// <summary>
        /// 强制高亮绘制（不依赖光照）
        /// </summary>
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // 贴图高亮：接近白色，不吃环境光
            Color drawColor = Color.White * (1f - Projectile.alpha / 255f);

            Main.EntitySpriteDraw(
                texture,
                drawPos,
                null,
                drawColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            return false; // 阻止默认绘制
        }
    }
}
