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
    public class LifeCrystal : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Life Crystal");
            // Tooltip.SetDefault("Increases life regeneration and max life");
        }

        public override void SetDefaults()
        {
            Item.width = 22;      // 物品贴图宽度
            Item.height = 32;     // 物品贴图高度

            Item.accessory = true; // 饰品

            Item.rare = ItemRarityID.Orange; // 亮橙色品质
            Item.value = Item.buyPrice(gold: 8); // 售价 8 金币
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // 生命恢复速度 +4
            player.lifeRegen += 4;

            // 最大生命 +40
            player.statLifeMax2 += 40;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(1006, 20)       // 叶绿锭
                .AddIngredient(3337, 1)                   // 闪亮石
                .AddIngredient(ItemID.LifeFruit, 4)       // 生命果
                .AddTile(TileID.MythrilAnvil)             // 秘银砧/山铜砧
                .Register();
        }
    }
}
