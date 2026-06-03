using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using SoulSpeech.Common.Players;
using Terraria.Localization;

namespace SoulSpeech.Content.Items.Accessories
{
    [AutoloadEquip(EquipType.Shield)]
    internal class MechanicalShield : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 30;

            Item.accessory = true;
            Item.defense = 4;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.buyPrice(gold: 4);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // ===== 基础属性 =====
            player.GetDamage(DamageClass.Melee) += 0.66f; // 66% 近战伤害
            player.GetCritChance(DamageClass.Melee) += 4f; // 4% 暴击
            player.GetKnockback(DamageClass.Melee) += 1.5f; // 很强击退

            // ===== 启用机械护盾冲刺 =====
            player.GetModPlayer<MechanicalShieldPlayer>().MechanicalShieldEquipped = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.EoCShield) // 克苏鲁之盾
                .AddIngredient(3354) // 机械车轮片
                .AddIngredient(3355) // 机械车体片
                .AddIngredient(3356) // 机械车池片
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}