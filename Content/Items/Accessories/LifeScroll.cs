using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace SoulSpeech.Content.Items.Accessories
{
    public class LifeScroll : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 48;

            Item.accessory = true;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(silver: 50);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.statLifeMax2 += 20;
            player.maxMinions += 1;
            player.GetKnockback(DamageClass.Summon) += 1f;
        }
    }
}
