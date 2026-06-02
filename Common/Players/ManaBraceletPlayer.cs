using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;

namespace SoulSpeech.Common.Players
{
    internal class ManaBraceletPlayer : ModPlayer
    {
        private const int CooldownTime = 30;
        private int cooldown;

        public bool manaBraceletEquipped;

        public override void ResetEffects()
        {
            manaBraceletEquipped = false;

            if (cooldown > 0)
                cooldown--;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!manaBraceletEquipped)
                return;

            if (cooldown > 0)
                return;

            Player.statMana += 5;
            Player.ManaEffect(5);

            cooldown = CooldownTime;
        }
    }
}
