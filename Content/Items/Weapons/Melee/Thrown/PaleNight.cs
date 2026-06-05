using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SoulSpeech.Content.Projectiles;

namespace SoulSpeech.Content.Items.Weapons.Melee.Thrown
{
    // 苍夜：仿破晓（内部物品 ID 3543）。投掷出苍夜枪，命中插入敌人并施加月蚀。
    internal class PaleNight : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 125;
            Item.DamageType = DamageClass.Melee;
            Item.knockBack = 5f;
            // 暴击加成（基础 4% 之外）。如需 tooltip 正好显示 6%，把这里改为 2。
            Item.crit = 6;

            Item.width = 46;
            Item.height = 46;

            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Swing; // 与破晓一致
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;

            Item.noMelee = true;      // 伤害由苍夜枪承担
            Item.noUseGraphic = true; // 不在手中显示武器贴图

            Item.shoot = ModContent.ProjectileType<PaleNightSpear>();
            Item.shootSpeed = 20f;

            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Red; // 与破晓同档
        }

        // 暂不提供合成配方
    }
}
