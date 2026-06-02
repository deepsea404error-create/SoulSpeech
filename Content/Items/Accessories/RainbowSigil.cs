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

    public class RainbowSigil : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Rainbow Sigil");
            // Tooltip.SetDefault("A condensed crystal of all gem energies");
        }

        public override void SetDefaults()
        {
            Item.width = 45;
            Item.height = 48;

            Item.scale = 0.5f;   // 贴图缩小

            Item.accessory = true;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(gold: 1);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // 生命恢复 +1
            player.lifeRegen += 1;

            // 防御 +2
            player.statDefense += 2;

            // 最大生命 +20
            player.statLifeMax2 += 20;

            // 攻击速度 +5%
            player.GetAttackSpeed(DamageClass.Generic) += 0.05f;

            // 伤害减免 +3%
            player.endurance += 0.03f;

            // 移动速度 +5%
            player.moveSpeed += 0.05f;

            // 召唤栏位 +1
            player.maxMinions += 1;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Amber, 10)
                .AddIngredient(ItemID.Diamond, 10)
                .AddIngredient(ItemID.Ruby, 10)
                .AddIngredient(ItemID.Emerald, 10)
                .AddIngredient(ItemID.Sapphire, 10)
                .AddIngredient(ItemID.Topaz, 10)
                .AddIngredient(ItemID.Amethyst, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
