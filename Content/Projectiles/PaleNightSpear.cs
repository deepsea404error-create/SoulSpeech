using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using SoulSpeech.Content.Buffs;
using SoulSpeech.Content.NPCs.Global;

namespace SoulSpeech.Content.Projectiles
{
    // 苍夜枪：仿破晓之光（射弹 ID 636, aiStyle 113）。自实现“飞行 → 插入目标 → 消亡”流程。
    // 与破晓不同：前 1s 不受重力影响。命中后插入敌人、施加 3s 月蚀（+1 叠层）；消亡时炸出苍夜爆炸。
    internal class PaleNightSpear : ModProjectile
    {
        // 状态机：Projectile.ai[0]  0 = 飞行   1 = 已插入敌人
        //         Projectile.ai[1]  插入时记录目标 NPC 索引
        //         Projectile.localAI[0] 飞行子步计数（用于无重力窗口）
        //         Projectile.localAI[1] 插入后子步计数（用于插入寿命）
        private const float NoGravitySubsteps = 120f; // 前 1s 无重力（60 帧 × MaxUpdates 2）
        private const float StickSubsteps = 300f;      // 插入后存活子步数（≈2.5s）

        private const int FrameCount = 8; //贴图：8 帧横向，总尺寸 704x88 → 每帧 88x88
        private const int FrameWidth = 88;
        private const int FrameHeight = 88;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }
        public override void SetDefaults()
        {
            Projectile.width = 88;
            Projectile.height = 88;

            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;

            Projectile.penetrate = -1; // 命中即插入，不会真的多段穿透
            Projectile.alpha = 255;    // 渐显
            Projectile.MaxUpdates = 2; // 与破晓一致，飞行更顺滑
            Projectile.timeLeft = 360; // 兜底寿命；实际由计数器控制
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            // 关键：多发快速投掷必须用本地无敌，否则共享全局无敌会把整体 DPS 限制到 ~6 次/秒
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // 每发对同一目标只生效一次（插入后也不再判定）
        }

        public override void AI()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % FrameCount;
            }
            // 渐显
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 25;
                if (Projectile.alpha < 0)
                    Projectile.alpha = 0;
            }

            if (Projectile.ai[0] == 0f)
                FlyingAI();
            else
                StuckAI();

            Lighting.AddLight(Projectile.Center, 0.5f, 0.35f, 0.8f); // 蓝紫色自发光
        }

        private void FlyingAI()
        {
            Projectile.localAI[0] += 1f; // 子步计数

            // 前 1s 不受重力影响；之后开始下坠（参考破晓 type==636 的 0.995 / 0.15）
            if (Projectile.localAI[0] >= NoGravitySubsteps)
            {
                Projectile.velocity.X *= 0.995f;
                Projectile.velocity.Y += 0.15f;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 + MathHelper.Pi;

            // 蓝紫色拖尾
            if (Main.rand.NextBool(2))
            {
                int dustType = Main.rand.NextBool() ? DustID.PurpleTorch : DustID.BlueTorch;
                Dust dust = Dust.NewDustDirect(
                    Projectile.position, Projectile.width, Projectile.height,
                    dustType, 0f, 0f, 100, default, 1.3f
                );
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
        }

        private void StuckAI()
        {
            int target = (int)Projectile.ai[1];
            Projectile.localAI[1] += 1f;

            // 目标失效 / 插入时间耗尽 → 消亡（OnKill 炸出苍夜爆炸）
            if (target < 0 || target >= Main.maxNPCs || !Main.npc[target].active
                || Main.npc[target].dontTakeDamage || Projectile.localAI[1] >= StickSubsteps)
            {
                Projectile.Kill();
                return;
            }

            // 跟随目标，保持插入姿态（velocity 已在命中时清零，避免渲染漂移）
            Projectile.tileCollide = false;
            Projectile.Center = Main.npc[target].Center;
            Projectile.gfxOffY = Main.npc[target].gfxOffY;
        }

        // 命中即插入目标 + 施加 3s 月蚀（+1 叠层）
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.ai[0] != 0f)
                return; // 已插入，避免重复触发

            Projectile.ai[0] = 1f;
            Projectile.ai[1] = target.whoAmI;
            Projectile.localAI[1] = 0f;
            Projectile.tileCollide = false;
            Projectile.friendly = false;          // 插入后不再造成接触伤害（DoT 由月蚀承担）
            Projectile.velocity = Vector2.Zero;    // 锚定到目标，避免每帧 position += velocity 造成渲染漂移
            Projectile.netUpdate = true;

            target.GetGlobalNPC<LunarEclipseGlobalNPC>().AddStack();
            target.AddBuff(ModContent.BuffType<LunarEclipseDebuff>(), 180); // 3s
        }

        // 撞墙即消亡并炸开（破晓之光落地/插墙后爆裂）
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.ai[0] == 0f)
                Projectile.Kill();

            return false;
        }

        // 消亡炸出苍夜爆炸（仿破晓 636 → 953）
        public override void OnKill(int timeLeft)
        {
            if (Main.myPlayer != Projectile.owner)
                return;

            Vector2 center = Projectile.Center;
            int idx = Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                center, Vector2.Zero,
                ModContent.ProjectileType<PaleNightExplosion>(),
                Projectile.damage, Projectile.knockBack, Projectile.owner
            );
            if (idx >= 0 && idx < Main.maxProjectiles)
                Main.projectile[idx].Center = center; // 以爆心对齐
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * (1f - Projectile.alpha / 255f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Rectangle src = new Rectangle(Projectile.frame * FrameWidth, 0, FrameWidth, FrameHeight);
            Vector2 origin = new Vector2(FrameWidth / 2f, FrameHeight / 2f);

            // 自发光：忽略世界光照，按 alpha 渐显
            Color color = Color.White * (1f - Projectile.alpha / 255f);

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                src,
                color,
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
