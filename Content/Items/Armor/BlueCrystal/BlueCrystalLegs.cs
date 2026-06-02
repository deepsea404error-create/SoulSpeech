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
    [AutoloadEquip(EquipType.Legs)]
    internal class BlueCrystalLegs : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 16;

            Item.defense = 6;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 50);
        }

        public override void UpdateEquip(Player player)
        {
            // 仆从上限 +1
            player.maxMinions += 1;

            // 最大魔力 +40
            player.statManaMax2 += 40;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SoulSpirit>(), 11)
                .AddIngredient(ItemID.Silk, 7)
                .AddIngredient(ItemID.ShadowScale, 10)
                .AddTile(TileID.Anvils)
                .Register();

            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SoulSpirit>(), 11)
                .AddIngredient(ItemID.Silk, 7)
                .AddIngredient(ItemID.TissueSample, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
