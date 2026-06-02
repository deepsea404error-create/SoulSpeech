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
    public class MechanicalBrain : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;

            Item.accessory = true;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.buyPrice(gold: 4);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // 标记装备状态
            player.GetModPlayer<MechanicalBrainPlayer>().mechanicalBrainEquipped = true;

            // 防御 +6
            player.statDefense += 6;

            // TODO 原版混沌之脑的“困惑”效果???
        }

        public override bool CanEquipAccessory(Player player, int slot, bool modded)
        {
            // 禁止与混沌之脑同时装备
            for (int i = 0; i < player.armor.Length; i++)
            {
                if (player.armor[i].type == ItemID.BrainOfConfusion)
                    return false;
            }

            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.BrainOfConfusion, 1)
                .AddIngredient(3354, 1) // 机械车轮片
                .AddIngredient(3355, 1) // 机械车体片
                .AddIngredient(3356, 1) // 机械车池片
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }
}
