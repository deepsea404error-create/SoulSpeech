using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Projectiles
{
    // 苍夜爆炸：仿日耀/破晓爆炸（射弹 ID 953, aiStyle 117）。
    // 在苍夜枪消亡处炸开的蓝紫色冲击，AoE 范围内对每个敌人单次判定。
    internal class PaleNightExplosion : ModProjectile
    {
        // 贴图：3 帧横向，总尺寸 384x128 → 每帧 128x128
        private const int FrameCount = 3;
        private const int FrameWidth = 128;
        private const int FrameHeight = 128;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 128;
            Projectile.height = 128;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;       // 仅存在数帧用于判定 + 视觉
            Projectile.alpha = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            // 整个生命周期内3次判定，每次命中后该NPC免疫20帧（约0.33秒）
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        // 伤害判定范围扩大至贴图基础尺寸的 1.5 倍（192x192），视觉/粒子不变
        private const float HitboxScale = 1.5f;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // 以爆心为中心，按 HitboxScale 放大判定框（不影响绘制的 128x128）
            float halfW = Projectile.width * HitboxScale / 2f;
            float halfH = Projectile.height * HitboxScale / 2f;
            Rectangle scaled = new(
                (int)(Projectile.Center.X - halfW),
                (int)(Projectile.Center.Y - halfH),
                (int)(halfW * 2f),
                (int)(halfH * 2f)
            );
            return scaled.Intersects(targetHitbox);
        }

        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

            // 内圈：高速蓝紫色爆裂粒子
            for (int i = 0; i < 40; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(3f, 11f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.BlueTorch;
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center, dustType, vel, 0, default, Main.rand.NextFloat(1.8f, 3.4f)
                );
                dust.noGravity = true;
            }

            // 外圈：慢速烟雾（受重力自然飘散）
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(1f, 4f);
                int smoke = Dust.NewDust(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.Smoke, vel.X, vel.Y, 150, default, Main.rand.NextFloat(1f, 1.8f)
                );
                Main.dust[smoke].noGravity = false;
            }
        }

        public override void AI()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % FrameCount;
            }
            // 缩放渐长 + 后半段渐隐
            Projectile.scale += 0.03f;
            if (Projectile.timeLeft < 15)
            {
                Projectile.alpha += 17;
                if (Projectile.alpha > 255)
                    Projectile.alpha = 255;
            }

            Lighting.AddLight(Projectile.Center, 0.6f, 0.45f, 1.0f); // 蓝紫色自发光
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * (1f - Projectile.alpha / 255f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Rectangle src = new Rectangle(Projectile.frame * FrameWidth, 0, FrameWidth, FrameHeight);
            Vector2 origin = new Vector2(FrameWidth / 2f, FrameHeight / 2f);

            // 自发光：忽略世界光照，按 alpha 渐隐
            Color color = Color.White * (1f - Projectile.alpha / 255f);

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                src,
                color,
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
