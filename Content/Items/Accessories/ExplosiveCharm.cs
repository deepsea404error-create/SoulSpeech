using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using SoulSpeech.Common.Players;

namespace SoulSpeech.Content.Items.Accessories
{
    internal class ExplosiveCharm : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;

            Item.value = Item.sellPrice(gold: 3);
            Item.rare = ItemRarityID.Orange;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ExplosiveCharmPlayer>().hasExplosiveCharm = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.CrossNecklace);
            recipe.AddIngredient(ItemID.Explosives, 5);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}