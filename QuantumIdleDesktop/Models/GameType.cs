using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace QuantumIdleDesktop.Models
{

    /// <summary>
    /// 游戏类型
    /// </summary>
    public enum GameType
    {
        [Description("扫雷")]
        Minesweeper,
        [Description("快三")]
        Kuai3,
        [Description("加拿大28")]
        Canada28,
        [Description("加拿大20")]
        Canada20,
        [Description("时时彩")]
        HashLottery,
    }






    /// <summary>
    /// 游戏玩法
    /// </summary>
    public enum GamePlayMode
    {
        /// <summary>
        /// 大小单双
        /// </summary>
        [Description("大小单双")]
        BigSmallOddEven,
        /// <summary>
        /// 数字
        /// </summary>
        [Description("数字")]
        Digital,
        /// <summary>
        /// 组合
        /// </summary>
        [Description("组合")]
        Combination = 301,

        /// <summary>
        /// 高倍
        /// </summary>
        [Description("高倍")]
        HighOdds = 302,


        // --- 定位类  ---

        /// <summary>
        /// 定位胆 (指定位置猜数字)
        /// </summary>
        [Description("定位胆")]
        PositionDigit = 1100,

        /// <summary>
        /// 定位大小单双 (指定位置猜属性)
        /// </summary>
        [Description("定位大小单双")]
        PositionBigSmallOddEven = 1102,

        // --- 龙虎类 ---

        /// <summary>
        /// 龙虎 
        /// </summary>
        [Description("龙虎")]
        DragonTiger = 1202,

        /// <summary>
        /// 和 
        /// </summary>
        [Description("和")]
        Sum = 1203

    }


}
