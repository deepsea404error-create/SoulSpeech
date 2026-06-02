using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using SoulSpeech.Common.BaseItems;
using SoulSpeech.Common;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using SoulSpeech.Content.Projectiles;

namespace SoulSpeech.Content.Items.Weapons.Melee.Swords
{
    internal class DemonSlayerSword : StageWeapon
    {
        // 这是“骷髅王后 - 肉山前”的武器
        protected override WeaponStage Stage => WeaponStage.Skeletron;

        private bool swingDown = true; // 控制上下挥砍

        // 不带任何倍率的基础伤害
        protected override int BaseDamage => 20;

        public override void SetDefaults()
        {
            // 先应用倍率计算伤害
            ApplyDamageScaling();

            Item.DamageType = DamageClass.Melee;

            Item.width = 75;
            Item.height = 75;

            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Swing;



            Item.knockBack = 4f;
            Item.value = Item.sellPrice(silver: 20);
            Item.rare = ItemRarityID.Blue;

            Item.autoReuse = true;

            Item.noMelee = false;       // 伤害来自武器本体
            Item.noUseGraphic = false;  // 显示武器贴图
        }

        public override bool? UseItem(Player player)
        {
            // 只在动画第一帧执行
            if (player.itemAnimation != player.itemAnimationMax)
                return null;

            if (Main.myPlayer != player.whoAmI)
                return null;

            int dir = player.direction;

            Vector2 spawnPos = player.Center + new Vector2(30 * dir, -4f);
            Vector2 velocity = new Vector2(12f * dir, 0f);

            Projectile.NewProjectile(
                player.GetSource_ItemUse(Item),
                spawnPos,
                velocity,
                ModContent.ProjectileType<DemonSlayerProj>(),
                Item.damage,
                Item.knockBack,
                player.whoAmI
            );

            return null;
        }

        public override Vector2? HoldoutOffset()
        {
            // X坐标往里移动10像素，Y坐标向上移动5像素
            return new Vector2(-120, -120);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.DemoniteBar, 15)
                .AddIngredient(ItemID.ShadowScale, 20)
                .AddTile(TileID.Anvils)
                .Register();

            CreateRecipe()
                .AddIngredient(ItemID.CrimtaneBar, 15)
                .AddIngredient(ItemID.TissueSample, 20)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
