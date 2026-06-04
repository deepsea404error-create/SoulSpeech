using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Items.Weapons.Ranged.Guns
{
    // 焰龙：快速手枪。每第 5 次射击后额外发射一次散射（5 发子弹，小范围角度偏移）。
    internal class FlameDragon : ModItem
    {
        private const int ScatterEvery = 5;   // 每 5 次射击触发一次散射
        private const int ScatterCount = 5;    // 散射子弹数量
        private const float ScatterSpread = 8f; // 散射单发角度偏移上限（度）

        // 发射计数：每第 5 次发射伴随散射
        private int shotCounter;

        public override void SetDefaults()
        {
            Item.damage = 24;
            Item.DamageType = DamageClass.Ranged;
            Item.crit = 4;

            Item.width = 50;
            Item.height = 30;

            Item.useTime = 14;       // 很快速度
            Item.useAnimation = 14;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;

            Item.knockBack = 2f;     // 很弱击退力
            Item.value = Item.buyPrice(gold: 8);
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item41;

            Item.useAmmo = AmmoID.Bullet;
            Item.shoot = ProjectileID.PurificationPowder; // 占位，实际由弹药决定
            Item.shootSpeed = 13f;

            Item.noMelee = true;
        }

        // 枪口偏移，让子弹从枪管射出
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-4f, 0f);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 正常发射当前弹药射弹
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

            // 每第 5 次射击后额外发射一次散射
            shotCounter++;
            if (shotCounter >= ScatterEvery)
            {
                shotCounter = 0;
                for (int i = 0; i < ScatterCount; i++)
                {
                    Vector2 scatterVel = velocity.RotatedByRandom(MathHelper.ToRadians(ScatterSpread));
                    Projectile.NewProjectile(source, position, scatterVel, type, damage, knockback, player.whoAmI);
                }
            }

            return false; // 已手动发射
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.HellstoneBar, 12)
                .AddIngredient(ItemID.IllegalGunParts, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
