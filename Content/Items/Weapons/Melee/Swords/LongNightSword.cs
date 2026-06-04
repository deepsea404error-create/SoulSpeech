using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using SoulSpeech.Content.Projectiles;

namespace SoulSpeech.Content.Items.Weapons.Melee.Swords
{
    internal class LongNightSword : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 94;
            Item.DamageType = DamageClass.Melee;

            Item.width = 100;
            Item.height = 100;

            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Swing;

            Item.knockBack = 5f;
            Item.value = Item.buyPrice(gold: 20);
            Item.rare = ItemRarityID.Pink;

            Item.autoReuse = true;
            Item.noMelee = false;

            Item.scale = 0.5f;

            Item.shoot = ModContent.ProjectileType<LongNightAura>();
            Item.shootSpeed = 26f;
        }

        // HoldoutOffset 对 Swing 武器无效，必须通过 UseStyle 控制握持位置
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (!player.ItemAnimationActive) return;

            float progress = 1f - player.itemAnimation / (float)player.itemAnimationMax;
            float angle = MathHelper.Lerp(
                MathHelper.ToRadians(-90f),
                MathHelper.ToRadians(80f),
                MathHelper.SmoothStep(0f, 1f, progress)
            );

            player.itemRotation = angle * player.direction;

            // 握持点在贴图中心的局部偏移（用户确认值）：(-21*dir, 21)
            // 挥舞时将该偏移随旋转角旋转，再从手部位置反推贴图中心 (itemLocation)，
            // 使握持点在整个挥舞过程中始终固定在玩家手部
            float gripDX = 21f * player.direction;
            float gripDY = -21f;
            float cos = (float)Math.Cos(player.itemRotation);
            float sin = (float)Math.Sin(player.itemRotation);
            Vector2 rotatedGrip = new(
                gripDX * cos - gripDY * sin,
                gripDX * sin + gripDY * cos
            );
            player.itemLocation = player.MountedCenter - rotatedGrip;

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full,
                (angle - MathHelper.PiOver2) * player.direction);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 dir = (Main.MouseWorld - player.Center).SafeNormalize(velocity);
            Projectile.NewProjectile(
                source,
                player.Center + new Vector2(30f * player.direction, -8f),
                dir * Item.shootSpeed,
                type, damage, knockback,
                player.whoAmI
            );
            return false;
        }

        // 完全接管掉落物绘制：引擎默认按「贴图底边对齐碰撞框底边」定位，
        // 256x256 贴图会被推到碰撞框下方导致下沉。这里以底边中心为原点手动锚定。
        public override bool PreDrawInWorld(SpriteBatch spriteBatch,
            Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D tex = TextureAssets.Item[Type].Value;
            const float drawScale = 0.5f;

            // 原点取贴图底边中心，绘制位置取物品碰撞框底边中心，
            // 使缩放后的贴图底边恰好落在地面上
            Vector2 origin = new(tex.Width / 2f, tex.Height);
            Vector2 drawPos = new(
                Item.position.X - Main.screenPosition.X + Item.width / 2f,
                Item.position.Y - Main.screenPosition.Y + Item.height
            );

            spriteBatch.Draw(tex, drawPos, null, lightColor, rotation, origin, drawScale,
                SpriteEffects.None, 0f);

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.HallowedBar, 15)
                .AddIngredient(ItemID.SoulofNight, 10)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
