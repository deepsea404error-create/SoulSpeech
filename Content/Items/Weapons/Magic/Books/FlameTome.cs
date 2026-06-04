using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Items.Weapons.Magic.Books
{
    // 焚炎书：法师武器。80% 概率发射小鬼火球，20% 概率发射浴火矢，均为原版射弹。
    internal class FlameTome : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 31;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 8;
            Item.crit = 6;

            Item.width = 32;
            Item.height = 32;

            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;

            Item.knockBack = 3f;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item20; // 法术音效

            Item.shoot = ProjectileID.ImpFireball; // 默认射弹，Shoot 内按概率覆盖
            Item.shootSpeed = 20f;

            Item.noMelee = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 33% 浴火矢（1.5 倍伤害），66% 小鬼火球
            bool inferno = Main.rand.NextBool(3);
            type = inferno ? ProjectileID.InfernoFriendlyBolt : ProjectileID.ImpFireball;
            if (inferno)
                damage = (int)(damage * 1.5f);

            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);

            return false; // 已手动发射
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.HellstoneBar, 10)
                .AddIngredient(ItemID.Book, 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
