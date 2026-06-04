using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Projectiles
{
    // 星脉脉冲矢：完全仿照原版脉冲矢（PulseBolt, ID 357）的粒子效果与反弹行为，但无伤害衰减。
    internal class StarVeinPulse : ModProjectile
    {
        public override void SetDefaults()
        {
            // 仿 PulseBolt 基础属性
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Ranged;

            Projectile.penetrate = 6;       // 原版穿透次数
            Projectile.alpha = 255;         // 初始完全透明，配合淡入效果
            Projectile.extraUpdates = 2;    // 高速射弹的额外更新
            Projectile.scale = 1.2f;
            Projectile.timeLeft = 600;
            
            // 每发子弹独立结算，避免全局无敌帧污染
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        // 仿 PulseBolt 的粒子拖尾与 alpha 淡入效果（无 aiStyle，手动处理移动，不受重力）
        public override void AI()
        {

            // alpha < 170 时开始绘制青色拖尾粒子（Dust 206）
            if (Projectile.alpha < 170)
            {
                for (int i = 0; i < 10; i++)
                {
                    float x = Projectile.position.X - Projectile.velocity.X / 10f * i;
                    float y = Projectile.position.Y - Projectile.velocity.Y / 10f * i;
                    int dust = Dust.NewDust(new Vector2(x, y), 1, 1, 206);
                    Main.dust[dust].alpha = Projectile.alpha;
                    Main.dust[dust].position.X = x;
                    Main.dust[dust].position.Y = y;
                    Main.dust[dust].velocity *= 0f;
                    Main.dust[dust].noGravity = true;
                }
            }

            // 每帧降低 alpha，从 255 淡入到 0
            if (Projectile.alpha > 0)
                Projectile.alpha -= 25;

            if (Projectile.alpha < 0)
                Projectile.alpha = 0;
        }

        // 仿 PulseBolt 的墙壁反弹（无伤害衰减）
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.penetrate > 0)
            {
                Projectile.penetrate--;

                // 反弹：反转碰到墙壁的速度分量
                if (Projectile.velocity.X != oldVelocity.X)
                    Projectile.velocity.X = -oldVelocity.X;

                if (Projectile.velocity.Y != oldVelocity.Y)
                    Projectile.velocity.Y = -oldVelocity.Y;

                // 墙壁碰撞音效与粒子
                Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
                SoundEngine.PlaySound(SoundID.Item10, Projectile.position);

                return false; // 不销毁，继续反弹
            }

            return true; // 穿透耗尽，销毁射弹
        }
    }
}
