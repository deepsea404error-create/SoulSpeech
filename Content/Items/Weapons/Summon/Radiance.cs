using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using SoulSpeech.Content.Buffs;
using SoulSpeech.Content.Projectiles.Summon;

namespace SoulSpeech.Content.Items.Weapons.Summon
{
    public class Radiance : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.damage = 50;
            Item.DamageType = DamageClass.Summon;
            Item.mana = 0;
            Item.width = 50;
            Item.height = 124;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4f;
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.value = Item.buyPrice(gold: 5);
            Item.rare = ItemRarityID.Yellow;
            Item.UseSound = SoundID.Item44;

            Item.shoot = ModContent.ProjectileType<RadianceMinion>();
            Item.buffType = ModContent.BuffType<RadianceMinionBuff>();
            Item.shootSpeed = 10f;
        }
        // public override Vector2? HoldoutOffset()
        // {
        //     return new Vector2(-6f, -6f);
        // }

        // 保留原版 Swing 挥舞动作(挥舞弧线由原版每帧算好)，
        // 这里用 += 在原版算出的旋转基础上再叠加：让贴图整体顺时针多转 45° 后挥舞。
        // UseStyle 在原版挥舞旋转之后调用，所以 += 能生效；* direction 保证朝左时镜像一致。
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            // 把挥舞位置向内(贴近身体)再上移一点，更像用手握着挥：
            // x 乘 direction 保证朝左时也是向内偏移；y 向上固定 -6。
            player.itemLocation += new Vector2(-25f * player.direction, 0f);
            player.itemRotation += MathHelper.PiOver4 * player.direction;
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
