using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SoulSpeech.Content.Projectiles.Summon;

namespace SoulSpeech.Content.Items.Weapons.Summon
{
    public class MagneticStormWhip : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            // DefaultToWhip(射弹, 伤害, 击退, 射速(决定鞭长), 动画总时长)
            Item.DefaultToWhip(ModContent.ProjectileType<MagneticStormWhipProj>(), 170, 4f, 7f, 30);

            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.buyPrice(gold: 10);
        }

        // 让鞭子可以附带近战(召唤鞭)前缀。
        public override bool MeleePrefix()
        {
            return true;
        }
    }
}
