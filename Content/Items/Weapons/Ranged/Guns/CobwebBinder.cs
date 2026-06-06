using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using SoulSpeech.Content.Projectiles;

namespace SoulSpeech.Content.Items.Weapons.Ranged.Guns
{
    // 蛛网束缚者：霰弹枪，每次 3-4 发子弹，每 2 次发射伴随 1 枚蛛网弹。
    internal class CobwebBinder : ModItem
    {
        private int shotCounter;

        public override void SetDefaults()
        {
            Item.damage = 13;
            Item.DamageType = DamageClass.Ranged;
            Item.crit = 4;

            Item.width = 46;
            Item.height = 28;

            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;

            Item.knockBack = 2f;
            Item.value = Item.buyPrice(silver: 80);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item36;

            Item.useAmmo = AmmoID.Bullet;
            Item.shoot = ProjectileID.Bullet; // 占位，实际由弹药决定
            Item.shootSpeed = 8f;

            Item.noMelee = true;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-4f, 0f);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 3-4 发子弹，12° 散开
            int bulletCount = Main.rand.Next(3, 5);
            for (int i = 0; i < bulletCount; i++)
            {
                Vector2 bulletVel = velocity.RotatedByRandom(MathHelper.ToRadians(12f));
                Projectile.NewProjectile(source, position, bulletVel, type, damage, knockback, player.whoAmI);
            }

            // 每 2 次发射伴随 1 枚蛛网弹
            shotCounter++;
            if (shotCounter >= 2)
            {
                shotCounter = 0;
                Projectile.NewProjectile(
                    source,
                    position,
                    velocity,
                    ModContent.ProjectileType<CobwebBullet>(),
                    damage,
                    knockback,
                    player.whoAmI
                );
            }

            return false; // 已手动发射
        }
    }
}
