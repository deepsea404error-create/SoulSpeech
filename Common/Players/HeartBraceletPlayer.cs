using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;

namespace SoulSpeech.Common.Players
{
    internal class HeartBraceletPlayer : ModPlayer
    {
        private const int CooldownTime = 30; // 0.5 秒
        private int cooldown;

        public bool heartBraceletEquipped;

        public override void ResetEffects()
        {
            heartBraceletEquipped = false;

            if (cooldown > 0)
                cooldown--;
        }

        // ✅ 所有攻击方式都会触发
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!heartBraceletEquipped)
                return;

            if (cooldown > 0)
                return;

            Player.statLife += 2;
            Player.HealEffect(2);

            cooldown = CooldownTime;
        }
    }
}
