using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;

namespace SoulSpeech.Common.Players
{
    internal class MechanicalShieldPlayer : ModPlayer
    {
        // ====== 方向索引（与原版 doubleTapCardinalTimer 一致） ======
        public const int DashRight = 2;
        public const int DashLeft = 3;

        // ====== 冲刺参数（贴近克苏鲁之盾手感，可微调） ======
        public const int DashCooldown = 50;       // 冲刺冷却（帧）
        public const int DashDuration = 35;       // 冲刺持续/残影时间（帧）
        public const float DashVelocity = 11.5f;  // 冲刺初速度

        // ====== 状态变量 ======
        public bool MechanicalShieldEquipped;

        public int DashDir = -1;
        public int DashDelay = 0;
        public int DashTimer = 0;

        // 防止一次冲刺多次撞击
        private bool hasHitEnemy = false;

        // ================= Reset =================
        public override void ResetEffects()
        {
            MechanicalShieldEquipped = false;
            DashDir = -1;

            // 仅检测双击左/右（与克苏鲁之盾一致，纯水平冲刺）
            if (Player.controlRight && Player.releaseRight &&
                Player.doubleTapCardinalTimer[DashRight] < 15 &&
                Player.doubleTapCardinalTimer[DashLeft] == 0)
                DashDir = DashRight;
            else if (Player.controlLeft && Player.releaseLeft &&
                     Player.doubleTapCardinalTimer[DashLeft] < 15 &&
                     Player.doubleTapCardinalTimer[DashRight] == 0)
                DashDir = DashLeft;
        }

        // ================= 冲刺主逻辑 =================
        public override void PreUpdateMovement()
        {
            // 启动冲刺：给一个水平初速度的爆发
            if (CanUseDash() && DashDir != -1 && DashDelay == 0)
            {
                Player.velocity.X = (DashDir == DashRight ? 1f : -1f) * DashVelocity;

                DashDelay = DashCooldown;
                DashTimer = DashDuration;
                hasHitEnemy = false;
            }

            // 冲刺期间
            if (DashTimer > 0)
            {
                DashTimer--;

                // 克苏鲁盾残影
                Player.eocDash = DashTimer;
                Player.armorEffectDrawShadowEOCShield = true;

                // 只设速度上限，不强制维持：靠原版摩擦自然衰减，手感更顺滑
                if (Player.velocity.X > DashVelocity)
                    Player.velocity.X = DashVelocity;
                else if (Player.velocity.X < -DashVelocity)
                    Player.velocity.X = -DashVelocity;

                TryHitNPC();
            }

            if (DashDelay > 0)
                DashDelay--;
        }

        // ================= 撞击敌人 =================
        private void TryHitNPC()
        {
            if (hasHitEnemy)
                return;

            Rectangle hitbox = Player.Hitbox;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;

                if (hitbox.Intersects(npc.Hitbox))
                {
                    NPC.HitInfo hit = new NPC.HitInfo
                    {
                        Damage = 66,
                        Knockback = 12f,
                        HitDirection = Player.direction,
                        Crit = Main.rand.NextBool(25), // ≈4%暴击
                        DamageType = DamageClass.Melee
                    };

                    npc.StrikeNPC(hit);

                    // 回弹
                    Player.velocity.X = -Player.direction * 8f;

                    // 4秒无敌
                    Player.immune = true;
                    Player.immuneTime = 240;

                    hasHitEnemy = true;
                    break;
                }
            }
        }

        // ================= 可否冲刺 =================
        private bool CanUseDash()
        {
            return MechanicalShieldEquipped
                && Player.dashType == DashID.None     // 不和克盾 / 忍者鞋冲突
                && !Player.setSolar
                && !Player.mount.Active;
        }
    }
}