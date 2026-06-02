using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using SoulSpeech.Content.Projectiles;

namespace SoulSpeech.Content.Items.Weapons.Melee.Swords
{
    internal class DemonSlayerSword : ModItem
    {
        // 挥砍状态
        private bool swingDown = true;  // 控制上下挥砍方向
        private bool swingArmed = true; // “武装锁”：一刀挥完才允许下一次翻转，防止重复翻转
        private int prevAnimation;      // 上一帧 itemAnimation，用于检测“新一次挥砍”的上升沿
        private float appliedTrueAngle; // 本帧真实挥砍角（=可见刀刃方向），供判定框 UseItemHitbox 复用

        // 挥砍上下限（真实角度，度）：0=正前方，负=上方，正=下方。判定与视觉都以此为准。
        private const float SwingTopAngle = -110f;    // 上限（向上挥到这里）
        private const float SwingBottomAngle = 110f;  // 下限（向下挥到这里）

        // 贴图旋正修正（度）：贴图本身倾斜，itemRotation 要在真实角上加这个量刀刃才朝对方向。
        // 若你把 PNG 旋正了，把它改成 0；当前 45 用于复现现有视觉而不动贴图。
        private const float SpriteCorrection = 45f;

        private const float HitboxReach = 40f; // 判定框中心距玩家中心的距离（像素）
        private const int HitboxSize = 60;     // 判定框边长（像素）

        public override void SetDefaults()
        {
            Item.damage = 24;
            Item.DamageType = DamageClass.Melee;

            Item.width = 75;
            Item.height = 75;

            // XX 帧 = 前段扫弧 + 后段端点停顿；改这里调整整体挥舞节奏
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Swing;

            Item.knockBack = 4f;
            Item.value = Item.sellPrice(silver: 20);
            Item.rare = ItemRarityID.Blue;

            Item.autoReuse = true;

            Item.noMelee = false;       // 伤害来自武器本体
            Item.noUseGraphic = false;  // 显示武器贴图
        }

        // 自定义挥舞：上下交替的弧线
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            if (!player.ItemAnimationActive)
            {
                // 闲置时清零计数器并重新武装，保证下一次起手能翻转
                prevAnimation = 0;
                swingArmed = true;
                return;
            }

            // 一刀挥到接近结束时重新武装（动画必须真正跑到底，才允许下一次翻转）
            if (player.itemAnimation <= 1)
                swingArmed = true;

            // 已武装 + 上升沿 = 一次新挥砍：翻转方向并朝鼠标转身，翻转后立即解除武装
            if (swingArmed && player.itemAnimation > prevAnimation)
            {
                if (Main.myPlayer == player.whoAmI)
                    player.ChangeDir(Main.MouseWorld.X > player.Center.X ? 1 : -1);

                swingDown = !swingDown;
                swingArmed = false;
            }
            prevAnimation = player.itemAnimation;

            // 挥砍进度：0（动画开始）→ 1（动画结束）
            float progress = 1f - player.itemAnimation / (float)player.itemAnimationMax;

            // 挥砍端点用真实角度（可见刀刃方向 = 判定方向）
            float topAngle = MathHelper.ToRadians(SwingTopAngle);
            float bottomAngle = MathHelper.ToRadians(SwingBottomAngle);

            // swingDown=true：上 → 下劈；false：下 → 上挥
            float start = swingDown ? topAngle : bottomAngle;
            float end = swingDown ? bottomAngle : topAngle;

            // 前 swingFraction 比例扫过弧线，剩余比例定格端点 → 明显停顿
            const float swingFraction = 0.98f;
            float sweepT = MathHelper.Clamp(progress / swingFraction, 0f, 1f);
            float eased = MathHelper.SmoothStep(0f, 1f, sweepT); // 缓入缓出，强化端点停顿
            float trueAngle = MathHelper.Lerp(start, end, eased);
            appliedTrueAngle = trueAngle; // 记录真实角供判定框对齐刀刃

            // 贴图倾斜：itemRotation 在真实角上加修正，倾斜的贴图才朝向 trueAngle
            float itemRotAngle = trueAngle + MathHelper.ToRadians(SpriteCorrection);
            player.itemRotation = itemRotAngle * player.direction; // 面向左时镜像
            player.itemLocation = player.MountedCenter;
            player.SetCompositeArmFront(
                true,
                Player.CompositeArmStretchAmount.Full,
                (itemRotAngle - MathHelper.PiOver2) * player.direction
            );
        }

        // 让近战判定框跟随可见刀刃：原版判定跟的是 itemLocation（固定在手），所以必须每帧重定位。
        // 方向直接用真实挥砍角（无偏移补丁），面向左时镜像 X，上下不变。
        public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
        {
            Vector2 bladeDir = new Vector2(
                (float)Math.Cos(appliedTrueAngle) * player.direction,
                (float)Math.Sin(appliedTrueAngle)
            );

            Vector2 center = player.MountedCenter + bladeDir * HitboxReach;
            hitbox = new Rectangle(
                (int)(center.X - HitboxSize / 2f),
                (int)(center.Y - HitboxSize / 2f),
                HitboxSize,
                HitboxSize
            );
        }

        public override bool? UseItem(Player player)
        {
            // 只在动画第一帧执行
            if (player.itemAnimation != player.itemAnimationMax)
                return null;

            if (Main.myPlayer != player.whoAmI)
                return null;

            // 朝鼠标方向转身（左/右），让剑气方向跟随光标
            player.ChangeDir(Main.MouseWorld.X > player.Center.X ? 1 : -1);

            int dir = player.direction;

            // 发射剑气（初速度越大飞得越远）
            Vector2 spawnPos = player.Center + new Vector2(30 * dir, -4f);
            Vector2 velocity = new Vector2(20f * dir, 0f);

            Projectile.NewProjectile(
                player.GetSource_ItemUse(Item),
                spawnPos,
                velocity,
                ModContent.ProjectileType<DemonSlayerProj>(),
                Item.damage,
                Item.knockBack,
                player.whoAmI
            );

            return null;
        }

        public override Vector2? HoldoutOffset()
        {
            // X坐标往里移动10像素，Y坐标向上移动5像素
            return new Vector2(-120, -120);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.DemoniteBar, 15)
                .AddIngredient(ItemID.ShadowScale, 20)
                .AddTile(TileID.Anvils)
                .Register();

            CreateRecipe()
                .AddIngredient(ItemID.CrimtaneBar, 15)
                .AddIngredient(ItemID.TissueSample, 20)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
