using Terraria;
using Terraria.ModLoader;
using SoulSpeech.Content.Projectiles.Summon;

namespace SoulSpeech.Content.Buffs
{
    internal class BlueSkullMinionBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<BlueSkull>()] > 0) {
                player.buffTime[buffIndex] = 18000;
            } else {
                player.DelBuff(buffIndex);
            }
        }
    }
}
