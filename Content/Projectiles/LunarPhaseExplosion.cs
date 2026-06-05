using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Projectiles
{
    // 月相爆炸：月相命中敌人后炸开的粉紫色范围伤害冲击。纯靠粒子呈现（借用透明贴图）。
    // 范围内对每个敌人单次判定，伤害由月相传入（= 月相伤害 100%）。
    internal class LunarPhaseExplosion : ModProjectile
    {
        // 借用原版透明贴图，绘制全靠粒子
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.PurificationPowder;

        public override void SetDefaults()
        {
            Projectile.width = 90;   // 中等范围 ~90px
            Projectile.height = 90;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 12;   // 仅存在数帧用于判定 + 视觉
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            // 整个生命周期内每个敌人只命中一次
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

            Color pinkPurple = new Color(131, 120, 255);

            // 内圈：高速粉紫爆裂粒子
            for (int i = 0; i < 40; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(3f, 11f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center, DustID.PurpleTorch, vel, 0, pinkPurple, Main.rand.NextFloat(1.8f, 3.4f)
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
            Lighting.AddLight(Projectile.Center, 0.6f, 0.45f, 1.0f); // 粉紫色自发光
        }

        // 不绘制贴图，完全靠粒子呈现
        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
