using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using SoulSpeech.Content.Buffs;

namespace SoulSpeech.Content.Projectiles.Summon
{
    public class MagneticStormWhipProj : ModProjectile
    {
        // 鞭子连接处(鞭绳)的颜色：浅蓝色。
        private static readonly Color WhipLineColor = new Color(140, 215, 255);

        public override void SetStaticDefaults()
        {
            // 让这个射弹走鞭子的碰撞检测，并允许药水(增益)对它生效。
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.DefaultToWhip();

            // 鞭节数量与射程倍率，可按手感微调。
            Projectile.WhipSettings.Segments = 16;
            Projectile.WhipSettings.RangeMultiplier = 1.4f;
        }

        // AI 在原版鞭子移动 AI 之后额外调用(不影响挥舞本身)，这里只用来挥鞭时撒粒子。
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // 用挥舞进度限制粒子只在“真正甩出”的那段时间生成。
            float swingTime = owner.itemAnimationMax * Projectile.MaxUpdates;
            float swingProgress = Projectile.ai[0] / swingTime;
            bool inActiveSwing = Utils.GetLerpValue(0.1f, 0.7f, swingProgress, true) * Utils.GetLerpValue(0.9f, 0.7f, swingProgress, true) > 0.5f;

            if (!inActiveSwing || Main.rand.NextBool(3))
                return;

            // 取鞭身靠近鞭头的若干节点之一，在其周围撒一颗 160 号粒子(蓝绿电弧)。
            List<Vector2> points = Projectile.WhipPointsForCollision;
            points.Clear();
            Projectile.FillWhipControlPoints(Projectile, points);
            if (points.Count < 3)
                return;

            int pointIndex = Main.rand.Next(System.Math.Max(1, points.Count - 10), points.Count);
            Rectangle spawnArea = Utils.CenteredRectangle(points[pointIndex], new Vector2(30f, 30f));

            Dust dust = Dust.NewDustDirect(spawnArea.TopLeft(), spawnArea.Width, spawnArea.Height, 160, 0f, 0f, 100, Color.White);
            dust.position = points[pointIndex];
            dust.fadeIn = 0.3f;
            dust.noGravity = true;
            dust.velocity *= 0.5f;
            // 让粒子沿鞭身的垂直方向飞出，像甩出的电火花。
            Vector2 segmentDir = points[pointIndex] - points[pointIndex - 1];
            dust.velocity += segmentDir.RotatedBy(owner.direction * MathHelper.PiOver2);
            dust.velocity *= 0.5f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];

            // 1) 挂上磁暴标记，让召唤物吃到 +25 标记伤害与 10% 标记暴击。
            target.AddBuff(ModContent.BuffType<MagneticStormWhipDebuff>(), 240);

            // 2) 把召唤物的攻击目标指向被命中的敌人(鞭子的常规效果)。
            owner.MinionAttackTargetNPC = target.whoAmI;

            // 3) 在敌人位置生成一个 50% 伤害的电圈(原版 Electrosphere, 射弹ID 443)。
            //    先按当前鞭伤算好电圈伤害，再施加多重命中惩罚。
            int sphereDamage = (int)(Projectile.damage * 0.5f);
            if (Projectile.owner == Main.myPlayer) {
                Projectile.NewProjectile(
                    Projectile.GetSource_OnHit(target),
                    target.Center,
                    Vector2.Zero,
                    ProjectileID.Electrosphere,
                    sphereDamage,
                    Projectile.knockBack * 0.5f,
                    Projectile.owner);
            }

            // 多重命中惩罚：命中越多敌人，鞭伤越低，避免群体目标过强。
            Projectile.damage = (int)(Projectile.damage * 0.6f);
        }

        // 在鞭节之间画出浅蓝色的鞭绳，避免贴图之间出现空隙。
        private void DrawLine(List<Vector2> list)
        {
            Texture2D texture = TextureAssets.FishingLine.Value;
            Rectangle frame = texture.Frame();
            Vector2 origin = new Vector2(frame.Width / 2, 2);

            Vector2 pos = list[0];
            for (int i = 0; i < list.Count - 1; i++) {
                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;

                float rotation = diff.ToRotation() - MathHelper.PiOver2;
                // 用浅蓝色乘上当前光照，既保持浅蓝又能随环境明暗变化。
                Color color = Lighting.GetColor(element.ToTileCoordinates(), WhipLineColor);
                Vector2 scale = new Vector2(1, (diff.Length() + 2) / frame.Height);

                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);

                pos += diff;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            List<Vector2> list = new List<Vector2>();
            Projectile.FillWhipControlPoints(Projectile, list);

            // 先画浅蓝色鞭绳(磁暴电弧感)。
            DrawLine(list);

            // 贴图与晨星(射弹ID 848)完全一致，直接复用原版晨星的绘制：
            // 它把贴图按 1 列 5 行(Frame(1,5))切分 —— 手柄/三段鞭身/锤头，并只在偶数节点绘制，
            // 形成间隔的锤珠效果。这样无需自己维护分段表，视觉与晨星一模一样。
            Main.DrawWhip_WhipMace(Projectile, list);

            return false;
        }
    }
}
