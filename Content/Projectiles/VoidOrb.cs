using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Projectiles
{
    internal class VoidOrb : ModProjectile
    {
        // 贴图：6 帧横向，每帧 72x70，总尺寸 432x70
        private const int FrameCount = 6;
        private const int SingleFrameWidth = 70;
        private const int SingleFrameHeight = 72;

        private const float SuctionRadius = 120f;   // 吸附半径（像素）
        private const float SuctionForce = 0.3f;   // 吸附力
        private const float EscapeSpeedThreshold = 16f; // 超过此速度的敌人可逃逸

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = SingleFrameWidth;
            Projectile.height = SingleFrameHeight;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;

            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;      // 1 秒后爆炸
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10; // 吸附 tick 伤害每 10 帧一次（每秒最多 6 次）
        }

        // ai[0] 存储原始武器伤害（由 LongNightAura.OnHitNPC 写入）
        private int WeaponDamage => (int)Projectile.ai[0];

        // tick 伤害为真实伤害，完全忽略敌人防御
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.ScalingArmorPenetration += 1f; // 100% 护甲穿透
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero; // 悬停不动

            // 自旋动画
            // Projectile.rotation += 0.04f;

            // 横向帧动画（每 5 帧切换）
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % FrameCount;
            }

            // 脉冲紫色发光
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.2f + 0.8f;
            Lighting.AddLight(Projectile.Center, 0.45f * pulse, 0f, 0.85f * pulse);

            // 周围暗影火焰粒子
            if (Main.rand.NextBool(4))
            {
                int dust = Dust.NewDust(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.Shadowflame,
                    Main.rand.NextFloat(-1f, 1f),
                    Main.rand.NextFloat(-1f, 1f),
                    100, default, 1.1f
                );
                Main.dust[dust].noGravity = true;
            }

            // 吸附敌人（速度低于逃逸阈值才会被拉近）
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly  || npc.dontTakeDamage)
                    continue;

                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist > SuctionRadius || dist < 5f)
                    continue;

                if (npc.velocity.Length() >= EscapeSpeedThreshold)
                    continue; // 速度过快，逃逸

                Vector2 pullDir = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero);
                float strength = SuctionForce * (1f - dist / SuctionRadius);
                npc.velocity += pullDir * strength;

                // 防止吸附力使速度超出阈值
                if (npc.velocity.Length() > EscapeSpeedThreshold)
                    npc.velocity = npc.velocity.SafeNormalize(Vector2.Zero) * EscapeSpeedThreshold;
            }
        }

        public override void Kill(int timeLeft)
        {
            // 爆炸音效
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

            // 爆炸粒子
            for (int i = 0; i < 35; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(3f, 9f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;

                int dust = Dust.NewDust(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.Shadowflame, vel.X, vel.Y, 0, default, Main.rand.NextFloat(1.5f, 2.8f)
                );
                Main.dust[dust].noGravity = true;
            }

            // 爆炸范围伤害：武器 200%
            // 仅由弹幕所有者执行，避免多人重复计算
            if (Main.myPlayer != Projectile.owner)
                return;

            int explosionDamage = WeaponDamage * 2;
            float explosionRadius = 150f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;

                if (Vector2.Distance(Projectile.Center, npc.Center) > explosionRadius)
                    continue;

                npc.StrikeNPC(new NPC.HitInfo
                {
                    Damage = explosionDamage,
                    Knockback = 7f,
                    HitDirection = npc.Center.X > Projectile.Center.X ? 1 : -1,
                    DamageType = DamageClass.Melee
                });
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Rectangle src = new Rectangle(Projectile.frame * SingleFrameWidth, 0, SingleFrameWidth, SingleFrameHeight);
            Vector2 origin = new Vector2(SingleFrameWidth / 2f, SingleFrameHeight / 2f);

            // 自发光，不受世界光照影响
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                src,
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
