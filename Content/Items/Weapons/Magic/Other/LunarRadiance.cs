using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using SoulSpeech.Content.Projectiles;

namespace SoulSpeech.Content.Items.Weapons.Magic.Other
{
    // 月华:法师武器。发射中心直线弹 + 缠绕螺旋弹(命中生成月相)。
    internal class LunarRadiance : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 107;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;

            Item.width = 40;
            Item.height = 40;

            Item.useTime = 14;
            Item.useAnimation = 14;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;

            Item.knockBack = 3f;
            Item.value = Item.sellPrice(gold: 10);
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item9;

            Item.noMelee = true;
            Item.shoot = ModContent.ProjectileType<LunarCenterBolt>(); // 占位,实际在 Shoot 手动发射
            Item.shootSpeed = 24f;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-6f, 0f);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 三档伤害在此一处算出,随魔法加成正确缩放
            int centerDamage = damage;                 // 中心弹
            int spiralDamage = (int)(damage * 0.7f);   // 螺旋弹
            int phaseDamage = damage;                  // 月相:100%

            // 中心直线弹
            Projectile.NewProjectile(source, position, velocity,
                ModContent.ProjectileType<LunarCenterBolt>(), centerDamage, knockback, player.whoAmI);

            // 螺旋弹:同位置同速度缠绕中心线;ai[0] 携带月相伤害
            Projectile.NewProjectile(source, position, velocity,
                ModContent.ProjectileType<LunarSpiral>(), spiralDamage, knockback, player.whoAmI,
                phaseDamage);

            return false; // 已手动发射
        }

        // 暂不提供合成配方
    }
}
