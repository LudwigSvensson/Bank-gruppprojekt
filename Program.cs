﻿using Bank_gruppprojekt;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BankApplication
{
    public class Program
    {
        static void Main(string[] args)
        {
            LogClass log = new LogClass();
            LogIn.LoginIn(log);           
        }        
    }
}
