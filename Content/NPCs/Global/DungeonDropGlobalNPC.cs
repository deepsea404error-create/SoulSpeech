using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulSpeech.Content.Items.Materials;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;
using Terraria;

namespace SoulSpeech.Content.NPCs.Global
{

    public class DungeonDropGlobalNPC : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            // 50% 掉落 1 个 魂灵
            int[] halfChanceNPCs =
            {
                31,   // 暗黑法师
                -13,
                -14,
                294,
                295,
                296
            };

            foreach (int id in halfChanceNPCs)
            {
                if (npc.type == id)
                {
                    npcLoot.Add(ItemDropRule.Common(
                        ModContent.ItemType<SoulSpirit>(),
                        chanceDenominator: 2,   // 50% 概率
                        minimumDropped: 1,
                        maximumDropped: 1
                    ));
                    return;
                }
            }

            // 100% 掉落 1 个 魂灵
            if (npc.type == 32 || npc.type == 34)
            {
                npcLoot.Add(ItemDropRule.Common(
                    ModContent.ItemType<SoulSpirit>(),
                    chanceDenominator: 1,   // 必掉
                    minimumDropped: 1,
                    maximumDropped: 1
                ));
            }
        }
    }
}
