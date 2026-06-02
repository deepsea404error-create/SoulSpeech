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
    [AutoloadEquip(EquipType.Body)]
    internal class BlueCrystalBody : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 18;

            Item.defense = 7;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 60);
        }

        public override void UpdateEquip(Player player)
        {
            // 魔法伤害 +10%
            player.GetDamage(DamageClass.Magic) += 0.10f;

            // 鞭子速度 +10%
            player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.10f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SoulSpirit>(), 15)
                .AddIngredient(ItemID.Silk, 9)
                .AddIngredient(ItemID.ShadowScale, 12)
                .AddTile(TileID.Anvils)
                .Register();

            CreateRecipe()
                .AddIngredient(ModContent.ItemType<SoulSpirit>(), 15)
                .AddIngredient(ItemID.Silk, 9)
                .AddIngredient(ItemID.TissueSample, 12)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
