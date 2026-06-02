using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace SoulSpeech.Content.Items.Weapons.Melee.Spears
{
    internal class MoltenSpear : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 39;
            Item.DamageType = DamageClass.Melee;

            Item.width = 74;
            Item.height = 74;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 32;
            Item.useAnimation = 32;

            Item.noUseGraphic = true;
            Item.noMelee = true;

            Item.knockBack = 9f;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold: 1);

            Item.shoot = ModContent.ProjectileType<Projectiles.MoltenSpearProj>();
            Item.shootSpeed = 7f;

            Item.autoReuse = true;
        }

        public override bool CanUseItem(Player player)
        {
            // 同一时间只能存在一支矛
            return player.ownedProjectileCounts[Item.shoot] == 0;
        }
        public override bool Shoot(
            Player player,
            EntitySource_ItemUse_WithAmmo source,
            Vector2 position,
            Vector2 velocity,
            int type,
            int damage,
            float knockback)
        {
            // 让水矢略微偏转，模拟你 1.3 时期的效果
            float rotation = Main.rand.NextBool()
                ? Main.rand.NextFloat(0f, 0.10f)
                : -Main.rand.NextFloat(0f, 0.10f);

            velocity = velocity.RotatedBy(rotation);

            Projectile.NewProjectile(
                source,
                position,
                velocity,
                type,
                damage,
                knockback,
                player.whoAmI
            );

            return false; // 我们手动生成弹幕
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.HellstoneBar, 10)
                .AddIngredient(ItemID.DarkLance, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}