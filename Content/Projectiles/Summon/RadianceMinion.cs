using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SoulSpeech.Content.Buffs;

namespace SoulSpeech.Content.Projectiles.Summon
{
    // 辉曜召唤物：直接照搬原版帝王之刃(曜光/Terraprisma, EmpressBlade 946, aiStyle 156)的 AI。
    // 仅做两处适配：① 用 mod 自己的 Buff 续命替换 player.empressBlade；
    // ② 贴图朝右(原版朝上)，故移除旋转里的 +PI/2 偏移；③ 搜索范围 1000→700(削弱)。
    // 命中 15% 概率施加 3s 裁决；整体金色尘埃。
    public class RadianceMinion : ModProjectile
    {
        private const float TargetRange = 700f;   // 原版 1000f，削弱

        public override void SetStaticDefaults()
        {
            // 注意：不要设 Main.projPet[type] = true。
            // 原版 946 虽是 projPet，但在 Damage_CanDealDamage 里有 "type != 946 || ai[0]==0" 的专属豁免；
            // 自定义 type 拿不到该豁免，一旦标记 projPet 就会被判定为不可造成接触伤害 → 永远零伤害。
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            // 照搬原版 type==946 的默认值
            Projectile.netImportant = true;
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.minionSlots = 1f;
            Projectile.timeLeft = 18000;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;   // 关键：配合每次攻击 ResetLocalNPCHitImmunity 才有伤害
            Projectile.manualDirectionChange = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.scale = 1f;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // 续命：替换原版 player.empressBlade
            if (player.dead || !player.active)
                player.ClearBuff(ModContent.BuffType<RadianceMinionBuff>());
            if (player.HasBuff(ModContent.BuffType<RadianceMinionBuff>()))
                Projectile.timeLeft = 2;

            EmitGoldDust(Projectile.ai[0] > 0f ? 0.5f : 0.15f);

            Think();
        }

        // 照搬 AI_156_Think 的 flag2(946) 分支
        private void Think()
        {
            const int num = 40;
            const int num2 = num - 1;   // 39
            const int num3 = num + 40;  // 80
            const int num4 = num3 - 1;  // 79
            const int num5 = num + 1;   // 41

            Player player = Main.player[Projectile.owner];
            if (player.active && Vector2.Distance(player.Center, Projectile.Center) > 2000f) {
                Projectile.ai[0] = 0f;
                Projectile.ai[1] = 0f;
                Projectile.netUpdate = true;
            }

            // 回归待机
            if (Projectile.ai[0] == -1f) {
                GetGroupIndex(out int index, out int total);
                GetIdlePosition(index, total, out Vector2 idleSpot, out float idleRotation);
                Projectile.velocity = Vector2.Zero;
                Projectile.Center = Projectile.Center.MoveTowards(idleSpot, 32f);
                Projectile.rotation = Projectile.rotation.AngleLerp(idleRotation, 0.2f);
                if (Projectile.Distance(idleSpot) < 2f) {
                    Projectile.ai[0] = 0f;
                    Projectile.netUpdate = true;
                }
                return;
            }

            // 待机绕圈 + 尝试索敌
            if (Projectile.ai[0] == 0f) {
                GetGroupIndex(out int index3, out int total3);
                GetIdlePosition(index3, total3, out Vector2 idleSpot3, out float idleRotation3);
                Projectile.velocity = Vector2.Zero;
                Projectile.Center = Vector2.SmoothStep(Projectile.Center, idleSpot3, 0.45f);
                Projectile.rotation = Projectile.rotation.AngleLerp(idleRotation3, 0.45f);
                if (Main.rand.Next(20) == 0) {
                    int t = TryAttackingNPCs(skipBodyCheck: false);
                    if (t != -1) {
                        StartAttack();
                        Projectile.ai[0] = num3;
                        Projectile.ai[1] = t;
                        Projectile.netUpdate = true;
                    }
                }
                return;
            }

            // 攻击中
            const bool skipBodyCheck = true;
            int num14 = 0;
            int num15 = num2;
            int num16 = 0;
            if (Projectile.ai[0] >= num5) {
                num14 = 1;
                num15 = num4;
                num16 = num5;
            }

            int num17 = (int)Projectile.ai[1];
            if (!Main.npc.IndexInRange(num17)) {
                RetargetOrIdle(num, num3, skipBodyCheck);
                return;
            }
            NPC nPC2 = Main.npc[num17];
            if (!nPC2.CanBeChasedBy(Projectile)) {
                RetargetOrIdle(num, num3, skipBodyCheck);
                return;
            }

            Projectile.ai[0] -= 1f;
            if (Projectile.ai[0] >= num15) {
                Projectile.direction = (Projectile.Center.X < nPC2.Center.X) ? 1 : -1;
                if (Projectile.ai[0] == num15) {
                    Projectile.localAI[0] = Projectile.Center.X;
                    Projectile.localAI[1] = Projectile.Center.Y;
                }
            }

            float lerpValue2 = Utils.GetLerpValue(num15, num16, Projectile.ai[0], clamped: true);
            if (num14 == 0) {
                Vector2 vector3 = new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
                if (lerpValue2 >= 0.5f)
                    vector3 = Vector2.Lerp(nPC2.Center, player.Center, 0.5f);

                Vector2 center3 = nPC2.Center;
                float num20 = (center3 - vector3).ToRotation();
                float num21 = (Projectile.direction == 1) ? (-(float)Math.PI) : ((float)Math.PI);
                float num22 = num21 + (0f - num21) * lerpValue2 * 2f;
                Vector2 sp2 = num22.ToRotationVector2();
                sp2.Y *= 0.5f;
                sp2.Y *= 0.8f + (float)Math.Sin((float)Projectile.identity * 2.3f) * 0.2f;
                sp2 = sp2.RotatedBy(num20);
                float num23 = (center3 - vector3).Length() / 2f;
                Vector2 center4 = Vector2.Lerp(vector3, center3, 0.5f) + sp2 * num23;
                Projectile.Center = center4;
                float num24 = MathHelper.WrapAngle(num20 + num22);
                Projectile.rotation = num24;   // 原版 + PI/2(朝上贴图)，我们贴图朝右故去掉
                Vector2 vel = num24.ToRotationVector2() * 10f;
                Projectile.velocity = vel;
                Projectile.position -= vel;   // 抵消引擎随后 position += velocity，使 Center 即为设定值
            }

            if (num14 == 1) {
                Vector2 vector5 = new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
                vector5 += new Vector2(0f, Utils.GetLerpValue(0f, 0.4f, lerpValue2, clamped: true) * -100f);
                Vector2 v = nPC2.Center - vector5;
                Vector2 vector6 = v.SafeNormalize(Vector2.Zero) * MathHelper.Clamp(v.Length(), 60f, 150f);
                Vector2 value = nPC2.Center + vector6;
                float lerpValue3 = Utils.GetLerpValue(0.4f, 0.6f, lerpValue2, clamped: true);
                float lerpValue4 = Utils.GetLerpValue(0.6f, 1f, lerpValue2, clamped: true);
                float targetAngle = v.SafeNormalize(Vector2.Zero).ToRotation();   // 原版 + PI/2
                Projectile.rotation = Projectile.rotation.AngleTowards(targetAngle, (float)Math.PI / 5f);
                Projectile.Center = Vector2.Lerp(vector5, nPC2.Center, lerpValue3);
                if (lerpValue4 > 0f)
                    Projectile.Center = Vector2.Lerp(nPC2.Center, value, lerpValue4);
            }

            if (Projectile.ai[0] == num16)
                RetargetOrIdle(num, num3, skipBodyCheck);
        }

        private void RetargetOrIdle(int num, int num3, bool skipBodyCheck)
        {
            int t = TryAttackingNPCs(skipBodyCheck);
            if (t != -1) {
                Projectile.ai[0] = Main.rand.NextFromList(num, num3);
                Projectile.ai[1] = t;
                StartAttack();
            } else {
                Projectile.ai[0] = -1f;
                Projectile.ai[1] = 0f;
            }
            Projectile.netUpdate = true;
        }

        private void StartAttack()
        {
            Projectile.ResetLocalNPCHitImmunity();   // 关键：每次攻击重置免疫，才能持续造成伤害
        }

        // 照搬 AI_156_TryAttackingNPCs：优先鞭子/右键指定目标，否则范围内最近
        private int TryAttackingNPCs(bool skipBodyCheck)
        {
            Vector2 center = Main.player[Projectile.owner].Center;
            int result = -1;
            float best = -1f;

            NPC owt = Projectile.OwnerMinionAttackTargetNPC;
            if (owt != null && owt.CanBeChasedBy(Projectile)) {
                bool flag = true;
                if (owt.Distance(center) > TargetRange) flag = false;
                if (!skipBodyCheck && !Projectile.CanHitWithOwnBody(owt)) flag = false;
                if (flag) return owt.whoAmI;
            }

            for (int i = 0; i < Main.maxNPCs; i++) {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy(Projectile)) {
                    float d = npc.Distance(center);
                    if (!(d > TargetRange) && (!(d > best) || best == -1f) && (skipBodyCheck || Projectile.CanHitWithOwnBody(npc))) {
                        best = d;
                        result = i;
                    }
                }
            }
            return result;
        }

        // 照搬 AI_GetMyGroupIndexAndFillBlackList(本版本未填充黑名单)
        private void GetGroupIndex(out int index, out int total)
        {
            index = 0;
            total = 0;
            for (int i = 0; i < Main.maxProjectiles; i++) {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == Projectile.owner && p.type == Projectile.type) {
                    if (Projectile.whoAmI > i) index++;
                    total++;
                }
            }
        }

        // 照搬 AI_156_GetIdlePosition 的 flag2(946) 分支。
        // idleSpot 计算与原版一致；返回的 idleRotation 去掉原版末尾 +PI/2(贴图朝右)。
        private void GetIdlePosition(int stackedIndex, int totalIndexes, out Vector2 idleSpot, out float idleRotation)
        {
            Player player = Main.player[Projectile.owner];
            int num3 = stackedIndex + 1;
            float baseRot = MathHelper.WrapAngle(num3 * ((float)Math.PI * 2f) * (1f / 60f) * player.direction + (float)Math.PI / 2f);
            int num4 = num3 % Math.Max(totalIndexes, 1);
            Vector2 vector = new Vector2(0f, 0.5f).RotatedBy((player.miscCounterNormalized * (2f + num4) + num4 * 0.5f + player.direction * 1.3f) * ((float)Math.PI * 2f)) * 4f;
            idleSpot = baseRot.ToRotationVector2() * 10f + player.MountedCenter + new Vector2(player.direction * (num3 * -6 - 16), player.gravDir * -15f);
            idleSpot += vector;
            idleRotation = baseRot;   // 原版此处再 += PI/2，朝右贴图省略
        }

        private void EmitGoldDust(float chance)
        {
            if (Main.rand.NextFloat() < chance) {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
                    DustID.GoldFlame, 0f, 0f, 100, default, 1.1f);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 0.3f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextFloat() < 0.15f)
                target.AddBuff(ModContent.BuffType<JudgmentDebuff>(), 180);
        }
    }
}
