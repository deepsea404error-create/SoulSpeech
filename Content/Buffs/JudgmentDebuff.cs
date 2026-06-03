using Terraria;
using Terraria.ModLoader;
using SoulSpeech.Content.NPCs.Global;

namespace SoulSpeech.Content.Buffs
{
    // 裁决 buff：每秒流失 50hp，敌人带金色粒子特效。DoT 与粒子在 JudgmentGlobalNPC 中实现。
    internal class JudgmentDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.pvpBuff[Type] = false;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<JudgmentGlobalNPC>().judgmentActive = true;
        }
    }
}
