using Terraria;
using Terraria.ModLoader;
using SoulSpeech.Content.NPCs.Global;

namespace SoulSpeech.Content.Buffs
{
    // 月蚀：仿破晓（Daybroken, 内部 BUFF ID 189）。本体仅作标志位，
    // 真正的 DoT / 叠层 / 蓝紫色粒子都在 LunarEclipseGlobalNPC 中实现。
    // 最多叠加 8 层，每层 100hp/s（复刻原版破晓公式），持续 3s。
    internal class LunarEclipseDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.pvpBuff[Type] = false;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<LunarEclipseGlobalNPC>().lunarEclipseActive = true;
        }
    }
}
