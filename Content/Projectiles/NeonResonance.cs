using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Projectiles
{
    // 霓裳共鸣：仿原版皇家共鸣（PrincessWeapon, ID 950, AI_186），
    // 使用 usesLocalNPCImmunity 消除骗伤。
    // 粒子系统严格复刻原版 + 额外环形绘制增强边界可见度。
    internal class NeonResonance : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 104;
            Projectile.height = 104;

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;

            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.alpha = 255;
            Projectile.timeLeft = 60;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            // ===== 与原版 AI_186 逐行对应 =====

            float num = 60f;
            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] >= num)
            {
                Projectile.Kill();
                return;
            }

            Projectile.Opacity = Utils.Remap(Projectile.ai[0], 0f, num, 1f, 0f);

            float num2 = Projectile.ai[0] / num;
            float num3 = 1f - (1f - num2) * (1f - num2);
            float num4 = 1f - (1f - num3) * (1f - num3);
            float num5 = Utils.Remap(Projectile.ai[0], num - 15f, num, 0f, 1f);
            float num6 = num5 * num5;
            float num7 = 1f - num6;

            Projectile.scale = (0.4f + 0.6f * num4) * num7;

            float num8 = Utils.Remap(Projectile.ai[0], 20f, num, 0f, 1f);
            float num9 = 1f - (1f - num8) * (1f - num8);
            float num10 = 1f - (1f - num9) * (1f - num9);
            Projectile.localAI[0] = (0.4f + 0.6f * num10) * num7;

            int num11 = Projectile.width / 2; // 52
            Color newColor = Main.hslToRgb(0.93f, 1f, 0.5f) * Projectile.Opacity;
            float num12 = 6f;
            float num13 = 2f;
            float ringRadius = num11 * Projectile.scale;

            // ===== 外环粒子（偏移 +6f）—— 原版 exact =====
            if (num2 < 0.9f)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (Main.rand.NextBool(2))
                    {
                        Vector2 radial = Vector2.UnitX.RotatedBy(Main.rand.NextFloat() * MathHelper.TwoPi);
                        Vector2 onRing = radial * ringRadius;
                        Vector2 tangent = radial.RotatedBy(0.7853981852531433); // π/4
                        Vector2 pos = Projectile.Center + onRing + tangent * num12;

                        int d = Dust.NewDust(pos, 0, 0, DustID.RainbowMk2, 0f, 0f, 0, newColor);
                        Main.dust[d].position = pos;
                        Main.dust[d].noGravity = true;
                        Main.dust[d].scale = 0.3f;
                        Main.dust[d].fadeIn = Main.rand.NextFloat() * 1.2f * Projectile.scale;
                        Main.dust[d].velocity = tangent * Projectile.scale * (0f - num13);
                        Main.dust[d].scale *= Projectile.scale;
                        Main.dust[d].velocity += Projectile.velocity * 0.5f;
                        Main.dust[d].position += Main.dust[d].velocity * -5f;

                        if (d != 6000)
                        {
                            Dust clone = Dust.CloneDust(d);
                            clone.scale /= 2f;
                            clone.fadeIn *= 0.85f;
                            clone.color = new Color(255, 255, 255, 255);
                        }
                    }
                }
            }

            // ===== 内环粒子（偏移 -6f）—— 原版 exact =====
            if (num2 < 0.9f)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (Main.rand.NextBool(2))
                    {
                        Vector2 radial = Vector2.UnitX.RotatedBy(Main.rand.NextFloat() * MathHelper.TwoPi);
                        Vector2 onRing = radial * ringRadius;
                        Vector2 tangent = radial.RotatedBy(0.7853981852531433);
                        Vector2 pos = Projectile.Center + onRing + tangent * (0f - num12);

                        int d = Dust.NewDust(pos, 0, 0, DustID.RainbowMk2, 0f, 0f, 0, newColor);
                        Main.dust[d].position = pos;
                        Main.dust[d].noGravity = true;
                        Main.dust[d].scale = 0.3f;
                        Main.dust[d].fadeIn = Main.rand.NextFloat() * 1.2f * Projectile.scale;
                        Main.dust[d].velocity = tangent * Projectile.scale * num13;
                        Main.dust[d].scale *= Projectile.scale;
                        Main.dust[d].velocity += Projectile.velocity * 0.5f;
                        Main.dust[d].position += Main.dust[d].velocity * -5f;

                        if (d != 6000)
                        {
                            Dust clone = Dust.CloneDust(d);
                            clone.scale /= 2f;
                            clone.fadeIn *= 0.85f;
                            clone.color = new Color(255, 255, 255, 255);
                        }
                    }
                }
            }

            // ===== ParticleOrchestra 特效（原版 PrincessWeapon sparkle）=====
            if (num2 < 0.95f)
            {
                for (float num16 = 0f; num16 < 0.8f; num16 += 1f)
                {
                    if (Main.rand.NextBool(4))
                    {
                        Vector2 vector9 = Vector2.UnitX.RotatedBy(
                            Main.rand.NextFloat() * MathHelper.TwoPi + MathHelper.PiOver2
                        ) * ringRadius;

                        ParticleOrchestrator.RequestParticleSpawn(
                            clientOnly: true,
                            ParticleOrchestraType.PrincessWeapon,
                            new ParticleOrchestraSettings
                            {
                                PositionInWorld = Projectile.Center + vector9,
                                MovementVector = Projectile.velocity
                            },
                            Projectile.owner
                        );
                    }
                }
            }

            // ===== 终末爆发（第 50 帧，和原版完全一致）=====
            if (Projectile.ai[0] == num - 10f)
            {
                for (float num17 = 0f; num17 < 1f; num17 += 0.25f)
                {
                    Vector2 vector10 = Vector2.UnitX.RotatedBy(MathHelper.TwoPi * num17);

                    ParticleOrchestrator.RequestParticleSpawn(clientOnly: true,
                        ParticleOrchestraType.PrincessWeapon,
                        new ParticleOrchestraSettings { PositionInWorld = Projectile.Center, MovementVector = vector10 * 1f },
                        Projectile.owner);

                    ParticleOrchestrator.RequestParticleSpawn(clientOnly: true,
                        ParticleOrchestraType.PrincessWeapon,
                        new ParticleOrchestraSettings { PositionInWorld = Projectile.Center, MovementVector = vector10 * 2f },
                        Projectile.owner);
                }
            }

            Lighting.AddLight(Projectile.Center, 0.9f, 0.2f, 0.5f);
        }

        public override bool? CanHitNPC(NPC target)
        {
            return target.CanBeChasedBy(Projectile) ? null : false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;

            Color drawColor = Main.hslToRgb(0.93f, 0.6f, 0.5f) * Projectile.Opacity;

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                drawColor,
                0f,
                origin,
                Projectile.scale * 1.2f,
                SpriteEffects.None,
                0
            );

            return false;
        }
    }
}
