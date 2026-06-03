using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using SoulSpeech.Content.Projectiles;

namespace SoulSpeech.Content.Items.Weapons.Melee.Thrown
{
    internal class PhantomShuriken : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 88;
            Item.DamageType = DamageClass.Melee;

            Item.width = 40;
            Item.height = 40;

            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Shoot;

            Item.noMelee = true;       // 伤害由弹幕承担
            Item.noUseGraphic = true;  // 不在手中显示

            Item.knockBack = 4f;
            Item.value = Item.buyPrice(gold: 20);
            Item.rare = ItemRarityID.Pink;

            Item.autoReuse = true;

            Item.shoot = ModContent.ProjectileType<PhantomShurikenProj>();
            Item.shootSpeed = 22f;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.HallowedBar, 15)
                .AddIngredient(ItemID.SoulofNight, 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
