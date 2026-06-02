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
    public class SwordIntentScroll : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Sword Intent Scroll");
            // Tooltip.SetDefault("Melee attacks release sword energy");
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;


            Item.scale = 0.5f;   // 贴图缩小

            Item.accessory = true;

            Item.rare = ItemRarityID.Purple;           // 紫色品质
            Item.value = Item.buyPrice(gold: 1);       // 售价 1 金币
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // 开启剑意效果
            player.GetModPlayer<SwordIntentPlayer>().hasSwordIntentScroll = true;
        }
    }
}
