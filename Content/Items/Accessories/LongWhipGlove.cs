using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace SoulSpeech.Content.Items.Accessories
{
    public class LongWhipGlove : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 22;

            Item.accessory = true;

            Item.rare = ItemRarityID.Green;
            Item.value = Item.buyPrice(gold: 1);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.whipRangeMultiplier += 0.20f;
        }
    }
}
