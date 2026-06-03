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
            player.GetModPlayer<MechanicalBrainPlayer>().mechanicalBrainEquipped = true;

            player.statDefense += 4;

            // 暴击率 +12%（全伤害类型）
            player.GetCritChance(DamageClass.Generic) += 12f;

            // 召唤伤害 +12%
            player.GetDamage(DamageClass.Summon) += 0.12f;
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
