using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulSpeech.Common.Config;
using Terraria.ModLoader;
using Terraria;

namespace SoulSpeech.Common.BaseItems
{
    /// <summary>
    /// 所有受阶段倍率影响的武器基类
    /// </summary>
    public abstract class StageWeapon : ModItem
    {
        /// <summary>
        /// 武器所处阶段（子类必须指定）
        /// </summary>
        protected abstract WeaponStage Stage { get; }

        /// <summary>
        /// 基础伤害（不含倍率）
        /// </summary>
        protected abstract int BaseDamage { get; }

        public override void SetDefaults()
        {
            // 子类负责设置其他属性
            ApplyDamageScaling();
        }

        /// <summary>
        /// 应用全局 + 阶段伤害倍率
        /// </summary>
        protected void ApplyDamageScaling()
        {
            var config = ModContent.GetInstance<WeaponBalanceConfig>();

            float finalMultiplier =
                config.GlobalWeaponDamageMultiplier *
                config.GetStageMultiplier(Stage);

            Item.damage = (int)(BaseDamage * finalMultiplier);
        }
    }
}
