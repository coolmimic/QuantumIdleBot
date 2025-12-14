using QuantumIdleDesktop.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuantumIdleDesktop.Strategies.JudgeRules
{
    public interface IJudgeStrategy
    {    
        int Judge(OrderModel oder);
    }
}
