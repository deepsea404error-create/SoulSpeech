using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Projectiles
{
    // 月华中心射线：仿激流冲击的中心直线子弹（aiStyle 1 + Bullet）。纯靠粉紫粒子呈现。
    internal class LunarCenterBolt : ModProjectile
    {
        // 借用原版透明贴图，绘制全靠粒子
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.PurificationPowder;

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            // Projectile.aiStyle = 1;
            // AIType = ProjectileID.Bullet;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;

            Projectile.penetrate = 5;
            Projectile.timeLeft = 360;
            Projectile.light = 0.5f;
            // 不设 extraUpdates：与螺旋弹保持相同的子步速率，水平速度一致（同 velocity 发射时不会冲到螺旋前面）
            Projectile.ignoreWater = true;

            // 记忆:穿透弹必须设本地无敌,否则共享全局无敌帧会把整体 DPS 压到 ~6 次/秒
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
        }

        public override void AI()
        {
            Color pinkPurple = new Color(131, 120, 255);

            // 忠实复刻 Torrent_Bullet 的中心拖尾：沿弹道在 5 个等距回拖插值点各放 1 颗，
            // 连成一条连续的中心线带（而非在同一点堆叠）。dust 173 暗影光束尘柔光，染粉紫。
            for (int i = 0; i < 5; i++)
            {
                float t = i / 5f;
                Vector2 spot = Projectile.Center - Projectile.velocity * t;
                Dust dust = Dust.NewDustPerfect(
                    spot, DustID.ShadowbeamStaff, Vector2.Zero, 150, pinkPurple, 1.1f
                );
                dust.noGravity = true;
                dust.velocity *= 0.1f;
                dust.velocity += Projectile.velocity * 0.1f;
            }
            Lighting.AddLight(Projectile.Center, 0.4f, 0.32f, 0.7f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.PurpleTorch, 0f, 0f, 100, new Color(131, 120, 255), 1.1f
                );
                dust.noGravity = true;
                dust.velocity *= 1.6f;
            }
            SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }

        // 不绘制贴图，完全靠粒子呈现（借用的透明贴图本身并非全透明）
        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
