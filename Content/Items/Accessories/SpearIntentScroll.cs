using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulSpeech.Common.Players;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace SoulSpeech.Content.Items.Accessories
{
    internal class SpearIntentScroll : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Spear Intent Scroll");
            // Tooltip.SetDefault("Spear attacks release spear energy");
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;

            Item.scale = 0.5f;

            Item.accessory = true;
            Item.rare = ItemRarityID.Purple;
            Item.value = Item.buyPrice(gold: 1);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<SpearIntentPlayer>().hasSpearIntentScroll = true;
        }
    }
}
