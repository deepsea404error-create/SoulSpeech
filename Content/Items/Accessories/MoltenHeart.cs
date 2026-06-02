using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace SoulSpeech.Content.Items.Accessories
{
    internal class MoltenHeart : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 43;
            Item.height = 43;

            Item.accessory = true;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold: 3);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // 所有伤害 +20%
            player.GetDamage(DamageClass.Generic) += 0.20f;

            // 最大生命值 -20%
            player.statLifeMax2 = (int)(player.statLifeMax2 * 0.8f);
        }
    }
}
