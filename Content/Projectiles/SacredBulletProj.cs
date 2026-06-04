using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Projectiles
{
    // 圣裁子弹射弹：直线飞行的普通子弹，穿透 3。
    // 命中敌人有 50% 概率在敌人半径 300 的圆周边界生成圣光剑（伤害为本弹的 70%）。
    internal class SacredBulletProj : ModProjectile
    {
        private const float SwordSpawnRadius = 300f; // 圣光剑生成边界半径
        private const float SwordSpeed = 30f;        // 圣光剑俯冲速度

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;

            Projectile.penetrate = 3;       // 穿透 3
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false; // 穿墙
            Projectile.ignoreWater = true;

            // 本地无敌帧：每发子弹独立结算，不污染目标的全局无敌帧(immune[owner])。
            // 否则 penetrate>1 的子弹命中会给目标上 10 帧全局无敌(Projectile.cs:11447)，
            // 把同一玩家的所有后续子弹限流到约 6 次/秒，导致高射速下 DPS 远低于原版单穿子弹。
            // -1 表示单发子弹对每个敌人整生命周期只命中一次（配合 penetrate 3 = 最多穿 3 个不同敌人）。
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;

            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            // 贴图为纵向、弹体朝上 -Y，需 +90° 让其对齐飞行方向
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // 金色光照
            Lighting.AddLight(Projectile.Center, 0.7f, 0.6f, 0.2f);

            // 金色拖尾：沿子弹刚走过的路径补一串插值粒子，填满高速飞行帧间空隙，
            // 形成连续无虚线感的密集拖尾（参考 1.3 神圣弹的逐点铺设算法，颜色保持金色尘 57）
            if (Projectile.timeLeft < 597)
            {
                const float trailPoints = 10f;
                for (float i = 0; i < trailPoints; i += 1f)
                {
                    

                    Vector2 pos = Projectile.Center - Projectile.velocity / trailPoints * i;
                    // Vector2 pos = new Vector2(x2, y2);
                    Dust dust = Dust.NewDustPerfect(
                        pos, DustID.Enchanted_Gold, Vector2.Zero, 100, default, 1f
                    );
                    dust.noGravity = true;
                    dust.velocity *= 0f;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.myPlayer != Projectile.owner)
                return;

            // 50% 概率触发圣光剑
            if (Main.rand.NextFloat() >= 0.5f)
                return;

            // 在敌人半径 300 的圆周边界随机一点生成
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 spawnPos = target.Center + angle.ToRotationVector2() * SwordSpawnRadius;

            // 朝敌人俯冲
            Vector2 vel = (target.Center - spawnPos).SafeNormalize(Vector2.UnitY) * SwordSpeed;

            // 圣光剑伤害 = 本弹伤害的 70%（本弹 damage 已是「子弹+武器」最终值）
            int swordDamage = (int)(Projectile.damage * 0.7f);

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                spawnPos,
                vel,
                ModContent.ProjectileType<HolyLightSword>(),
                swordDamage,
                Projectile.knockBack,
                Projectile.owner
            );
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;

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
