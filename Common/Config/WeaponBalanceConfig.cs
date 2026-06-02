using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace SoulSpeech.Common.Config
{
    public class WeaponBalanceConfig : ModConfig
    {
        // 配置类型：客户端即可（不影响世界）
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Header("全局倍率")]

        [DefaultValue(1.0f)]
        [Range(0.1f, 5.0f)]
        [Increment(0.1f)]
        [Label("全局武器伤害倍率")]
        public float GlobalWeaponDamageMultiplier;

        [Header("阶段倍率")]

        [DefaultValue(1.0f)]
        [Range(0.1f, 5.0f)]
        [Increment(0.1f)]
        [Label("开始-骷髅王前")]
        public float Start;

        [DefaultValue(1.0f)]
        [Range(0.1f, 5.0f)]
        [Increment(0.1f)]
        [Label("骷髅王后-肉山前")]
        public float Skeletron;

        [DefaultValue(1.0f)]
        [Range(0.1f, 5.0f)]
        [Increment(0.1f)]
        [Label("肉山后-新三王前")]
        public float MeatMountain;

        [DefaultValue(1.0f)]
        [Range(0.1f, 5.0f)]
        [Increment(0.1f)]
        [Label("新三王-石巨人前")]
        public float Mech;

        [DefaultValue(1.0f)]
        [Range(0.1f, 5.0f)]
        [Increment(0.1f)]
        [Label("石巨人后-月总前")]
        public float Golem;

        [DefaultValue(1.0f)]
        [Range(0.1f, 5.0f)]
        [Increment(0.1f)]
        [Label("月总后")]
        public float MoonLord;

        /// <summary>
        /// 根据武器阶段获取最终倍率
        /// </summary>
        public float GetStageMultiplier(WeaponStage stage)
        {
            return stage switch
            {
                WeaponStage.Start => Start,
                WeaponStage.Skeletron => Skeletron,
                WeaponStage.MeatMountain => MeatMountain,
                WeaponStage.Mech => Mech,
                WeaponStage.Golem => Golem,
                WeaponStage.MoonLord => MoonLord,
                _ => 1.0f
            };
        }
    }
}
