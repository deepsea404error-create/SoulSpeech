using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace SoulSpeech.Content.Projectiles
{
    // 魅影手里剑本体，贴图 78x78
    internal class PhantomShurikenProj : ModProjectile
    {
        // ai[1] 用作初始化标记（0 = 尚未生成外旋）
        private bool SpawnedOuter
        {
            get => Projectile.ai[1] != 0f;
            set => Projectile.ai[1] = value ? 1f : 0f;
        }

        public override void SetDefaults()
        {
            Projectile.width = 78;
            Projectile.height = 78;

            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;

            Projectile.penetrate = 1;       // 命中一个敌人后消失
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false; // 穿墙
            Projectile.ignoreWater = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            // 扔出时的蓝紫色爆散粒子
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f) + Projectile.velocity * 0.1f;
                int dust = Dust.NewDust(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.PurpleTorch, vel.X, vel.Y, 100, default, 1.3f
                );
                Main.dust[dust].noGravity = true;
            }
        }

        public override void AI()
        {
            // 持续自旋
            Projectile.rotation += 0.25f;

            // 第一帧生成外旋伴随弹幕
            if (!SpawnedOuter)
            {
                SpawnedOuter = true;

                if (Main.myPlayer == Projectile.owner)
                {
                    int outerDamage = (int)(Projectile.damage * 0.8f);
                    Projectile outer = Projectile.NewProjectileDirect(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<PhantomShurikenOuter>(),
                        outerDamage,
                        Projectile.knockBack,
                        Projectile.owner
                    );
                    outer.ai[0] = Projectile.whoAmI; // 让外旋记住父弹幕
                }
            }

            // 蓝紫色光源 (#5340ed 风格)
            Lighting.AddLight(Projectile.Center, 0.33f, 0.25f, 0.93f);

            // 蓝紫色粒子尾迹
            if (Main.rand.NextBool(2))
            {
                int dust = Dust.NewDust(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.PurpleTorch,
                    0f, 0f, 100, default, 1.2f
                );
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.2f;
            }
        }

        // 命中敌人：生成 2-3 个自动制导球（外旋命中不会进入此处）
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.myPlayer != Projectile.owner)
                return;

            int orbCount = Main.rand.Next(2, 4); // 2 或 3
            int orbDamage = (int)(Projectile.damage * 0.8f);

            for (int i = 0; i < orbCount; i++)
            {
                // 幽魂式：在敌人中心生成，赋予随机方向、速度 4 的初速，先向外飞离再绕回
                Vector2 vel = Main.rand.NextVector2CircularEdge(4f, 4f);

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    vel,
                    ModContent.ProjectileType<PhantomHomingOrb>(),
                    orbDamage,
                    Projectile.knockBack,
                    Projectile.owner
                );
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = texture.Size() / 2f;

            // 高亮：忽略世界光照
            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                Color.White,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            return false;
        }
    }
}
