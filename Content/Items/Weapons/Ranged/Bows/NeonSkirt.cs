using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using SoulSpeech.Content.Projectiles;

namespace SoulSpeech.Content.Items.Weapons.Ranged.Bows
{
    // 霓裳：发射 3 支箭，同时从天落下 3 发霓裳矢（仿狂星之怒下落逻辑）。
    internal class NeonSkirt : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 64;
            Item.DamageType = DamageClass.Ranged;
            Item.crit = 6;

            Item.width = 38;
            Item.height = 86;

            Item.useTime = 24;
            Item.useAnimation = 24;
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

            // 仿狂星之怒：3 发霓裳矢从天空落下
            Vector2 cursorPos = Main.MouseWorld;
            float targetY = cursorPos.Y;
            // 光标 Y 不低于玩家上方 200 像素（仿 Star Wrath）
            if (targetY > player.Center.Y - 200f)
                targetY = player.Center.Y - 200f;

            for (int i = 0; i < 3; i++)
            {
                // 生成位置：玩家上方 600-800，水平随机 ±200
                Vector2 spawnPos = player.Center + new Vector2(
                    Main.rand.Next(-200, 201) * player.direction,
                    -600f - 100f * i
                );

                Vector2 starVel = cursorPos - spawnPos;
                if (starVel.Y < 0f)
                    starVel.Y *= -1f;
                if (starVel.Y < 20f)
                    starVel.Y = 20f;
                starVel = starVel.SafeNormalize(Vector2.UnitY) * Item.shootSpeed;
                starVel.Y += Main.rand.Next(-40, 41) * 0.02f;

                // ai[0] 初始 0（追踪状态机），ai[1] 为目标 Y（仿 Star Wrath 的 alpha 淡入参考）
                Projectile.NewProjectile(source, spawnPos, starVel,
                    ModContent.ProjectileType<NeonSkirtProj>(), damage, knockback,
                    player.whoAmI, 0f, targetY);
            }

            return false; // 已手动发射
        }
    }
}
