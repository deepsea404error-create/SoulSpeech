using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using SoulSpeech.Content.Projectiles;

namespace SoulSpeech.Content.Items.Weapons.Ranged.Bows
{
    // 奈特之弦：每次发射 2 支箭矢（小角度散开）；每 3 次发射追加 1 枚沙暴矢，命中产生 2 秒沙暴。
    internal class KnightBow : ModItem
    {
        private int shotCounter;

        public override void SetDefaults()
        {
            Item.damage = 46;
            Item.DamageType = DamageClass.Ranged;
            Item.crit = 4;

            Item.width = 48;
            Item.height = 92;

            Item.useTime = 17;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;

            Item.knockBack = 2f;
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

            // 每次发射 2 支箭，小角度散开，发射点上下偏移
            for (int i = 0; i < 2; i++)
            {
                Vector2 arrowVel = velocity.RotatedByRandom(MathHelper.ToRadians(3f));
                float offset = (i - 0.5f) * 6f; // -3, +3 垂直偏移
                Vector2 spawnPos = position + perpendicular * offset;
                Projectile.NewProjectile(source, spawnPos, arrowVel, type, damage, knockback, player.whoAmI);
            }

            // 每 3 次发射追加 1 枚沙暴矢（在本次 2 支箭之后紧接发射），速度为普通箭矢 2 倍
            shotCounter++;
            if (shotCounter >= 3)
            {
                shotCounter = 0;
                Projectile.NewProjectile(source, position, velocity * 2f,
                    ModContent.ProjectileType<SandstormArrow>(), damage, knockback, player.whoAmI);
            }

            return false; // 已手动发射
        }
    }
}
