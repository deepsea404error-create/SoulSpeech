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
    internal class NightmareOrb : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;

            Item.accessory = true;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 2);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // 所有伤害 +25%
            player.GetDamage(DamageClass.Generic) += 0.25f;

            // 受到伤害 +15%（减少伤害减免）
            player.endurance -= 0.15f;
        }
    }
}