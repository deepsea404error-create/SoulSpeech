using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SoulSpeech.Common.BaseItems;
using SoulSpeech.Common;
using SoulSpeech.Content.Items.Materials;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace SoulSpeech.Content.Items.Weapons.Melee.Spears
{

    public class BlueCrystalSpear : StageWeapon
    {
        // 这是“骷髅王后 - 肉山前”的武器
        protected override WeaponStage Stage => WeaponStage.Skeletron;

        // 不带任何倍率的基础伤害
        protected override int BaseDamage => 22;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            // 先应用倍率计算伤害
            ApplyDamageScaling();

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 18;
            Item.useTime = 25;

            Item.knockBack = 6.5f;
            Item.width = 32;
            Item.height = 32;

            Item.noMelee = true;        // 使用弹幕造成伤害
            Item.noUseGraphic = true;   // 不显示物品本体
            Item.autoReuse = true;

            Item.DamageType = DamageClass.Melee;
            Item.UseSound = SoundID.Item1;

            Item.shoot = ModContent.ProjectileType<Projectiles.BlueCrystalSpearProj>();
            Item.shootSpeed = 8f;

            Item.rare = ItemRarityID.Green;
            Item.value = Item.buyPrice(silver: 15);
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
                ? Main.rand.NextFloat(0.05f, 0.20f)
                : -Main.rand.NextFloat(0.05f, 0.20f);

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
                .AddIngredient<SoulSpirit>(15)
                .AddIngredient(ItemID.FallenStar, 5)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
