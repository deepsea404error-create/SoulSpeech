using Terraria;
using Terraria.ModLoader;
using SoulSpeech.Content.NPCs.Global;

namespace SoulSpeech.Content.Buffs
{
    // 冥火 buff：每秒流失 20hp，敌人带暗紫色粒子特效。DoT 与粒子在 NetherFlameGlobalNPC 中实现。
    internal class NetherFlameDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.pvpBuff[Type] = false;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<NetherFlameGlobalNPC>().netherFlameActive = true;
        }
    }
}
