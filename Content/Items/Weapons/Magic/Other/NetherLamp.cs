using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using SoulSpeech.Content.Projectiles;

namespace SoulSpeech.Content.Items.Weapons.Magic.Other
{
    // 冥灯：法师武器。每次攻击发射 3~4 枚虚空弹，发射方式与裂天剑（SkyFracture）一致。
    internal class NetherLamp : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 83;
            Item.crit = 16;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 14;

            // 贴图 60×44（高×宽），单帧静态
            Item.width = 44;
            Item.height = 60;

            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot; // 握持方式 5
            Item.autoReuse = true;

            Item.knockBack = 3f;
            Item.value = Item.buyPrice(gold: 25);
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item9; // 裂天剑同款

            Item.shoot = ModContent.ProjectileType<NetherVoidBolt>();
            Item.shootSpeed = 11f;

            Item.noMelee = true;
        }

		public override Vector2? HoldoutOffset() {
			return new Vector2(-10f, 0f);
		}
        // 仿裂天剑发射：每枚弹在玩家周围随机偏移处生成，朝鼠标方向飞出
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
            Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int count = Main.rand.Next(3, 5); // 3 或 4 枚
            float speed = velocity.Length();
            if (speed < 1f)
                speed = Item.shootSpeed;

            Vector2 aimVel = velocity.SafeNormalize(Vector2.UnitY) * speed;

            for (int n = 0; n < count; n++)
            {
                // 在玩家周围随机角度、距中心 20~60px 处生成（校验不卡墙，最多重试 50 次）
                float f = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 spawn = player.Center + f.ToRotationVector2() * MathHelper.Lerp(20f, 60f, Main.rand.NextFloat());
                for (int tries = 0; tries < 50; tries++)
                {
                    spawn = player.Center + f.ToRotationVector2() * MathHelper.Lerp(20f, 60f, Main.rand.NextFloat());
                    Vector2 probe = spawn + (spawn - player.Center).SafeNormalize(Vector2.UnitX) * 8f;
                    if (Collision.CanHit(player.Center, 0, 0, probe, 0, 0))
                        break;
                    f = Main.rand.NextFloat() * MathHelper.TwoPi;
                }

                // 朝鼠标飞出，再向玩家瞄准方向 lerp 25%
                Vector2 v = (Main.MouseWorld - spawn).SafeNormalize(aimVel) * speed;
                v = Vector2.Lerp(v, aimVel, 0.25f);

                Projectile.NewProjectile(source, spawn, v, type, damage, knockback, player.whoAmI);
            }

            return false; // 已手动发射
        }
    }
}
