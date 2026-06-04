using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using SoulSpeech.Content.Projectiles;

namespace SoulSpeech.Content.Items.Weapons.Magic.Books
{
    // 夜魂：法师武器。发射一枚夜魂弹（仿星云烈焰追踪，命中后爆散并释放 3 道暗影焰）。
    internal class NightSoul : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 40;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.crit = 6;

            Item.width = 40;
            Item.height = 40;

            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;

            Item.knockBack = 3f;
            Item.value = Item.buyPrice(gold: 15);
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item20;

            Item.shoot = ModContent.ProjectileType<NightSoulBolt>();
            Item.shootSpeed = 20f;

            Item.noMelee = true;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-6f, 0f);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SoulofNight, 12)
                .AddIngredient(ItemID.CrystalShard, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
