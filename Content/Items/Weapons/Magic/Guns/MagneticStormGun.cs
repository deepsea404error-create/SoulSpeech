using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Items.Weapons.Magic.Guns
{
    // 磁暴枪：法师枪械。每次发射 3 枚激光；每第 3 次额外发射 1 枚 250% 伤害的电圈导弹。
    internal class MagneticStormGun : ModItem
    {
        private const float SpreadDegrees = 6f; // 激光散开角度上限（度）

        private int shotCounter;

        public override void SetDefaults()
        {
            Item.damage = 60;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 6;
            Item.crit = 4;

            Item.width = 67;
            Item.height = 44;

            Item.useTime = 16;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;

            Item.knockBack = 2f;
            Item.value = Item.buyPrice(gold: 7);
            Item.rare = ItemRarityID.LightPurple;
            Item.UseSound = SoundID.Item9;

            Item.shoot = ProjectileID.ChargedBlasterCannon; // 占位，实际在 Shoot 手动发射
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
            // 每次发射 3 枚激光，小角度散开
            for (int i = 0; i < 3; i++)
            {
                Vector2 spreadVel = velocity.RotatedByRandom(MathHelper.ToRadians(SpreadDegrees));
                Projectile.NewProjectile(source, position, spreadVel,
                    ProjectileID.LaserMachinegunLaser, damage, knockback, player.whoAmI);
            }

            // 每第 3 次额外发射 1 枚 250% 伤害的电圈导弹
            shotCounter++;
            if (shotCounter >= 5)
            {
                shotCounter = 0;
                Projectile.NewProjectile(source, position, velocity,
                    ProjectileID.ElectrosphereMissile, (int)(damage * 2f), knockback, player.whoAmI);
            }

            return false; // 已手动发射
        }
    }
}
