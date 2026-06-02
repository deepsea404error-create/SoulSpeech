using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace SoulSpeech.Content.Projectiles
{

    public class BlueCrystalSpearProj : ModProjectile
    {
        // 计时器，用于控制水矢发射频率
        private int timer = 14;

        public override void SetDefaults()
        {
            // 使用原版秘银长戟的长矛 AI
            Projectile.CloneDefaults(ProjectileID.MythrilHalberd);
            AIType = ProjectileID.MythrilHalberd;

            Projectile.width = 80;
            Projectile.height = 80;
        }

        public override void AI()
        {
            // 每帧减少计时
            timer--;

            if (timer <= 0)
            {
                // 播放水魔法音效
                SoundEngine.PlaySound(SoundID.Item8, Projectile.Center);

                if (Main.myPlayer == Projectile.owner)
                {
                    // 发射水矢（0.8 倍伤害）
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        Projectile.velocity,
                        ProjectileID.WaterBolt,
                        (int)(Projectile.damage * 0.8f),
                        Projectile.knockBack,
                        Projectile.owner
                    );
                }

                // 重置计时器
                timer = 14;
            }

            // 可选：添加蓝色水晶粒子效果
            if (Main.rand.NextBool(4))
            {
                Dust.NewDust(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.BlueCrystalShard,
                    Projectile.velocity.X * 0.2f,
                    Projectile.velocity.Y * 0.2f
                );
            }
        }
    }
}
