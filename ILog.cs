﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank_gruppprojekt
{
    public interface ILog
    {
        List<string> GetLogBois();
        void LogDeposit(double ammount);
        void LogWithdraw(double ammount);
    }
}