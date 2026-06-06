using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using SoulSpeech.Content.Projectiles;

namespace SoulSpeech.Content.Items.Weapons.Melee.Swords
{
    // 蛛网剑：近战剑，每 2 次挥击后追加 1 枚蛛网弹（命中产生持续 1 秒的蛛网）。
    internal class CobwebSword : ModItem
    {
        private int shotCounter;

        public override void SetDefaults()
        {
            Item.damage = 27;
            Item.DamageType = DamageClass.Melee;
            Item.crit = 4;

            Item.width = 46;
            Item.height = 28;

            Item.useTime = 21;
            Item.useAnimation = 21;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.autoReuse = true;

            Item.knockBack = 3f;
            Item.value = Item.buyPrice(silver: 50);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item1;

            Item.shoot = ModContent.ProjectileType<CobwebBullet>(); // 占位，触发 Shoot，由 Shoot 中手动发射
            Item.shootSpeed = 15f;

            Item.noMelee = false;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 每 2 次挥击追加 1 枚蛛网弹
            shotCounter++;
            if (shotCounter >= 2)
            {
                shotCounter = 0;
                Vector2 dir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX * player.direction);
                Vector2 spawnPos = player.Center + dir * 30f;
                Projectile.NewProjectile(
                    source,
                    spawnPos,
                    dir * Item.shootSpeed,
                    ModContent.ProjectileType<CobwebBullet>(),
                    damage,
                    knockback,
                    player.whoAmI
                );
            }
            return false; // 阻止默认射弹生成（每次挥击只挥砍，不发弹）
        }
    }
}
