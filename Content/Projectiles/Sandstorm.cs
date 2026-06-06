using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Projectiles
{
    // 沙暴：仿原版 SandnadoFriendly (id 656) 的列状沙尘效果，固定列高可悬空存在，2 秒持续。
    public class Sandstorm : ModProjectile
    {
        public override void SetDefaults()
        {
            // 固定列高，可悬空：完全不依赖物块（与原版 Sandnado 行为不同）
            Projectile.width = 20;
            Projectile.height = 150;

            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;

            Projectile.timeLeft = 120; // 2 秒

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;

            Projectile.DamageType = DamageClass.Ranged;
        }

        public override void AI()
        {
            // 0. 年龄计数器（用于绘制时的整体旋转 / 淡入淡出，仿原版 proj.ai[0]）
            Projectile.ai[0]++;

            // 1. 出生音效（一次性，仿原版 Sandnado）
            if (Projectile.soundDelay == 0)
            {
                Projectile.soundDelay = -1;
                SoundEngine.PlaySound(SoundID.Item82, Projectile.Center);
            }

            // 2. 沙尘粒子（仿原版 Sandnado dust 269 = 沙尘）
            Vector2 center = Projectile.Center;
            Vector2 size = new Vector2(Projectile.width, Projectile.height);

            Vector2 vector157 = new Vector2(6f, 10f);
            float amount = Main.rand.NextFloat();
            Vector2 vector156 = new Vector2(
                MathHelper.Lerp(0.1f, 1f, Main.rand.NextFloat()),
                MathHelper.Lerp(-0.5f, 0.9f, amount)
            );
            vector156.X *= MathHelper.Lerp(2.2f, 0.6f, amount);
            vector156.X *= -1f;
            Vector2 vector158 = center + size * vector156 * 0.5f + vector157;

            Dust dust = Main.dust[Dust.NewDust(vector158, 0, 0, 269)];
            dust.position = vector158;
            dust.customData = center + vector157;
            dust.fadeIn = 1f;
            dust.scale = 0.3f;
            if (vector156.X > -1.2f) dust.velocity.X = 1f + Main.rand.NextFloat();
            dust.velocity.Y = Main.rand.NextFloat() * -0.5f - 1f;
        }

        // 仿原版 Main.DrawProj 中 type==656 的列状沙暴绘制：
        // 把同一张 60x60 贴图沿竖柱每隔 5.1 像素叠画一遍，每层各自旋转/缩放/染色，
        // 上下两端淡出，整体随年龄旋转、随寿命淡入淡出。
        // 与原版区别：列高用弹幕自身固定高度（可悬空），不依赖 Collision.ExpandVertically。
        public override bool PreDraw(ref Color lightColor)
        {
            const float life = 120f;          // 与 timeLeft 一致
            float age = Projectile.ai[0];

            // 淡入淡出：前 30 帧淡入，最后 30 帧淡出
            float fade = MathHelper.Clamp(age / 30f, 0f, 1f);
            if (age > life - 30f)
                fade = MathHelper.Lerp(1f, 0f, (age - (life - 30f)) / 30f);

            Vector2 top = Projectile.Top;
            Vector2 bottom = Projectile.Bottom;
            float columnHeight = bottom.Y - top.Y;
            if (columnHeight <= 0f)
                return false;

            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = tex.Frame();
            Vector2 origin = frame.Size() / 2f;

            float baseRotation = -MathHelper.Pi / 50f * age;   // 整体随时间旋转
            const float step = 5.1f;
            float traveled = 0f;
            Color tint = new Color(212, 192, 100);             // 原版沙黄色

            for (float y = (int)bottom.Y; y > (int)top.Y; y -= step)
            {
                traveled += step;
                float t = traveled / columnHeight;             // 0=底部，1=顶部
                float segRotation = traveled * MathHelper.TwoPi / -20f;
                float scaleAdd = t - 0.15f;                     // 越往上越大

                // 中段最实、上下两端透明
                Color color = t > 0.5f
                    ? Color.Lerp(Color.Transparent, tint, 2f - t * 2f)
                    : Color.Lerp(Color.Transparent, tint, t * 2f);
                color.A = (byte)(color.A * 0.5f);
                color *= fade;

                Vector2 drawPos = new Vector2(bottom.X, y) - Main.screenPosition;
                Main.EntitySpriteDraw(tex, drawPos, frame, color, baseRotation + segRotation,
                    origin, 1f + scaleAdd, SpriteEffects.None, 0);
            }

            return false; // 已自定义绘制，跳过默认绘制
        }
    }
}
