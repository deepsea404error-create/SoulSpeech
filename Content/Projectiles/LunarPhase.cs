using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Projectiles
{
    // 月相：仿无头骑士南瓜剑的南瓜头（射弹 321, aiStyle 55）。
    // 由月华螺旋弹命中时在屏幕外随机边缘生成，追踪被命中的 NPC。
    internal class LunarPhase : ModProjectile
    {
        private const float HomingSpeed = 13f;  // 追踪飞行速度（已加强，原版南瓜头为 8）
        private const int TurnInertia = 6;       // 转向惯性，越小转得越急、咬得越紧（原版 15）

        // ai[0] = 锁定的目标 NPC 索引
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;

            Projectile.penetrate = 1;       // 只命中 1 个敌人后消亡
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false; // 仿南瓜头穿墙，保证从屏外能飞到目标
            Projectile.ignoreWater = true;
            Projectile.light = 0.4f;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        // 屏幕外随机边缘生成一枚月相，瞄准 target，并把 target 索引写入 ai[0]。
        // 逻辑照搬 Player.HorsemansBlade_SpawnPumpkin。
        public static void Spawn(Player player, NPC target, int damage, float knockBack)
        {
            if (Main.myPlayer != player.whoAmI)
                return; // 只在本地玩家端生成，避免多人重复

            Vector2 center = target.Center;
            int y = Main.maxScreenH;
            int x = Main.maxScreenW;
            int num = Main.rand.Next(100, 300);
            int num2 = Main.rand.Next(100, 300);
            num = (Main.rand.Next(2) != 0) ? (num + (x / 2 - num)) : (num - (x / 2 + num));
            num2 = (Main.rand.Next(2) != 0) ? (num2 + (y / 2 - num2)) : (num2 - (y / 2 + num2));
            num += (int)player.position.X;
            num2 += (int)player.position.Y;
            Vector2 spawn = new Vector2(num, num2);

            float dx = center.X - spawn.X;
            float dy = center.Y - spawn.Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            float scale = HomingSpeed / dist;
            dx *= scale;
            dy *= scale;

            Projectile.NewProjectile(
                player.GetSource_ItemUse(player.HeldItem),
                num, num2, dx, dy,
                ModContent.ProjectileType<LunarPhase>(),
                damage, knockBack, player.whoAmI,
                target.whoAmI // ai[0] = 目标索引
            );
        }

        public override void AI()
        {
            // 朝向跟随速度
            if (Projectile.velocity.X < 0f)
            {
                Projectile.spriteDirection = -1;
                Projectile.rotation = (float)Math.Atan2(-Projectile.velocity.Y, -Projectile.velocity.X);
            }
            else
            {
                Projectile.spriteDirection = 1;
                Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X);
            }

            int target = (int)Projectile.ai[0];
            NPC npc = (target >= 0 && target < Main.maxNPCs) ? Main.npc[target] : null;

            if (npc != null && npc.CanBeChasedBy(Projectile) && !NPCID.Sets.CountsAsCritter[npc.type])
            {
                // 平滑追踪锁定目标
                Vector2 dir = npc.Center - Projectile.Center;
                float len = dir.Length();
                if (len > 0f)
                    dir *= HomingSpeed / len;
                Projectile.velocity = (Projectile.velocity * (TurnInertia - 1) + dir) / TurnInertia;
            }
            else
            {
                // 目标失效：找最近可追击且可视线命中的敌人（仿 aiStyle 55）。
                // 搜索上限放大到 2000：月相从屏外生成，距敌常 >1000px，过小会令其失标后空滑。
                float best = 2000f;
                int found = -1;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC n = Main.npc[i];
                    if (n.CanBeChasedBy(Projectile) && !NPCID.Sets.CountsAsCritter[n.type])
                    {
                        float d = Math.Abs(Projectile.Center.X - n.Center.X) + Math.Abs(Projectile.Center.Y - n.Center.Y);
                        if (d < best && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, n.position, n.width, n.height))
                        {
                            best = d;
                            found = i;
                        }
                    }
                }
                if (found != -1)
                    Projectile.ai[0] = found;
                // 找不到目标不立即 Kill（保留南瓜头那种持续滑行；timeLeft 到点自然消失）
            }

            // 粉紫色拖尾
            if (Main.rand.NextBool())
            {
                Dust dust = Dust.NewDustDirect(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.PurpleTorch, 0f, 0f, 100, new Color(131, 120, 255), 1.3f
                );
                dust.noGravity = true;
                dust.velocity *= 0.4f;
            }

            Lighting.AddLight(Projectile.Center, 0.5f, 0.4f, 0.9f);
        }

        // 命中爆炸粒子效果：粉紫色爆裂 + 烟雾 + 音效；并炸出范围伤害
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Color pinkPurple = new Color(131, 120, 255);

            // 内圈：高速粉紫爆裂粒子
            for (int i = 0; i < 24; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(2.5f, 8f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center, DustID.PurpleTorch, vel, 0, pinkPurple, Main.rand.NextFloat(1.4f, 2.6f)
                );
                dust.noGravity = true;
            }

            // 外圈：慢速烟雾（受重力自然飘散）
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(1f, 3f);
                int smoke = Dust.NewDust(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.Smoke, vel.X, vel.Y, 150, default, Main.rand.NextFloat(1f, 1.6f)
                );
                Main.dust[smoke].noGravity = false;
            }

            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);

            // 炸出范围伤害（= 月相伤害的 100%），由本地玩家生成避免多人重复
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center, Vector2.Zero,
                    ModContent.ProjectileType<LunarPhaseExplosion>(),
                    Projectile.damage, Projectile.knockBack, Projectile.owner
                );
            }
        }
    }
}
