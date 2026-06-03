using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SoulSpeech.Content.Projectiles;

namespace SoulSpeech.Content.Items.Ammos.Bullet
{
    internal class SacredJudgmentBullet : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 13;
            Item.DamageType = DamageClass.Ranged;

            Item.width = 8;
            Item.height = 8;

            Item.knockBack = 2f;
            Item.value = Item.sellPrice(copper: 10);
            Item.rare = ItemRarityID.Pink;

            Item.shoot = ModContent.ProjectileType<SacredBulletProj>();
            Item.shootSpeed = 12f;

            Item.ammo = AmmoID.Bullet;
            Item.consumable = true;
            Item.maxStack = 9999;
        }

        public override void AddRecipes()
        {
            CreateRecipe(70)
                .AddIngredient(ItemID.EmptyBullet, 70)
                .AddIngredient(ItemID.SoulofLight, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
