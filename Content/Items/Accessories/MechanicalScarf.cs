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
    public class MechanicalScarf : ModItem
    {
        public override void SetStaticDefaults()
        {
            // 名称与描述建议用 Localization，这里留空即可
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;

            Item.accessory = true;

            Item.rare = ItemRarityID.LightRed; // 机械期饰品比较合适
            Item.value = Item.buyPrice(gold: 4);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // 20% 伤害减免
            player.endurance += 0.20f;

            // 防御 +3
            player.statDefense += 3;
        }

        /// <summary>
        /// 控制是否允许装备该饰品
        /// </summary>
        public override bool CanEquipAccessory(Player player, int slot, bool modded)
        {
            // 遍历玩家当前已装备的饰品栏
            for (int i = 0; i < player.armor.Length; i++)
            {
                Item equippedItem = player.armor[i];

                // 如果已经装备了蠕虫围巾，则禁止装备机械围巾
                if (equippedItem.type == ItemID.WormScarf)
                {
                    return false;
                }
            }

            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.WormScarf, 1)
                .AddIngredient(3354, 1) // 机械车轮片
                .AddIngredient(3355, 1) // 机械车体片
                .AddIngredient(3356, 1) // 机械车池片
                .AddTile(TileID.TinkerersWorkbench) // 工匠作坊更符合进阶饰品
                .Register();
        }
    }
}
