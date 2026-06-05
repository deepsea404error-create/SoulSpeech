using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Items.Weapons.Ranged.Guns
{
    // 幽影：远程突击步枪。发射方式与发条式突击步枪相同：每次扣动发射 3 发子弹。
    // 区别在于：每第 3 次发射（即每第 9 发子弹）追加 1 发 150% 伤害的暗影光束。
    internal class Shadow : ModItem
    {
        private const int ShotsPerBurst = 3;   // 每次扣动发射 3 发子弹（与发条式突击步枪一致）
        private const int BeamEveryNShots = 9; // 每 9 发子弹追加 1 枚暗影光束

        private int shotCounter;

        public override void SetDefaults()
        {
            Item.damage = 39;
            Item.DamageType = DamageClass.Ranged;
            Item.knockBack = 0f;
            Item.crit = 4;

            Item.width = 92;
            Item.height = 64;

            Item.useTime = 6;                      // 单发射弹间隔
            Item.useAnimation = 6 * ShotsPerBurst; // 36：3 发为一次扣动
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;

            Item.value = Item.buyPrice(gold: 8);
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item41;

            Item.useAmmo = AmmoID.Bullet;
            Item.shoot = ProjectileID.PurificationPowder; // 占位，实际由弹药决定
            Item.shootSpeed = 12f;

            Item.noMelee = true;
        }

        // 枪口偏移，让子弹从枪管射出
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-8f, -2f);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 正常发射当前弹药射弹
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

            // 每第 9 发子弹（即每第 3 次扣动）追加 1 枚 150% 伤害的暗影光束
            shotCounter++;
            if (shotCounter >= BeamEveryNShots)
            {
                shotCounter = 0;
                Projectile.NewProjectile(
                    source,
                    position,
                    velocity,
                    ProjectileID.ShadowBeamFriendly,
                    (int)(damage * 1.5f),
                    knockback,
                    player.whoAmI
                );
            }

            return false; // 已手动发射
        }
    }
}
