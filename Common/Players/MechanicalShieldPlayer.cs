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
        // ====== 方向索引（与原版一致） ======
        public const int DashDown = 0;
        public const int DashUp = 1;
        public const int DashRight = 2;
        public const int DashLeft = 3;

        // ====== 冲刺参数 ======
        public const int DashCooldown = 60;     // 冲刺冷却（帧）
        public const int DashDuration = 20;     // 冲刺持续时间
        public const float DashVelocity = 13f;  // 冲刺速度（比克盾略远）

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

            // 检测双击方向键（与 ExampleShield 完全一致）
            if (Player.controlDown && Player.releaseDown && Player.doubleTapCardinalTimer[DashDown] < 15)
                DashDir = DashDown;
            else if (Player.controlUp && Player.releaseUp && Player.doubleTapCardinalTimer[DashUp] < 15)
                DashDir = DashUp;
            else if (Player.controlRight && Player.releaseRight &&
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
            // 启动冲刺
            if (CanUseDash() && DashDir != -1 && DashDelay == 0)
            {
                Vector2 newVelocity = Player.velocity;

                switch (DashDir)
                {
                    case DashLeft:
                        newVelocity.X = -DashVelocity;
                        break;
                    case DashRight:
                        newVelocity.X = DashVelocity;
                        break;
                    case DashUp:
                        newVelocity.Y = -DashVelocity * 1.3f;
                        break;
                    case DashDown:
                        newVelocity.Y = DashVelocity;
                        break;
                }

                DashDelay = DashCooldown;
                DashTimer = DashDuration;
                hasHitEnemy = false;

                Player.velocity = newVelocity;
            }

            // 冲刺中
            if (DashTimer > 0)
            {
                DashTimer--;

                // 克苏鲁盾残影
                Player.eocDash = DashTimer;
                Player.armorEffectDrawShadowEOCShield = true;

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