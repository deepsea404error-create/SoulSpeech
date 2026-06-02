using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SoulSpeech.Content.Items.Materials;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using SoulSpeech.Common.Players;
using SoulSpeech.Content.Projectiles;
using Terraria.DataStructures;

namespace SoulSpeech.Content.Items.Weapons.Melee.Boomerangs
{
    internal class BlueCrystalBoomerang : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("蓝晶回旋镖");
            // Tooltip.SetDefault("最多可同时存在两个回旋镖");
        }

        public override void SetDefaults()
        {
            Item.damage = 22;
            Item.DamageType = DamageClass.Melee;
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4f;
            Item.value = Item.buyPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item1;

            Item.noUseGraphic = true;
            Item.noMelee = true;

            Item.shoot = ModContent.ProjectileType<Projectiles.BlueCrystalBoomerangProj>();
            Item.shootSpeed = 12f;

            Item.autoReuse = true;
            Item.crit = 4;
        }

        public override bool CanUseItem(Player player)
        {
            // 核心：最多允许 3 个存在
            int count = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active &&
                    proj.owner == player.whoAmI &&
                    proj.type == Item.shoot)
                {
                    count++;
                }
            }
            return count < 5;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<Items.Materials.SoulSpirit>(), 10)
                .AddIngredient(ItemID.MythrilBar, 12)   // 或 OrichalcumBar
                .AddIngredient(ItemID.ShadowScale, 7)   // 或 TissueSample
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}