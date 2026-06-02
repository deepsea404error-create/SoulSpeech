using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulSpeech.Content.Items.Materials;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Items.Armor.BlueCrystal
{
    [AutoloadEquip(EquipType.Head)]
    internal class BlueCrystalHead : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 18;

            Item.defense = 6;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 40);
        }

        public override void UpdateEquip(Player player)
        {
            // 魔法暴击率 +8%
            player.GetCritChance(DamageClass.Magic) += 8f;

            // 召唤伤害 +10%
            player.GetDamage(DamageClass.Summon) += 0.10f;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<BlueCrystalBody>()
                && legs.type == ModContent.ItemType<BlueCrystalLegs>();
        }
        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "+10%魔法暴击率\n+10%召唤伤害\n+8%鞭子范围\n+1仆从上限\n散发蓝色光芒";

            // 魔法暴击率 +10%
            player.GetCritChance(DamageClass.Magic) += 10f;

            // 召唤伤害 +10%
            player.GetDamage(DamageClass.Summon) += 0.10f;

            // 鞭子范围 +8%
            player.whipRangeMultiplier += 0.08f;

            // 仆从上限 +1
            player.maxMinions += 1;

            // 蓝色光照
            Lighting.AddLight(player.Center, 0.2f, 0.4f, 0.8f);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SoulSpirit>(), 9)
                .AddIngredient(ItemID.Silk, 6)
                .AddIngredient(ItemID.ShadowScale, 8)
                .AddTile(TileID.Anvils)
                .Register();

            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SoulSpirit>(), 9)
                .AddIngredient(ItemID.Silk, 6)
                .AddIngredient(ItemID.TissueSample, 8)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
