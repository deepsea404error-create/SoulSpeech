using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulSpeech.Common
{
    /// <summary>
    /// 武器所处的游戏进度阶段
    /// </summary>
    public enum WeaponStage
    {
        Start,          // 骷髅王前
        Skeletron,         // 骷髅王后 - 肉山前
        MeatMountain,               // 肉山后 - 新三王前
        Mech,              // 新三王后 - 石巨人前
        Golem,             // 石巨人后 - 月总前
        MoonLord           // 月总后
    }
}
