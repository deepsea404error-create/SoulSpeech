using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Items.Materials
{
    /// <summary>
    /// 魂灵：地牢怪物掉落的材料
    /// </summary>
    public class SoulSpirit : ModItem
    {
        public override void SetStaticDefaults()
        {
            // 研究模式所需数量
            Item.ResearchUnlockCount = 25;

            // 设置为类似“地牢灵气”的漂浮效果
            ItemID.Sets.ItemIconPulse[Type] = true;     // 图标呼吸闪烁
            ItemID.Sets.ItemNoGravity[Type] = true;     // 不受重力影响（漂浮）
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;

            Item.maxStack = 9999;
            Item.value = Item.buyPrice(silver: 2);      // 售价 2 银币
            Item.rare = ItemRarityID.Green;              // 绿色品质
        }

        public override void PostUpdate()
        {
            // 让物品在世界中发光（即使在黑暗中也可见）
            Lighting.AddLight(
                Item.Center,
                0.2f,   // 红
                0.45f,  // 绿
                0.6f    // 蓝（偏灵魂色）
            );
        }
    }
}
