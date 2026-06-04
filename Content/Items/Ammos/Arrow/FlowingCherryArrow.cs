using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SoulSpeech.Content.Projectiles;

namespace SoulSpeech.Content.Items.Ammos.Arrow
{
    // 流樱箭：不受重力影响，造成酸性毒液，命中后释放追踪花瓣。
    internal class FlowingCherryArrow : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 14;
            Item.DamageType = DamageClass.Ranged;

            Item.width = 14;
            Item.height = 32;

            Item.knockBack = 2f;
            Item.value = Item.sellPrice(copper: 16);
            Item.rare = ItemRarityID.Orange;

            Item.shoot = ModContent.ProjectileType<FlowingCherryArrowProj>();
            Item.shootSpeed = 5f;

            Item.ammo = AmmoID.Arrow;
            Item.consumable = true;
            Item.maxStack = 9999;
        }
    }
}
