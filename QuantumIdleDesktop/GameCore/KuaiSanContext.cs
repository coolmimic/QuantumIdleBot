using QuantumIdleDesktop.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuantumIdleDesktop.GameCore
{
    public class KuaiSanContext : MinesweeperContext
    {
        public KuaiSanContext(long groupId, GameType gameType, TelegramGroupModel groupModel) : base(groupId, gameType, groupModel)
        {

        }
    }
}
