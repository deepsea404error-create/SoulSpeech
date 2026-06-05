using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Projectiles
{
    // 月华螺旋:仿激流冲击的螺旋弹。两条反相正弦尘带缠绕中心线,命中炸粉紫水花环,
    // 且冷却就绪时在屏幕外生成月相(追踪被命中的敌人)。纯靠粒子呈现。
    internal class LunarSpiral : ModProjectile
    {
        // 借用原版透明贴图
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.PurificationPowder;

        private const int SpiralPeriod = 40;     // 螺旋周期
        private const float SpiralRadius = 20f;   // 螺旋半径
        private const int PhaseCooldown = 24;     // 月相生成帧冷却

        // ai[0] = 月相伤害(由物品写入);  localAI[0] = 月相生成冷却计数
        private int spiralCounter;

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.aiStyle = -1;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;

            Projectile.penetrate = 5;
            Projectile.timeLeft = 360;
            Projectile.ignoreWater = true;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
        }

        public override void AI()
        {
            // 月相生成冷却递减
            if (Projectile.localAI[0] > 0f)
                Projectile.localAI[0]--;

            Color pinkPurple = new Color(131, 120, 255);
            float baseAngle = Projectile.velocity.ToRotation();
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.Zero);

            // 忠实复刻激流冲击：原版在一帧内跑“两段”，每段把相位计数 +1，
            // 故每帧相位推进 2 步——螺旋采样率翻倍，两股辫子才连贯（这是密度的真正来源）。
            // 每段画两股反相正弦尘带（相位差 π），每股的尘做沿弹道的前置拖尾偏移铺成带。
            for (int step = 0; step < 2; step++)
            {
                for (int strand = 0; strand < 2; strand++)
                {
                    float phase = (strand == 0) ? -MathHelper.PiOver2 : MathHelper.PiOver2;
                    float offsetY = (float)Math.Cos(spiralCounter * MathHelper.TwoPi / SpiralPeriod + phase) * SpiralRadius;
                    Vector2 offset = new Vector2(0f, offsetY).RotatedBy(baseAngle);

                    // 原版每个相位点叠两颗：一颗近、一颗沿弹道更前，连成带状（暗影光束尘 173，柔光紫，染粉紫）
                    Dust d1 = Dust.NewDustDirect(
                        Projectile.Center - Projectile.Size / 4f, Projectile.width / 2, Projectile.height / 2,
                        DustID.ShadowbeamStaff, 0f, 0f, 0, pinkPurple, 1.2f
                    );
                    d1.noGravity = true;
                    d1.position = Projectile.Center + offset;
                    d1.velocity *= 0.15f;
                    d1.velocity += forward * 2f;
                    d1.fadeIn = 1.4f;
                    d1.position += Projectile.velocity * 1.2f;

                    Dust d2 = Dust.NewDustDirect(
                        Projectile.Center - Projectile.Size / 4f, Projectile.width / 2, Projectile.height / 2,
                        DustID.ShadowbeamStaff, 0f, 0f, 0, pinkPurple, 1.2f
                    );
                    d2.noGravity = true;
                    d2.position = Projectile.Center + offset;
                    d2.velocity *= 0.15f;
                    d2.velocity += forward * 2f;
                    d2.fadeIn = 1.4f;
                    d2.position += Projectile.velocity * 0.5f;
                }

                spiralCounter++;
                if (spiralCounter >= SpiralPeriod)
                    spiralCounter = 0;
            }

            // 沿弹道前方投粉紫色光
            DelegateMethods.v3_1 = new Vector3(0.51f, 0.47f, 1f);
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + Projectile.velocity * 4f, 40f,
                new Utils.TileActionAttempt(DelegateMethods.CastLightOpen));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Color pinkPurple = new Color(131, 120, 255);
            float baseAngle = Projectile.velocity.ToRotation();

            // 水花环:绕速度垂直方向铺一圈 40 点尘
            for (int i = 0; i < 40; i++)
            {
                Vector2 ring = (-Vector2.UnitY.RotatedBy(i * MathHelper.TwoPi / 40f) * new Vector2(20f, 20f)).RotatedBy(baseAngle);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.PurpleTorch, 0f, 0f, 100, pinkPurple, 1.25f);
                dust.noGravity = true;
                dust.position = Projectile.Center + ring;
                dust.velocity = ring.SafeNormalize(Vector2.UnitY) * 1f;
            }

            // 冷却就绪 → 屏外生成月相,伤害来自 ai[0]
            if (Projectile.localAI[0] <= 0f)
            {
                Projectile.localAI[0] = PhaseCooldown;
                int phaseDamage = (int)Projectile.ai[0];
                if (phaseDamage <= 0)
                    phaseDamage = Projectile.damage; // 兜底
                LunarPhase.Spawn(Main.player[Projectile.owner], target, phaseDamage, Projectile.knockBack);
            }
        }

        // 不绘制贴图，完全靠粒子呈现（借用的透明贴图本身并非全透明）
        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
