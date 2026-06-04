using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Items.Weapons.Ranged.Guns
{
    // 龙灼：焰龙的升级版。每次射击有 30% 概率触发散射（5 发，小角度偏移），
    // 另有 20% 概率伴随发射浴火矢（原版 InfernoFriendlyBolt / 射弹 295）。
    internal class Dragonblaze : ModItem
    {
        private const float ScatterChance = 0.3f;  // 散射触发概率
        private const int ScatterCount = 5;         // 散射子弹数量
        private const float ScatterSpread = 8f;     // 散射单发角度偏移上限（度）
        private const float InfernoChance = 0.2f;   // 浴火矢触发概率

        public override void SetDefaults()
        {
            Item.damage = 32;
            Item.DamageType = DamageClass.Ranged;
            Item.crit = 8;

            Item.width = 50;
            Item.height = 30;

            Item.useTime = 14;
            Item.useAnimation = 14;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;

            Item.knockBack = 3f;
            Item.value = Item.buyPrice(gold: 20);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item41;

            Item.useAmmo = AmmoID.Bullet;
            Item.shoot = ProjectileID.PurificationPowder; // 占位，实际由弹药决定
            Item.shootSpeed = 15f;

            Item.noMelee = true;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-4f, 0f);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 正常发射当前弹药射弹
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

            // 30% 概率触发一次散射
            if (Main.rand.NextFloat() < ScatterChance)
            {
                for (int i = 0; i < ScatterCount; i++)
                {
                    Vector2 scatterVel = velocity.RotatedByRandom(MathHelper.ToRadians(ScatterSpread));
                    Projectile.NewProjectile(source, position, scatterVel, type, damage, knockback, player.whoAmI);
                }
            }

            // 20% 概率伴随发射一发浴火矢（原版射弹 295）
            if (Main.rand.NextFloat() < InfernoChance)
            {
                Projectile.NewProjectile(
                    source,
                    position,
                    velocity,
                    ProjectileID.InfernoFriendlyBolt,
                    damage,
                    knockback,
                    player.whoAmI
                );
            }

            return false; // 已手动发射
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<FlameDragon>(1)
                .AddIngredient(ItemID.HallowedBar, 12)
                .AddIngredient(ItemID.SoulofFright, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
