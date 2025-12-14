using QuantumIdleDesktop.Models;
using QuantumIdleDesktop.Models.Odds;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Windows.Forms.LinkLabel;

namespace QuantumIdleDesktop.Strategies.OddsRules
{
    public class LinearOddsRule : IOddsRule
    {
        public int GetNextMultiplier(SchemeModel scheme)
        {
            if (scheme.OddsConfig is LinearOddsConfig line)
            {
                if (line.Sequence == null || line.Sequence.Count == 0)
                    return 0;
                if (line.CurrentIndex >= line.Sequence.Count)
                {
                    line.CurrentIndex = 0;
                }
                return line.Sequence[line.CurrentIndex];
            }
            return 0;
        }
        public void UpdateState(SchemeModel scheme, OrderModel lastOrder)
        {
            if (scheme.OddsConfig is LinearOddsConfig line)
            {
                if (line.Sequence == null || line.Sequence.Count == 0) return;

                if (lastOrder.PayoutAmount > 0)
                {
                    line.CurrentIndex = 0;
                }
                else
                {
                    line.CurrentIndex++;
                    if (line.CurrentIndex >= line.Sequence.Count)
                    {
                        line.CurrentIndex = 0;
                    }
                }
            }
        }
    }
}
