using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Projectiles
{
    // 圣光剑：由圣裁子弹命中触发，在敌人半径 300 边界生成后朝敌人俯冲（类似星怒）。
    // 金色高亮，伤害为触发子弹的 40%。
    internal class HolyLightSword : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 14;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;

            Projectile.penetrate = 3;
            Projectile.timeLeft = 18;
            Projectile.tileCollide = false; // 穿墙俯冲
            Projectile.ignoreWater = true;

            // 本地无敌帧：多把圣光剑同时扑同一敌人时各自独立结算，互不抢全局无敌帧、不丢伤害。
            // 冷却 -1 表示单把剑对每个敌人整个生命周期只打一次（一次俯冲斩）。
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            // 剑尖朝向俯冲方向（贴图为横向、剑尖朝右 +X，无需偏移）
            Projectile.rotation = Projectile.velocity.ToRotation();

            // 金色光照
            Lighting.AddLight(Projectile.Center, 1.0f, 0.85f, 0.3f);

            // 聚集的金色拖尾：每帧在剑体中心密集生成多颗，沿俯冲反方向短距铺开
            for (int i = 0; i < 3; i++)
            {
                Vector2 back = -Projectile.velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(0f, 10f);
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + back,
                    DustID.Enchanted_Gold,
                    Vector2.Zero, 80, default, 1.4f
                );
                dust.noGravity = true;
                dust.velocity *= 0.1f;
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
