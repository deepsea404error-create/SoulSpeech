using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SoulSpeech.Content.Buffs;

namespace SoulSpeech.Content.Projectiles.Summon
{
    public class BlueSkull : ModProjectile
    {
        private const float SearchRange = 1000f;
        private const float SideOffset = 80f;
        private const int MinDelayFrames = 45;
        private const int MaxDelayFramesExclusive = 76;
        private const float BoltSpeed = 10f;

        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 28;
            Projectile.minion = true;
            Projectile.minionSlots = 1f;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            Projectile.netImportant = true;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 16;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // 玩家死亡/离线 → 主动清掉 Buff
            if (player.dead || !player.active) {
                player.ClearBuff(ModContent.BuffType<BlueSkullMinionBuff>());
            }
            // 仅当 Buff 存在时续命;右键取消 Buff 后随从 2 帧内自然消失
            if (player.HasBuff(ModContent.BuffType<BlueSkullMinionBuff>())) {
                Projectile.timeLeft = 2;
            }

            if (Projectile.localAI[0] == 0f) {
                Projectile.localAI[0] = 1f;
                Projectile.ai[1] = Main.rand.Next(MinDelayFrames, MaxDelayFramesExclusive);
            }

            float side = (Projectile.whoAmI % 2 == 0) ? SideOffset : -SideOffset;

            NPC target = FindTarget();

            Vector2 hoverPos = target != null
                ? new Vector2(target.Center.X + side, target.Center.Y)
                : new Vector2(player.Center.X + side, player.Center.Y - 60f);

            Vector2 toHover = hoverPos - Projectile.Center;
            float distance = toHover.Length();
            if (distance > 0.001f) toHover.Normalize();

            float lerpAccel = 8f;
            if (distance > 200f) lerpAccel = 10f;
            Projectile.velocity = (Projectile.velocity * 20f + toHover * lerpAccel) / 21f;
            if (Projectile.velocity.Length() > lerpAccel)
                Projectile.velocity *= 0.95f;

            // 贴图朝右,用 spriteDirection 水平镜像:优先朝目标,否则朝移动方向
            int faceDir = Projectile.spriteDirection;
            if (target != null)
                faceDir = (target.Center.X > Projectile.Center.X) ? 1 : -1;
            else if (System.Math.Abs(Projectile.velocity.X) > 0.5f)
                faceDir = (Projectile.velocity.X > 0) ? 1 : -1;

            Projectile.spriteDirection = Projectile.direction = faceDir;
            Projectile.rotation = 0f;

            if (Projectile.ai[1] > 0f)
                Projectile.ai[1]--;

            if (target != null && Projectile.ai[1] <= 0f) {
                if (Main.myPlayer == Projectile.owner) {
                    Vector2 toTarget = target.Center - Projectile.Center;
                    toTarget.Normalize();
                    toTarget *= BoltSpeed;
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        toTarget,
                        ProjectileID.WaterBolt,
                        Projectile.damage,
                        0f,
                        Projectile.owner
                    );
                }
                Projectile.ai[1] = Main.rand.Next(MinDelayFrames, MaxDelayFramesExclusive);
            }
        }

        private NPC FindTarget()
        {
            Player player = Main.player[Projectile.owner];

            // 鞭子标记 / 右键指定目标优先
            if (player.HasMinionAttackTargetNPC) {
                NPC tagged = Main.npc[player.MinionAttackTargetNPC];
                if (tagged.CanBeChasedBy(Projectile))
                    return tagged;
            }

            NPC best = null;
            float bestDistSq = SearchRange * SearchRange;
            foreach (NPC npc in Main.npc) {
                if (!npc.active || !npc.CanBeChasedBy(Projectile)) continue;
                float d = (npc.Center - Projectile.Center).LengthSquared();
                if (d < bestDistSq) {
                    bestDistSq = d;
                    best = npc;
                }
            }
            return best;
        }
    }
}
