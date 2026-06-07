using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using SoulSpeech.Content.Buffs;
using SoulSpeech.Content.Projectiles.Summon;

namespace SoulSpeech.Content.Items.Weapons.Summon
{
    public class BlueBoneStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 21;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 0;
            Item.width = 38;
            Item.height = 38;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 1.5f;
            Item.noMelee = true;
            Item.autoReuse = false;
            Item.value = Item.buyPrice(silver: 50);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item44;

            Item.shoot = ModContent.ProjectileType<BlueSkull>();
            Item.buffType = ModContent.BuffType<BlueSkullMinionBuff>();
            Item.shootSpeed = 0f;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = player.Center - new Vector2(0f, 20f);
            velocity = Vector2.Zero;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, Main.myPlayer);
            projectile.originalDamage = Item.damage;

            return false;
        }
    }
}
