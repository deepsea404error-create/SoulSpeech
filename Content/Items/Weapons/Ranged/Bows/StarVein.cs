using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using SoulSpeech.Content.Projectiles;

namespace SoulSpeech.Content.Items.Weapons.Ranged.Bows
{
    // 星脉：每次发射 3 支箭伴随 3 发脉冲矢。
    internal class StarVein : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 78;
            Item.DamageType = DamageClass.Ranged;
            Item.crit = 11;

            Item.width = 38;
            Item.height = 86;

            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;

            Item.knockBack = 3f;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item5;

            Item.useAmmo = AmmoID.Arrow;
            Item.shoot = ProjectileID.WoodenArrowFriendly; // 占位，实际由弹药决定
            Item.shootSpeed = 12f;

            Item.noMelee = true;
        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-6f, 0f);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 垂直于射击方向的方向，用于上下偏移发射点
            Vector2 perpendicular = new Vector2(-velocity.Y, velocity.X).SafeNormalize(Vector2.Zero);

            // 发射 3 支箭，小角度散开，发射点上下偏移
            for (int i = 0; i < 3; i++)
            {
                Vector2 arrowVel = velocity.RotatedByRandom(MathHelper.ToRadians(5f));
                float offset = (i - 1) * 6f; // -6, 0, +6 垂直偏移
                Vector2 spawnPos = position + perpendicular * offset;
                Projectile.NewProjectile(source, spawnPos, arrowVel, type, damage, knockback, player.whoAmI);
            }

            // 伴随发射 3 发脉冲矢，发射点上下偏移
            for (int i = 0; i < 3; i++)
            {
                Vector2 pulseVel = velocity.RotatedByRandom(MathHelper.ToRadians(3f));
                float offset = (i - 1) * 6f; // -6, 0, +6 垂直偏移
                Vector2 spawnPos = position + perpendicular * offset;
                Projectile.NewProjectile(source, spawnPos, pulseVel, ModContent.ProjectileType<StarVeinPulse>(), damage, knockback, player.whoAmI);
            }

            return false; // 已手动发射
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.HellstoneBar, 12)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
