using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Buffs
{
    // 磁暴鞭命中后挂在敌人身上的标记 Debuff：让你的召唤物对该敌人造成额外伤害与暴击。
    public class MagneticStormWhipDebuff : ModBuff
    {
        public static readonly int TagDamage = 25;       // 标记伤害(每次召唤命中加的固定伤害)
        public static readonly int TagCritChance = 10;   // 标记暴击率(%)

        public override void SetStaticDefaults()
        {
            // 让这个 Debuff 能挂到那些原本免疫所有 Debuff 的敌人身上(标记类 Buff 的惯例)。
            BuffID.Sets.IsATagBuff[Type] = true;
        }
    }

    // 真正把标记伤害/暴击应用到“召唤物命中带标记敌人”这件事上。
    public class MagneticStormWhipDebuffNPC : GlobalNPC
    {
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            // 只有玩家的召唤物/哨兵命中才吃标记加成，排除敌方弹幕与陷阱。
            if (projectile.npcProj || projectile.trap || !projectile.IsMinionOrSentryRelated)
                return;

            if (!npc.HasBuff<MagneticStormWhipDebuff>())
                return;

            // SummonTagDamageMultiplier 会对部分召唤物的标记伤害做平衡缩放。
            var projTagMultiplier = ProjectileID.Sets.SummonTagDamageMultiplier[projectile.type];

            // 固定标记伤害。
            modifiers.FlatBonusDamage += MagneticStormWhipDebuff.TagDamage * projTagMultiplier;

            // 标记暴击：每次命中按概率触发暴击。伤害由攻击方(召唤物拥有者)计算并同步，
            // 所以这里 roll Main.rand 在多人下也是一致的。
            if (Main.rand.Next(100) < MagneticStormWhipDebuff.TagCritChance)
                modifiers.SetCrit();
        }
    }
}
