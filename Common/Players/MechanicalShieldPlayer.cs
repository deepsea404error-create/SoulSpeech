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

        // ====== 冲刺参数 ======
        public const int DashCooldown = 50;
        public const int DashDuration = 35;
        public const float DashVelocity = 11.5f;

        // ====== 状态变量 ======
        public bool MechanicalShieldEquipped;

        public int DashDir = -1;
        public int DashDelay = 0;
        public int DashTimer = 0;

        private bool hasHitEnemy = false;

        // 由 PostUpdateEquips 每帧检测，供 CanUseDash 使用
        private bool hasNinjaGearEquipped;

        // ================= Reset =================
        public override void ResetEffects()
        {
            MechanicalShieldEquipped = false;
            DashDir = -1;

            if (Player.controlRight && Player.releaseRight &&
                Player.doubleTapCardinalTimer[DashRight] < 15 &&
                Player.doubleTapCardinalTimer[DashLeft] == 0)
                DashDir = DashRight;
            else if (Player.controlLeft && Player.releaseLeft &&
                     Player.doubleTapCardinalTimer[DashLeft] < 15 &&
                     Player.doubleTapCardinalTimer[DashRight] == 0)
                DashDir = DashLeft;
        }

        // 优先级：忍者大师 > 机械护盾 > 克苏鲁之盾
        public override void PostUpdateEquips()
        {
            if (!MechanicalShieldEquipped) return;

            hasNinjaGearEquipped = false;
            bool hasEoCShield = false;

            for (int i = 3; i < 10 + Player.extraAccessorySlots; i++)
            {
                int type = Player.armor[i]?.type ?? 0;
                if (type == ItemID.MasterNinjaGear) hasNinjaGearEquipped = true;
                if (type == ItemID.EoCShield) hasEoCShield = true;
            }

            // 若未检测到忍者大师，压制克苏鲁之盾的冲刺 dashType
            if (hasEoCShield && !hasNinjaGearEquipped)
                Player.dashType = DashID.None;
        }

        // ================= 冲刺主逻辑 =================
        public override void PreUpdateMovement()
        {
            if (CanUseDash() && DashDir != -1 && DashDelay == 0)
            {
                Player.velocity.X = (DashDir == DashRight ? 1f : -1f) * DashVelocity;

                DashDelay = DashCooldown;
                DashTimer = DashDuration;
                hasHitEnemy = false;
            }

            if (DashTimer > 0)
            {
                DashTimer--;

                Player.eocDash = DashTimer;
                Player.armorEffectDrawShadowEOCShield = true;

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
                        Crit = Main.rand.NextBool(25),
                        DamageType = DamageClass.Melee
                    };

                    npc.StrikeNPC(hit);

                    Player.velocity.X = -Player.direction * 8f;

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
                && !hasNinjaGearEquipped   // 忍者大师优先级更高
                && !Player.setSolar
                && !Player.mount.Active;
        }
    }
}
