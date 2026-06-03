using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using SoulSpeech.Common.Players;

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
            player.GetModPlayer<MechanicalScarfPlayer>().hasMechanicalScarf = true;

            // 覆盖蠕虫围巾的 17% 减伤效果（Player.wormScarf 在 PostUpdateEquips 中被清除）
            player.endurance += 0.20f;

            // 防御 +2（升级加成）
            player.statDefense += 2;
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
