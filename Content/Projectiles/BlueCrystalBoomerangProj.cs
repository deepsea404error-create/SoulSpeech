using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace SoulSpeech.Content.Projectiles
{
    internal class BlueCrystalBoomerangProj : ModProjectile
    {
        private const float MaxDistance = 420f;
        private const float ReturnSpeed = 14f;
        private const int SlowDownTime = 25;

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("蓝晶回旋镖");
        }

        public override void SetDefaults()
        {
            Projectile.width = 47;
            Projectile.height = 47;
            Projectile.friendly = true;
            Projectile.penetrate = -1; // 无限穿透
            Projectile.timeLeft = 600;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false; // 穿墙
            Projectile.ignoreWater = true;
        }

        //public override void AI()
        //{
        //    Player player = Main.player[Projectile.owner];

        //    // 旋转
        //    Projectile.rotation += 0.4f * Projectile.direction;

        //    // 蓝色粒子
        //    if (Main.rand.NextBool(4))
        //    {
        //        Dust dust = Dust.NewDustDirect(
        //            Projectile.position,
        //            Projectile.width,
        //            Projectile.height,
        //            DustID.BlueCrystalShard,
        //            0f,
        //            0f,
        //            150,
        //            default,
        //            1.1f
        //        );
        //        dust.noGravity = true;
        //    }

        //    // ai[0] = 状态
        //    // 0 = 飞出
        //    // 1 = 减速
        //    // 2 = 返回

        //    if (Projectile.ai[0] == 0f)
        //    {
        //        // 飞出阶段
        //        if (Vector2.Distance(Projectile.Center, player.Center) > MaxDistance)
        //        {
        //            Projectile.ai[0] = 1f;
        //            Projectile.ai[1] = 0f;
        //        }
        //    }
        //    else if (Projectile.ai[0] == 1f)
        //    {
        //        // 减速阶段
        //        Projectile.velocity *= 0.92f;
        //        Projectile.ai[1]++;

        //        if (Projectile.ai[1] >= SlowDownTime)
        //        {
        //            Projectile.ai[0] = 2f;
        //        }
        //    }
        //    else
        //    {
        //        // 返回玩家
        //        Vector2 toPlayer = player.Center - Projectile.Center;
        //        float distance = toPlayer.Length();

        //        if (distance < 20f)
        //        {
        //            Projectile.Kill();
        //            return;
        //        }

        //        toPlayer.Normalize();
        //        Projectile.velocity = toPlayer * ReturnSpeed;
        //        Projectile.tileCollide = false;
        //    }
        //}

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // 始终旋转
            Projectile.rotation += 0.45f * Projectile.direction;

            // 蓝色粒子
            Dust dust = Dust.NewDustDirect(
                Projectile.position,
                Projectile.width,
                Projectile.height,
                DustID.BlueCrystalShard,
                0f,
                0f,
                150,
                default,
                1.5f
            );
            dust.noGravity = true;
            

            switch ((int)Projectile.ai[0])
            {
                case 0:
                    // ===== 飞出（前半段）=====
                    Projectile.ai[1]++;

                    // 即将进入减速前，触发一次波纹
                    if (Projectile.ai[1] == 20f && Projectile.localAI[1] == 0f)
                    {
                        Projectile.localAI[1] = 1f;

                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            Projectile.Center,
                            Vector2.Zero,
                            ModContent.ProjectileType<ShockwaveProj>(),
                            Projectile.damage,
                            0f,
                            Projectile.owner
                        );
                    }

                    // 飞行一段时间后进入减速阶段
                    if (Projectile.ai[1] >= 20f)
                    {
                        Projectile.ai[0] = 1f;
                        Projectile.ai[1] = 0f;
                    }
                    break;

                case 1:
                    // ===== 缓慢减速阶段（后半段）=====
                    Projectile.velocity *= 0.95f;
                    Projectile.ai[1]++;

                    // 当速度很小，进入悬停
                    if (Projectile.velocity.Length() < 1.2f)
                    {
                        Projectile.velocity = Vector2.Zero;
                        Projectile.ai[0] = 2f;
                        Projectile.ai[1] = 0f;
                    }
                    break;

                //case 2:
                //    // ===== 原地旋转悬停 =====
                //    Projectile.velocity = Vector2.Zero;
                //    Projectile.ai[1]++;

                //    // 悬停 20 帧后开始回收
                //    if (Projectile.ai[1] >= 20f)
                //    {
                //        Projectile.ai[0] = 3f;
                //    }
                //    break;
                case 2:
                    // 只生成一次冲击波
                    if (Projectile.localAI[0] == 0f)
                    {
                        Projectile.localAI[0] = 1f;

                        Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            Projectile.Center,
                            Vector2.Zero,
                            ModContent.ProjectileType<ShockwaveProj>(),
                            Projectile.damage,
                            0f,
                            Projectile.owner
                        );
                    }
                    Projectile.velocity = Vector2.Zero;
                    Projectile.ai[1]++;
                    if (Projectile.ai[1] >= 20f)
                    {
                        Projectile.ai[0] = 3f;
                    }
                    break;

                case 3:
                    // ===== 返回玩家 =====
                    Vector2 toPlayer = player.Center - Projectile.Center;
                    float distance = toPlayer.Length();

                    if (distance < 20f)
                    {
                        Projectile.Kill();
                        return;
                    }

                    toPlayer.Normalize();
                    Projectile.velocity = toPlayer * 14f;
                    Projectile.tileCollide = false;
                    break;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // 撞墙不立刻消失，进入返回阶段
            Projectile.ai[0] = 2f;
            Projectile.tileCollide = false;
            return false;
        }
    }
}