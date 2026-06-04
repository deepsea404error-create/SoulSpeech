using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using SoulSpeech.Content.Projectiles;

namespace SoulSpeech.Content.Items.Weapons.Magic.Books
{
    // 天羽书：法师武器。每次施放发射 3 枚天羽射弹，呈小角度扇形散开。
    internal class SkyFeatherTome : ModItem
    {
        private const int ShotCount = 3;          // 每次发射的射弹数量
        private const float SpreadDegrees = 4f;   // 扇形半张角（度）
        private const float JitterDegrees = 2f;   // 单发随机抖动上限（度）

        public override void SetDefaults()
        {
            Item.damage = 12;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 5;
            Item.crit = 8;

            Item.width = 32;
            Item.height = 32;

            Item.useTime = 28;       // 速度适中
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;

            Item.knockBack = 2f;
            Item.value = Item.buyPrice(silver: 75);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item43; // 法术书音效

            Item.shoot = ModContent.ProjectileType<SkyFeatherProj>();
            Item.shootSpeed = 18f;

            Item.noMelee = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float spread = MathHelper.ToRadians(SpreadDegrees);

            for (int i = 0; i < ShotCount; i++)
            {
                // 在 [-spread, +spread] 间均匀分布（3 发 → -8°, 0°, +8°），再叠加小幅随机抖动
                float t = ShotCount > 1 ? (float)i / (ShotCount - 1) : 0.5f;
                float rot = MathHelper.Lerp(-spread, spread, t);
                Vector2 v = velocity.RotatedBy(rot).RotatedByRandom(MathHelper.ToRadians(JitterDegrees));

                Projectile.NewProjectile(source, position, v, type, damage, knockback, player.whoAmI);
            }

            return false; // 已手动发射
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Feather, 5)
                .AddIngredient(ItemID.Book, 1)
                .AddIngredient(ItemID.FallenStar, 5)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
