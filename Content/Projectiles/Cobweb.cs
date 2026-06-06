using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Projectiles
{
    // 蛛网：固定 1 秒的蛛网状持续伤害区域，每 20 帧（0.33 秒）对范围内敌人造成 50% 子弹伤害，共 3 段。
    // 视觉上通过 scale 脉冲 + alpha 渐变表现"网住"的感觉（scale 0.5→1→0.7，alpha 0→1→0）。
    public class Cobweb : ModProjectile
    {
        private const int LifeTime = 60; // 1 秒 = 60 帧

        public override void SetDefaults()
        {
            Projectile.width = 78;
            Projectile.height = 76;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = LifeTime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20; // 每 20 帧对同一敌人最多命中 1 次 → 60 帧内最多 3 段
            Projectile.DamageType = DamageClass.Ranged;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float t = 1f - (float)Projectile.timeLeft / LifeTime; // 0 → 1

            // Scale 三段：缩放进入 → 脉冲 → 缩放退出
            float scale;
            if (t < 0.15f)
                scale = MathHelper.Lerp(0.5f, 1f, t / 0.15f);
            else if (t < 0.85f)
                scale = 1f + 0.08f * (float)Math.Sin((t - 0.15f) / 0.7f * Math.PI * 4f);
            else
                scale = MathHelper.Lerp(1f, 0.7f, (t - 0.85f) / 0.15f);

            // Alpha 三段：渐入 → 满 → 渐出
            float alpha;
            if (t < 0.1f)
                alpha = t / 0.1f;
            else if (t < 0.7f)
                alpha = 1f;
            else
                alpha = 1f - (t - 0.7f) / 0.3f;

            Color color = Color.White * alpha;

            Main.EntitySpriteDraw(
                tex,
                drawPos,
                null,
                color,
                0f,
                origin,
                scale,
                SpriteEffects.None,
                0
            );

            return false; // 已自定义绘制，跳过默认绘制
        }
    }
}
