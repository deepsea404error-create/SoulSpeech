using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using SoulSpeech.Content.Buffs;

namespace SoulSpeech.Content.Projectiles
{
    // 裁决弹命中后的范围爆炸。伤害由裁决弹传入（已是 50%），命中亦施加裁决 buff。
    // 无独立贴图，纯靠金色粒子呈现。
    internal class JudgmentExplosion : ModProjectile
    {
        // 借用原版透明贴图，绘制完全由粒子代替
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.PurificationPowder;

        public override void SetDefaults()
        {
            Projectile.width = 150;
            Projectile.height = 150;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 3;          // 仅存在数帧用于判定
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            // 整个生命周期内每个敌人只命中一次
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

            // 烟雾碎片 (参考爆炸护符)
            for (int i = 0; i < 6; i++)
            {
                Vector2 goreVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 7f);
                Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, goreVel, GoreID.Smoke1 + Main.rand.Next(3));
            }

            // 内圈：高速金色火焰粒子 (金色尘 57)
            int fireCount = Main.rand.Next(30, 50);
            for (int i = 0; i < fireCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(4f, 12f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center, DustID.Enchanted_Gold, vel, 0, default, Main.rand.NextFloat(1.8f, 3.5f)
                );
                dust.noGravity = true;
            }

            // 外圈：慢速烟雾粒子（受重力自然飘散）
            int smokeCount = Main.rand.Next(20, 35);
            for (int i = 0; i < smokeCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(1f, 4f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                int smoke = Dust.NewDust(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.Smoke, vel.X, vel.Y, 150, default, Main.rand.NextFloat(1f, 2f)
                );
                Main.dust[smoke].noGravity = false;
            }
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 1.1f, 0.9f, 0.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<JudgmentDebuff>(), 180); // 3s
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false; // 不绘制贴图
        }
    }
}
