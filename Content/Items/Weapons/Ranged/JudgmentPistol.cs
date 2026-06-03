using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using SoulSpeech.Content.Projectiles;

namespace SoulSpeech.Content.Items.Weapons.Ranged
{
    internal class JudgmentPistol : ModItem
    {
        // 发射计数：每第 4 次发射伴随裁决弹
        private int shotCounter;

        public override void SetDefaults()
        {
            Item.damage = 93;
            Item.DamageType = DamageClass.Ranged;

            Item.width = 50;
            Item.height = 30;

            Item.useTime = 14;       // 攻速较快
            Item.useAnimation = 14;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;

            Item.knockBack = 2f;
            Item.value = Item.buyPrice(gold: 25);
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item41;

            Item.useAmmo = AmmoID.Bullet;
            Item.shoot = ProjectileID.PurificationPowder; // 占位，实际由弹药决定
            Item.shootSpeed = 12f;

            Item.noMelee = true;
        }

        // 40% 概率不消耗弹药
        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            return Main.rand.NextFloat() >= 0.4f;
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

            // 每第 3 次发射额外伴随一发裁决弹
            shotCounter++;
            if (shotCounter >= 3)
            {
                shotCounter = 0;
                Projectile.NewProjectile(
                    source,
                    position,
                    velocity,
                    ModContent.ProjectileType<JudgmentBolt>(),
                    damage * 2,    // 裁决弹造成 200% 射击伤害
                    knockback,
                    player.whoAmI
                );
            }

            return false; // 已手动发射
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.HallowedBar, 12)
                .AddIngredient(ItemID.SoulofLight, 8)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
